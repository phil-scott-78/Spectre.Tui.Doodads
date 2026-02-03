using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads;

/// <summary>
/// TEA program runner. Manages the terminal, event loop, and renders doodads.
/// </summary>
public static class Program
{
    /// <summary>
    /// Runs a TEA program with the given initial model.
    /// </summary>
    /// <typeparam name="TModel">The doodad model type.</typeparam>
    /// <param name="initialModel">The initial model state.</param>
    /// <param name="configure">Optional configuration callback.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The final model state when the program exits.</returns>
    public static async Task<TModel> RunAsync<TModel>(
        TModel initialModel,
        Action<ProgramOptions>? configure = null,
        CancellationToken cancellationToken = default)
        where TModel : IDoodad<TModel>
    {
        var options = new ProgramOptions();
        configure?.Invoke(options);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, options.CancellationToken);

        var mode = options.TerminalMode ?? new FullscreenMode();
        var ownsTerminal = options.Terminal is null;
        var terminal = options.Terminal ?? Terminal.Create(mode);

        // Auto-size inline mode based on model dimensions
        if (mode is InlineMode inlineMode && initialModel is ISizedRenderable sized)
        {
            inlineMode.SetHeight(sized.MinHeight);
        }

        try
        {
            if (!ownsTerminal)
            {
                terminal.Clear();
            }

            var renderer = new Renderer(terminal);
            renderer.SetTargetFps(options.TargetFps);

            using var inputReader = new ConsoleInputReader();
            var messageQueue = Channel.CreateUnbounded<Message>(
                new UnboundedChannelOptions { SingleReader = true });

            var model = initialModel;
            TModel? lastRenderedModel = default;

            // Run Init
            var initCmd = model.Init();
            if (initCmd is not null)
            {
                ScheduleCommand(initCmd, messageQueue.Writer, cts.Token);
            }

            // Bootstrap: trigger initial render by posting ReadyMessage
            // This ensures the first loop iteration runs even if Init() returned null
            // and no input has arrived yet
            await messageQueue.Writer.WriteAsync(new ReadyMessage(), cts.Token).ConfigureAwait(false);

            // Start input pump - posts input events to the unified channel
            var inputPumpTask = RunInputPumpAsync(inputReader, messageQueue.Writer, cts.Token);

            // Event-driven main loop - wakes only when messages arrive
            try
            {
                await foreach (var message in messageQueue.Reader.ReadAllAsync(cts.Token)
                    .ConfigureAwait(false))
                {
                    // Force re-render when terminal is resized — viewport dimensions changed
                    // even if the model itself didn't change
                    if (message is WindowSizeMessage)
                    {
                        lastRenderedModel = default;
                    }

                    var (updated, shouldQuit) = ProcessMessage(model, message, messageQueue.Writer, cts.Token);
                    model = updated;

                    if (shouldQuit)
                    {
                        break;
                    }

                    // Render when model has changed
                    if (!EqualityComparer<TModel>.Default.Equals(model, lastRenderedModel))
                    {
                        var currentModel = model;
                        var rendered = false;
                        renderer.Draw((context, _) =>
                        {
                            IRenderSurface BuildSurface(RenderContext ctx)
                            {
                                IRenderSurface s = new RenderContextSurface(ctx, BuildSurface);
                                foreach (var stage in options.RenderPipeline)
                                {
                                    s = stage(s);
                                }

                                return s;
                            }

                            currentModel.View(BuildSurface(context));
                            rendered = true;
                        });

                        if (rendered)
                        {
                            lastRenderedModel = currentModel;
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                // Expected on shutdown
            }

            // Stop input pump and wait for it to complete
            await cts.CancelAsync().ConfigureAwait(false);
            messageQueue.Writer.Complete();

            try
            {
                await inputPumpTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }

            return model;
        }
        finally
        {
            if (ownsTerminal)
            {
                terminal.Dispose();
            }
        }
    }

    private static (TModel Model, bool ShouldQuit) ProcessMessage<TModel>(
        TModel model,
        Message message,
        ChannelWriter<Message> writer,
        CancellationToken ct)
        where TModel : IDoodad<TModel>
    {
        switch (message)
        {
            case BatchMessage batch:
                var batchQuit = false;
                foreach (var msg in batch.Messages)
                {
                    var (batchModel, quit) = ProcessMessage(model, msg, writer, ct);
                    model = batchModel;
                    batchQuit |= quit;
                }

                return (model, batchQuit);

            case SequenceMessage seq:
                var seqQuit = false;
                if (seq.StepMessage is not null)
                {
                    (model, seqQuit) = ProcessMessage(model, seq.StepMessage, writer, ct);
                }

                if (!seqQuit && seq.Remaining.Count > 0)
                {
                    var nextCmd = Commands.Sequence(
                        seq.Remaining.Select(c => (Command?)c).ToArray());
                    if (nextCmd is not null)
                    {
                        ScheduleCommand(nextCmd, writer, ct);
                    }
                }

                return (model, seqQuit);

            default:
                var isQuit = message is QuitMessage;
                var (updatedModel, cmd) = model.Update(message);
                if (cmd is not null && !isQuit)
                {
                    ScheduleCommand(cmd, writer, ct);
                }

                return (updatedModel, isQuit);
        }
    }

    private static void ScheduleCommand(
        Command command,
        ChannelWriter<Message> writer,
        CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var result = await command(ct).ConfigureAwait(false);
                if (result is not null)
                {
                    await writer.WriteAsync(result, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            catch (Exception ex)
            {
                try
                {
                    await writer.WriteAsync(
                        new CommandErrorMessage { Exception = ex },
                        CancellationToken.None).ConfigureAwait(false);
                }
                catch (ChannelClosedException)
                {
                    // Channel already closed during shutdown
                }
            }
        }, ct);
    }

    private static async Task RunInputPumpAsync(
        IInputReader reader,
        ChannelWriter<Message> writer,
        CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var message = await reader.ReadAsync(ct).ConfigureAwait(false);
                if (message is not null)
                {
                    await writer.WriteAsync(message, ct).ConfigureAwait(false);
                }
                else
                {
                    // No input available - small delay before next poll
                    await Task.Delay(10, ct).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
        catch (ChannelClosedException)
        {
            // Channel closed during shutdown
        }
    }
}
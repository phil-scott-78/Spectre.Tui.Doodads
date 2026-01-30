using System.Text;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests;

/// <summary>
/// Test harness for doodad models. Provides convenience methods for sending messages
/// and asserting on model state.
/// </summary>
/// <typeparam name="TModel">The concrete doodad model type.</typeparam>
public sealed class DoodadFixture<TModel>
    where TModel : IDoodad<TModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DoodadFixture{TModel}"/> class.
    /// </summary>
    /// <param name="model">The initial doodad model.</param>
    /// <param name="size">The terminal size for rendering. Defaults to 80x24.</param>
    public DoodadFixture(TModel model, Size? size = null)
    {
        Model = model;
        Size = size ?? new Size(80, 24);
    }

    /// <summary>
    /// Gets the current doodad model.
    /// </summary>
    public TModel Model { get; private set; }

    /// <summary>
    /// Gets the terminal size used for rendering.
    /// </summary>
    public Size Size { get; }

    /// <summary>
    /// Runs <see cref="IDoodad{TSelf}.Init"/> on the model and processes the resulting
    /// command synchronously.
    /// </summary>
    /// <returns>This fixture for fluent chaining.</returns>
    public DoodadFixture<TModel> Init()
    {
        var cmd = Model.Init();
        if (cmd is not null)
        {
            ProcessCommand(cmd);
        }

        return this;
    }

    /// <summary>
    /// Sends a message to the model via <see cref="IDoodad{TSelf}.Update"/> and applies
    /// the resulting state change.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>This fixture for fluent chaining.</returns>
    public DoodadFixture<TModel> Send(Message message)
    {
        var (updated, cmd) = Model.Update(message);
        Model = updated;
        if (cmd is not null)
        {
            ProcessCommand(cmd);
        }

        return this;
    }

    /// <summary>
    /// Sends a key message to the model.
    /// </summary>
    /// <param name="key">The key to send.</param>
    /// <param name="alt">Whether the Alt modifier is held.</param>
    /// <param name="shift">Whether the Shift modifier is held.</param>
    /// <param name="ctrl">Whether the Ctrl modifier is held.</param>
    /// <returns>This fixture for fluent chaining.</returns>
    public DoodadFixture<TModel> SendKey(Key key, bool alt = false, bool shift = false, bool ctrl = false)
    {
        return Send(new KeyMessage { Key = key, Runes = [], Alt = alt, Shift = shift, Ctrl = ctrl });
    }

    /// <summary>
    /// Sends a character key message to the model.
    /// </summary>
    /// <param name="c">The character to send.</param>
    /// <returns>This fixture for fluent chaining.</returns>
    public DoodadFixture<TModel> SendChar(char c)
    {
        return Send(new KeyMessage { Key = Key.Char, Runes = [new Rune(c)] });
    }

    /// <summary>
    /// Sends multiple key messages to the model in sequence.
    /// </summary>
    /// <param name="keys">The keys to send.</param>
    /// <returns>This fixture for fluent chaining.</returns>
    public DoodadFixture<TModel> SendKeys(params Key[] keys)
    {
        foreach (var key in keys)
        {
            SendKey(key);
        }

        return this;
    }

    /// <summary>
    /// Renders the current model using a <see cref="TuiFixture"/> and returns the output
    /// as a string grid.
    /// </summary>
    /// <returns>A string representation of the rendered output where empty cells are shown as a bullet character.</returns>
    public string Render()
    {
        var tuiFixture = new TuiFixture(Size);
        return tuiFixture.Render(Model);
    }

    /// <summary>
    /// Processes a command synchronously. Executes the command and feeds the result message
    /// back through the message processing pipeline.
    /// </summary>
    /// <param name="cmd">The command to process.</param>
    /// <param name="depth">Recursion depth to prevent infinite tick loops.</param>
    private void ProcessCommand(Command cmd, int depth = 0)
    {
        if (depth >= 10)
        {
            return;
        }

        try
        {
            var result = cmd(CancellationToken.None).GetAwaiter().GetResult();
            if (result is not null)
            {
                ProcessCommandResult(result, depth);
            }
        }
        catch (TaskCanceledException)
        {
            // Tick commands use Task.Delay which may throw if cancelled - safe to ignore in tests.
        }
        catch (OperationCanceledException)
        {
            // Same as above.
        }
    }

    /// <summary>
    /// Processes a message result from a command, handling batch and sequence messages
    /// the same way the program runner does.
    /// </summary>
    /// <param name="result">The message to process.</param>
    /// <param name="depth">Recursion depth to prevent infinite tick loops.</param>
    private void ProcessCommandResult(Message result, int depth)
    {
        switch (result)
        {
            case BatchMessage batch:
                foreach (var msg in batch.Messages)
                {
                    ProcessCommandResult(msg, depth);
                }

                break;

            case SequenceMessage seq:
                if (seq.StepMessage is not null)
                {
                    ProcessCommandResult(seq.StepMessage, depth);
                }

                foreach (var remaining in seq.Remaining)
                {
                    ProcessCommand(remaining, depth + 1);
                }

                break;

            default:
                var (updated, cmd) = Model.Update(result);
                Model = updated;
                if (cmd is not null)
                {
                    ProcessCommand(cmd, depth + 1);
                }

                break;
        }
    }
}
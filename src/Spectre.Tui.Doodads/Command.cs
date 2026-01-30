namespace Spectre.Tui.Doodads;

/// <summary>
/// An async function that produces a message. Commands represent side effects
/// in the TEA architecture.
/// </summary>
/// <param name="cancellationToken">A token to cancel the command.</param>
/// <returns>A message produced by the command, or null.</returns>
public delegate Task<Message?> Command(CancellationToken cancellationToken);

/// <summary>
/// Provides factory methods for creating commands.
/// </summary>
public static class Commands
{
    /// <summary>
    /// Creates a command that immediately produces a message.
    /// </summary>
    public static Command Message(Message message)
    {
        return _ => Task.FromResult<Message?>(message);
    }

    /// <summary>
    /// Batches multiple commands to run concurrently. All resulting messages
    /// are collected into a <see cref="BatchMessage"/>.
    /// </summary>
    public static Command? Batch(params Command?[] commands)
    {
        var filtered = commands.Where(c => c is not null).Cast<Command>().ToArray();
        if (filtered.Length == 0)
        {
            return null;
        }

        if (filtered.Length == 1)
        {
            return filtered[0];
        }

        return async ct =>
        {
            var tasks = filtered.Select(async c =>
            {
                try
                {
                    return await c(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    return new CommandErrorMessage { Exception = ex };
                }
            });

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            var messages = results.Where(m => m is not null).Cast<Message>().ToList();
            return messages.Count switch
            {
                0 => null,
                1 => messages[0],
                _ => new BatchMessage { Messages = messages },
            };
        };
    }

    /// <summary>
    /// Sequences multiple commands to run one after another.
    /// Each result is fed back through Update before the next command runs.
    /// </summary>
    public static Command? Sequence(params Command?[] commands)
    {
        var filtered = commands.Where(c => c is not null).Cast<Command>().ToArray();
        if (filtered.Length == 0)
        {
            return null;
        }

        if (filtered.Length == 1)
        {
            return filtered[0];
        }

        return async ct =>
        {
            Message? first;
            try
            {
                first = await filtered[0](ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                first = new CommandErrorMessage { Exception = ex };
            }

            var remaining = filtered.Skip(1).ToList();
            return new SequenceMessage { StepMessage = first, Remaining = remaining };
        };
    }

    /// <summary>
    /// Creates a command that waits for the specified interval, then produces a message.
    /// Used for animations (spinner frames, progress, timers).
    /// </summary>
    public static Command Tick(TimeSpan interval, Func<DateTimeOffset, Message> createMessage)
    {
        return async ct =>
        {
            await Task.Delay(interval, ct).ConfigureAwait(false);
            return createMessage(DateTimeOffset.UtcNow);
        };
    }

    /// <summary>
    /// Creates a command that produces a <see cref="QuitMessage"/>.
    /// </summary>
    public static Command Quit()
    {
        return Message(new QuitMessage());
    }
}
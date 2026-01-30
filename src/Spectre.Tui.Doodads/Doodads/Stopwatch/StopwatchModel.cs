namespace Spectre.Tui.Doodads.Doodads.Stopwatch;

/// <summary>
/// An elapsed time counter that ticks upward at a configurable interval.
/// </summary>
public record StopwatchModel : IDoodad<StopwatchModel>, ISizedRenderable
{
    private static int _nextId;

    /// <summary>
    /// Gets the minimum display width of the stopwatch.
    /// </summary>
    public int MinWidth { get; init; } = 10;

    /// <summary>
    /// Gets the minimum display height of the stopwatch (always 1).
    /// </summary>
    public int MinHeight { get; init; } = 1;

    /// <summary>
    /// Gets the total elapsed time.
    /// </summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>
    /// Gets the tick interval.
    /// </summary>
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets a value indicating whether the stopwatch is currently running.
    /// </summary>
    public bool Running { get; init; }

    /// <summary>
    /// Gets the unique identifier for this stopwatch instance.
    /// </summary>
    internal int Id { get; init; } = Interlocked.Increment(ref _nextId);

    /// <summary>
    /// Gets the generation tag for stale tick detection.
    /// </summary>
    internal int Tag { get; init; }

    /// <inheritdoc />
    public Command? Init()
    {
        if (Running)
        {
            return TickCommand();
        }

        return null;
    }

    /// <inheritdoc />
    public (StopwatchModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case StopwatchTickMessage tick when tick.Id == Id && tick.Tag == Tag && Running:
                var updated = this with { Elapsed = Elapsed + Interval };
                return (updated, updated.TickCommand());

            case StopwatchStartStopMessage startStop when startStop.Id == Id:
                if (startStop.Running)
                {
                    return Start();
                }

                return Stop();

            case StopwatchResetMessage reset when reset.Id == Id:
                return Reset();

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var totalSeconds = (int)Elapsed.TotalSeconds;
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        var text = Elapsed.TotalHours >= 1
            ? $"{(int)Elapsed.TotalHours}:{minutes % 60:D2}:{seconds:D2}"
            : $"{minutes}:{seconds:D2}";

        surface.SetString(0, 0, text, Appearance.Plain);
    }

    /// <summary>
    /// Starts the stopwatch, scheduling the next tick.
    /// </summary>
    /// <returns>The updated model and a tick command.</returns>
    public (StopwatchModel Model, Command? Command) Start()
    {
        var started = this with { Running = true, Tag = Tag + 1 };
        return (started, started.TickCommand());
    }

    /// <summary>
    /// Stops the stopwatch, invalidating pending ticks.
    /// </summary>
    /// <returns>The updated model with no command.</returns>
    public (StopwatchModel Model, Command? Command) Stop()
    {
        return (this with { Running = false, Tag = Tag + 1 }, null);
    }

    /// <summary>
    /// Toggles the stopwatch between running and stopped states.
    /// </summary>
    /// <returns>The updated model and an optional command.</returns>
    public (StopwatchModel Model, Command? Command) Toggle()
    {
        return Running ? Stop() : Start();
    }

    /// <summary>
    /// Resets the elapsed time to zero and stops the stopwatch.
    /// </summary>
    /// <returns>The updated model with no command.</returns>
    public (StopwatchModel Model, Command? Command) Reset()
    {
        return (this with { Elapsed = TimeSpan.Zero, Running = false, Tag = Tag + 1 }, null);
    }

    private Command TickCommand()
    {
        var id = Id;
        var tag = Tag;
        return Commands.Tick(Interval, _ => new StopwatchTickMessage { Id = id, Tag = tag });
    }
}

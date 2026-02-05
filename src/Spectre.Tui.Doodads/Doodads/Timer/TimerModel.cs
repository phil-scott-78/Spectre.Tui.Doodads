namespace Spectre.Tui.Doodads.Doodads.Timer;

/// <summary>
/// A countdown timer that ticks at a configurable interval and emits
/// a <see cref="TimerTimeoutMessage"/> when the countdown reaches zero.
/// </summary>
public record TimerModel : IDoodad<TimerModel>, ISizedRenderable
{
    /// <summary>
    /// Gets the minimum display width of the timer.
    /// </summary>
    public int MinWidth { get; init; } = 10;

    /// <summary>
    /// Gets the minimum display height of the timer (always 1).
    /// </summary>
    public int MinHeight { get; init; } = 1;

    /// <summary>
    /// Gets the remaining time on the countdown.
    /// </summary>
    public TimeSpan Timeout { get; init; }

    /// <summary>
    /// Gets the tick interval.
    /// </summary>
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets a value indicating whether the timer is currently running.
    /// </summary>
    public bool Running { get; init; }

    /// <summary>
    /// Gets a value indicating whether the timer has timed out (reached zero).
    /// </summary>
    public bool Timedout { get; init; }

    /// <summary>
    /// Gets the tick source for stale tick detection.
    /// </summary>
    internal TickSource Ticks { get; init; } = new();

    /// <inheritdoc />
    public Command? Init()
    {
        if (Running)
        {
            return Ticks.CreateTick(Interval);
        }

        return null;
    }

    /// <inheritdoc />
    public (TimerModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case TickMessage tick when Ticks.IsValid(tick) && Running:
                var remaining = Timeout - Interval;
                if (remaining <= TimeSpan.Zero)
                {
                    var finished = this with { Timeout = TimeSpan.Zero, Running = false, Timedout = true, Ticks = Ticks.Advance() };
                    return (finished, Commands.Message(new TimerTimeoutMessage { Id = Ticks.Id }));
                }

                var updated = this with { Timeout = remaining };
                return (updated, updated.Ticks.CreateTick(Interval));

            case TimerStartStopMessage startStop when startStop.Id == Ticks.Id:
                if (startStop.Running)
                {
                    return Start();
                }

                return Stop();

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var totalSeconds = (int)Math.Ceiling(Timeout.TotalSeconds);
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;

        var text = Timeout.TotalHours >= 1
            ? $"{(int)Timeout.TotalHours}:{minutes % 60:D2}:{seconds:D2}"
            : $"{minutes}:{seconds:D2}";

        surface.SetString(0, 0, text, Appearance.Plain);
    }

    /// <summary>
    /// Starts the timer, scheduling the next tick.
    /// </summary>
    /// <returns>The updated model and a tick command.</returns>
    public (TimerModel Model, Command? Command) Start()
    {
        var started = this with { Running = true, Ticks = Ticks.Advance() };
        return (started, started.Ticks.CreateTick(Interval));
    }

    /// <summary>
    /// Stops the timer, invalidating pending ticks.
    /// </summary>
    /// <returns>The updated model with no command.</returns>
    public (TimerModel Model, Command? Command) Stop()
    {
        return (this with { Running = false, Ticks = Ticks.Advance() }, null);
    }

    /// <summary>
    /// Toggles the timer between running and stopped states.
    /// </summary>
    /// <returns>The updated model and an optional command.</returns>
    public (TimerModel Model, Command? Command) Toggle()
    {
        return Running ? Stop() : Start();
    }

    /// <summary>
    /// Resets the timer to the specified timeout, stopping it.
    /// </summary>
    /// <param name="timeout">The new countdown duration.</param>
    /// <returns>The updated model with no command.</returns>
    public (TimerModel Model, Command? Command) Reset(TimeSpan timeout)
    {
        return (this with { Timeout = timeout, Running = false, Timedout = false, Ticks = Ticks.Advance() }, null);
    }
}

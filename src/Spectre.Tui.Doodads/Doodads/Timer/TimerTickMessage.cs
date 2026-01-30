namespace Spectre.Tui.Doodads.Doodads.Timer;

/// <summary>
/// Message for advancing the countdown timer.
/// </summary>
internal record TimerTickMessage : Message
{
    public required int Id { get; init; }
    public required int Tag { get; init; }
}

/// <summary>
/// Message for starting or stopping the timer.
/// </summary>
internal record TimerStartStopMessage : Message
{
    public required int Id { get; init; }
    public required bool Running { get; init; }
}

/// <summary>
/// Message emitted when the countdown timer reaches zero.
/// </summary>
public record TimerTimeoutMessage : Message
{
    /// <summary>
    /// Gets the identifier of the timer that timed out.
    /// </summary>
    public required int Id { get; init; }
}
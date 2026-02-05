namespace Spectre.Tui.Doodads.Doodads.Stopwatch;

/// <summary>
/// Message for starting or stopping the stopwatch.
/// </summary>
internal record StopwatchStartStopMessage : Message
{
    public required int Id { get; init; }
    public required bool Running { get; init; }
}

/// <summary>
/// Message for resetting the stopwatch.
/// </summary>
internal record StopwatchResetMessage : Message
{
    public required int Id { get; init; }
}

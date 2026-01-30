namespace Spectre.Tui.Doodads.Doodads.Stopwatch;

/// <summary>
/// Message for advancing the stopwatch elapsed time.
/// </summary>
internal record StopwatchTickMessage : Message
{
    public required int Id { get; init; }
    public required int Tag { get; init; }
}

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
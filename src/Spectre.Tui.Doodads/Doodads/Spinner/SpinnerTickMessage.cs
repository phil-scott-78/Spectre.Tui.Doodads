namespace Spectre.Tui.Doodads.Doodads.Spinner;

/// <summary>
/// Message for advancing spinner animation frames.
/// </summary>
internal record SpinnerTickMessage : Message
{
    public required int Id { get; init; }
    public required int Tag { get; init; }
}
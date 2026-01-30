namespace Spectre.Tui.Doodads.Doodads.Progress;

/// <summary>
/// Message for advancing progress bar animation frames.
/// </summary>
internal record ProgressFrameMessage : Message
{
    public required int Id { get; init; }
}
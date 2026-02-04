namespace Spectre.Tui.Doodads.Doodads.Cursor;

/// <summary>
/// Message signaling the cursor should transition to idle state and resume blinking.
/// </summary>
internal record CursorIdleMessage : Message
{
    public required int Id { get; init; }
    public required int Tag { get; init; }
}

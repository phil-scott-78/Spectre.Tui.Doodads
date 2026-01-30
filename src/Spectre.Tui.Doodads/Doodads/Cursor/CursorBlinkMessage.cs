namespace Spectre.Tui.Doodads.Doodads.Cursor;

/// <summary>
/// Message for cursor blink toggling.
/// </summary>
internal record CursorBlinkMessage : Message
{
    public required int Id { get; init; }
    public required int Tag { get; init; }
}
namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// A keyboard input message.
/// </summary>
public record KeyMessage : Message
{
    /// <summary>
    /// Gets the key that was pressed.
    /// </summary>
    public required Key Key { get; init; }

    /// <summary>
    /// Gets the runes (typed characters) associated with this key event.
    /// </summary>
    public Rune[] Runes { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the Alt modifier was held.
    /// </summary>
    public bool Alt { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Shift modifier was held.
    /// </summary>
    public bool Shift { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Ctrl modifier was held.
    /// </summary>
    public bool Ctrl { get; init; }
}
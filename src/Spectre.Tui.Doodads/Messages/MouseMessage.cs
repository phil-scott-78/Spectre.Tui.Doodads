namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// Represents the mouse action type.
/// </summary>
public enum MouseAction
{
    Press,
    Release,
    Motion,
    WheelUp,
    WheelDown,
}

/// <summary>
/// Represents the mouse button.
/// </summary>
public enum MouseButton
{
    None,
    Left,
    Right,
    Middle,
}

/// <summary>
/// A mouse input message.
/// </summary>
public record MouseMessage : Message
{
    /// <summary>
    /// Gets the X coordinate of the mouse event.
    /// </summary>
    public required int X { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the mouse event.
    /// </summary>
    public required int Y { get; init; }

    /// <summary>
    /// Gets the mouse action.
    /// </summary>
    public required MouseAction Action { get; init; }

    /// <summary>
    /// Gets the mouse button.
    /// </summary>
    public MouseButton Button { get; init; }
}
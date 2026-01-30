namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// A terminal resize message.
/// </summary>
public record WindowSizeMessage : Message
{
    /// <summary>
    /// Gets the new terminal width.
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Gets the new terminal height.
    /// </summary>
    public required int Height { get; init; }
}
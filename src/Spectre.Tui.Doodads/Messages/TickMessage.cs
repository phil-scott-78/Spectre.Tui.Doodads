namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// A generic tick message used by animated components.
/// </summary>
public record TickMessage : Message
{
    /// <summary>
    /// Gets the time of the tick.
    /// </summary>
    public required DateTimeOffset Time { get; init; }

    /// <summary>
    /// Gets the owner identifier for routing.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the tag for stale tick detection.
    /// </summary>
    public required int Tag { get; init; }
}
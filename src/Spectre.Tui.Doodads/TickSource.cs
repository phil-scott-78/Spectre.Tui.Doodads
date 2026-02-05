namespace Spectre.Tui.Doodads;

/// <summary>
/// Encapsulates the Id + Tag stale-tick detection pattern used by animated components.
/// Each <see cref="TickSource"/> has a unique <see cref="Id"/> and a monotonically
/// increasing <see cref="Tag"/>. Call <see cref="Advance"/> when state changes to
/// invalidate in-flight ticks, and <see cref="IsValid(TickMessage)"/> to guard
/// incoming tick messages.
/// </summary>
public record TickSource
{
    private static int _nextId;

    /// <summary>
    /// Gets the unique identifier for this tick source.
    /// </summary>
    public int Id { get; init; } = Interlocked.Increment(ref _nextId);

    /// <summary>
    /// Gets the generation tag. Incremented on state changes to invalidate pending ticks.
    /// </summary>
    public int Tag { get; init; }

    /// <summary>
    /// Returns a new <see cref="TickSource"/> with <see cref="Tag"/> incremented by one,
    /// preserving <see cref="Id"/>.
    /// </summary>
    public TickSource Advance() => this with { Tag = Tag + 1 };

    /// <summary>
    /// Returns <c>true</c> if the tick message matches this source's <see cref="Id"/>
    /// and <see cref="Tag"/>.
    /// </summary>
    public bool IsValid(TickMessage tick) => tick.Id == Id && tick.Tag == Tag;

    /// <summary>
    /// Returns <c>true</c> if the tick message matches this source's <see cref="Id"/>,
    /// <see cref="Tag"/>, and the specified <paramref name="kind"/>.
    /// </summary>
    public bool IsValid(TickMessage tick, string kind) =>
        tick.Id == Id && tick.Tag == Tag && tick.Kind == kind;

    /// <summary>
    /// Creates a <see cref="Command"/> that waits for the specified interval then produces
    /// a <see cref="TickMessage"/> with this source's current <see cref="Id"/> and <see cref="Tag"/>.
    /// </summary>
    public Command CreateTick(TimeSpan interval, string? kind = null)
    {
        var id = Id;
        var tag = Tag;
        return Commands.Tick(interval, time => new TickMessage { Time = time, Id = id, Tag = tag, Kind = kind });
    }
}

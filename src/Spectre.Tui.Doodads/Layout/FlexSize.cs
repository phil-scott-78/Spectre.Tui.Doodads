namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// Defines how a flex item is sized within a <see cref="FlexRow"/> or <see cref="FlexColumn"/>.
/// </summary>
public abstract record FlexSize
{
    private protected FlexSize()
    {
    }

    /// <summary>
    /// Creates a fixed-size item that takes exactly <paramref name="characters"/> cells.
    /// </summary>
    /// <param name="characters">The exact size in terminal cells.</param>
    /// <returns>A fixed-size <see cref="FlexSize"/>.</returns>
    public static FlexSize Fixed(int characters) => new FixedSize(Math.Max(0, characters));

    /// <summary>
    /// Creates a ratio-sized item that takes a proportional share of remaining space.
    /// </summary>
    /// <param name="weight">The weight for proportional distribution (minimum 1).</param>
    /// <returns>A ratio-based <see cref="FlexSize"/>.</returns>
    public static FlexSize Ratio(int weight) => new RatioSize(Math.Max(1, weight));

    /// <summary>
    /// Creates a fill-sized item equivalent to <c>Ratio(1)</c>.
    /// </summary>
    /// <returns>A ratio-based <see cref="FlexSize"/> with weight 1.</returns>
    public static FlexSize Fill() => new RatioSize(1);

    /// <summary>
    /// A fixed-size strategy that allocates an exact number of terminal cells.
    /// </summary>
    /// <param name="Characters">The exact size in terminal cells.</param>
    internal sealed record FixedSize(int Characters) : FlexSize;

    /// <summary>
    /// A ratio-based strategy that distributes remaining space proportionally by weight.
    /// </summary>
    /// <param name="Weight">The proportional weight (minimum 1).</param>
    internal sealed record RatioSize(int Weight) : FlexSize;
}
namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// Factory methods for creating flex layout containers.
/// </summary>
public static class Flex
{
    /// <summary>
    /// Creates a new horizontal flex layout container.
    /// </summary>
    /// <param name="minWidth">The minimum width in columns.</param>
    /// <param name="minHeight">The minimum height in rows.</param>
    /// <param name="gap">The gap between items in columns.</param>
    /// <returns>A new <see cref="FlexRow"/>.</returns>
    public static FlexRow Row(int minWidth = 0, int minHeight = 0, int gap = 0)
    {
        return new FlexRow { MinWidthOverride = minWidth, MinHeightOverride = minHeight, Gap = gap };
    }

    /// <summary>
    /// Creates a new vertical flex layout container.
    /// </summary>
    /// <param name="minWidth">The minimum width in columns.</param>
    /// <param name="minHeight">The minimum height in rows.</param>
    /// <param name="gap">The gap between items in rows.</param>
    /// <returns>A new <see cref="FlexColumn"/>.</returns>
    public static FlexColumn Column(int minWidth = 0, int minHeight = 0, int gap = 0)
    {
        return new FlexColumn { MinWidthOverride = minWidth, MinHeightOverride = minHeight, Gap = gap };
    }
}
namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// A horizontal flex layout container that distributes width among child renderables.
/// Implements <see cref="ISizedRenderable"/> so it can be nested and used in layout templates.
/// </summary>
public record FlexRow : ISizedRenderable
{
    /// <summary>
    /// Gets the minimum width of the container in columns.
    /// </summary>
    public int MinWidth => Math.Max(MinWidthOverride, CalculateContentMinWidth());

    /// <summary>
    /// Gets the minimum height of the container in rows.
    /// </summary>
    public int MinHeight => Math.Max(MinHeightOverride, CalculateContentMinHeight());

    /// <summary>
    /// Gets the minimum width override in columns.
    /// </summary>
    public int MinWidthOverride { get; init; }

    /// <summary>
    /// Gets the minimum height override in rows.
    /// </summary>
    public int MinHeightOverride { get; init; }

    /// <summary>
    /// Gets the gap between items in columns.
    /// </summary>
    public int Gap { get; init; }

    /// <summary>
    /// Gets the flex items in this row.
    /// </summary>
    internal ImmutableArray<FlexItem> Items { get; init; } = [];

    /// <summary>
    /// Adds a widget with the specified sizing strategy.
    /// </summary>
    /// <param name="widget">The renderable to add.</param>
    /// <param name="size">The sizing strategy (defaults to <see cref="FlexSize.Fill"/>).</param>
    /// <returns>A new <see cref="FlexRow"/> with the item appended.</returns>
    public FlexRow Add(ISizedRenderable widget, FlexSize? size = null)
    {
        return this with { Items = Items.Add(new FlexItem(widget, size ?? FlexSize.Fill())) };
    }

    /// <inheritdoc />
    public void Render(IRenderSurface surface)
    {
        if (Items.IsEmpty)
        {
            return;
        }

        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        var minSizes = GetItemMinWidths();
        var sizes = FlexAlgorithm.Distribute(width, Gap, Items, minSizes);
        var x = 0;

        for (var i = 0; i < Items.Length; i++)
        {
            var allocatedWidth = sizes[i];
            if (allocatedWidth > 0)
            {
                surface.Render(Items[i].Widget, new Rectangle(x, 0, allocatedWidth, height));
            }

            x += allocatedWidth + Gap;
        }
    }

    private int CalculateContentMinWidth()
    {
        if (Items.IsEmpty)
        {
            return 0;
        }

        var total = 0;
        for (var i = 0; i < Items.Length; i++)
        {
            total += GetItemMinWidth(Items[i]);
        }

        total += Gap * (Items.Length - 1);
        return total;
    }

    private int CalculateContentMinHeight()
    {
        if (Items.IsEmpty)
        {
            return 0;
        }

        var max = 0;
        for (var i = 0; i < Items.Length; i++)
        {
            max = Math.Max(max, Items[i].Widget.MinHeight);
        }

        return max;
    }

    private int[] GetItemMinWidths()
    {
        var result = new int[Items.Length];
        for (var i = 0; i < Items.Length; i++)
        {
            result[i] = GetItemMinWidth(Items[i]);
        }

        return result;
    }

    private static int GetItemMinWidth(FlexItem item)
    {
        var min = item.Widget.MinWidth;
        if (item.Size is FlexSize.FixedSize fixedSize)
        {
            min = Math.Max(min, fixedSize.Characters);
        }

        return min;
    }
}

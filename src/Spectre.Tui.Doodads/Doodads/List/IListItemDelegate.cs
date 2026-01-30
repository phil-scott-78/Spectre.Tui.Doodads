namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// Controls how list items are rendered, including their height and spacing.
/// </summary>
/// <typeparam name="TItem">The type of list item to render.</typeparam>
public interface IListItemDelegate<in TItem>
    where TItem : IListItem
{
    /// <summary>
    /// Gets the height in rows of each rendered item.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets the number of blank rows between items.
    /// </summary>
    int Spacing { get; }

    /// <summary>
    /// Renders a single list item.
    /// </summary>
    /// <param name="surface">The render surface to draw into.</param>
    /// <param name="item">The item to render.</param>
    /// <param name="index">The index of the item in the visible list.</param>
    /// <param name="selected">Whether this item is currently selected.</param>
    void Render(IRenderSurface surface, TItem item, int index, bool selected);
}

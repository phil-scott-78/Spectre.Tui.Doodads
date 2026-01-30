namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// Represents an item that can be displayed in a list and supports filtering.
/// </summary>
public interface IListItem
{
    /// <summary>
    /// Gets the value used for filtering this item.
    /// </summary>
    string FilterValue { get; }
}

/// <summary>
/// Represents a list item with a title and description for richer display.
/// </summary>
public interface IListItemWithDescription : IListItem
{
    /// <summary>
    /// Gets the display title of the item.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the description of the item.
    /// </summary>
    string Description { get; }
}
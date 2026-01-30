namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// Default item delegate for <see cref="IListItemWithDescription"/> items.
/// Renders a title line with a cursor indicator and a dimmed description line.
/// </summary>
public sealed class DefaultListItemDelegate : IListItemDelegate<IListItemWithDescription>
{
    /// <inheritdoc />
    public int Height => 2;

    /// <inheritdoc />
    public int Spacing => 0;

    /// <inheritdoc />
    public void Render(IRenderSurface surface, IListItemWithDescription item, int index, bool selected)
    {
        var cursor = selected ? "> " : "  ";
        var titleStyle = selected
            ? new Appearance { Decoration = Decoration.Bold }
            : Appearance.Plain;

        surface.SetString(0, 0, cursor + item.Title, titleStyle);
        surface.SetString(2, 1, item.Description, new Appearance { Decoration = Decoration.Dim });
    }
}

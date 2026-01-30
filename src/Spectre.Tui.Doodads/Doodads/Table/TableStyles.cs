namespace Spectre.Tui.Doodads.Doodads.Table;

/// <summary>
/// Styles for table rendering.
/// </summary>
public record TableStyles
{
    /// <summary>
    /// Gets the style for header cells.
    /// </summary>
    public Appearance Header { get; init; } = new() { Decoration = Decoration.Bold };

    /// <summary>
    /// Gets the style for normal data cells.
    /// </summary>
    public Appearance Cell { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for the selected row.
    /// </summary>
    public Appearance Selected { get; init; } = new() { Decoration = Decoration.Invert };
}
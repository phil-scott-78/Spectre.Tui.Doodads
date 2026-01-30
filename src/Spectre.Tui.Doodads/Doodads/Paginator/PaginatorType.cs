namespace Spectre.Tui.Doodads.Doodads.Paginator;

/// <summary>
/// The display type for a paginator.
/// </summary>
public enum PaginatorType
{
    /// <summary>
    /// Renders dots for each page.
    /// </summary>
    Dots,

    /// <summary>
    /// Renders a numeric "Page X of Y" display.
    /// </summary>
    Numeric,
}
namespace Spectre.Tui.Doodads.Doodads.Table;

/// <summary>
/// Defines a column in a table.
/// </summary>
/// <param name="Title">The column header text.</param>
/// <param name="Width">The column width in characters.</param>
public record TableColumn(string Title, int Width);
namespace Spectre.Tui.Doodads.Doodads.Table;

/// <summary>
/// Represents a row of cell values in a table.
/// </summary>
/// <param name="Cells">The cell values for each column.</param>
public record TableRow(IReadOnlyList<string> Cells);
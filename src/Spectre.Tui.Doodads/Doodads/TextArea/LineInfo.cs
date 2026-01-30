namespace Spectre.Tui.Doodads.Doodads.TextArea;

/// <summary>
/// Provides detailed information about the cursor position within a text area.
/// </summary>
/// <param name="LineNumber">The zero-based line number.</param>
/// <param name="ColumnOffset">The zero-based column offset within the line.</param>
/// <param name="Height">The total number of lines in the text area.</param>
/// <param name="Width">The width of the current line in characters.</param>
/// <param name="CharOffset">The absolute character offset from the beginning of the content.</param>
/// <param name="RowOffset">The vertical scroll offset.</param>
/// <param name="ColumnWidth">The visible column width.</param>
public record LineInfo(
    int LineNumber,
    int ColumnOffset,
    int Height,
    int Width,
    int CharOffset,
    int RowOffset,
    int ColumnWidth);
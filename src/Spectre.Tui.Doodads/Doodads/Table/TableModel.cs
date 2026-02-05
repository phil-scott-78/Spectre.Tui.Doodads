using Spectre.Tui.Doodads.Doodads.Help;

namespace Spectre.Tui.Doodads.Doodads.Table;

/// <summary>
/// A tabular data display with column headers, scrolling, and row selection.
/// </summary>
public record TableModel : IDoodad<TableModel>, ISizedRenderable
{
    /// <summary>
    /// Gets the column definitions.
    /// </summary>
    public IReadOnlyList<TableColumn> Columns { get; init; } = [];

    /// <summary>
    /// Gets the data rows.
    /// </summary>
    public IReadOnlyList<TableRow> Rows { get; init; } = [];

    /// <summary>
    /// Gets the index of the currently selected row.
    /// </summary>
    public int SelectedIndex { get; init; }

    /// <summary>
    /// Gets a value indicating whether the table is focused.
    /// </summary>
    public bool Focused { get; init; }

    /// <summary>
    /// Gets the table styles.
    /// </summary>
    public TableStyles Styles { get; init; } = new();

    /// <summary>
    /// Gets the minimum table width in columns.
    /// </summary>
    public int MinWidth { get; init; } = 80;

    /// <summary>
    /// Gets the minimum table height in rows.
    /// </summary>
    public int MinHeight { get; init; } = 20;

    /// <summary>
    /// Gets the actual rendered width (updated via <see cref="WindowSizeMessage"/>).
    /// Falls back to <see cref="MinWidth"/> when zero.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets the actual rendered height (updated via <see cref="WindowSizeMessage"/>).
    /// Falls back to <see cref="MinHeight"/> when zero.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Gets the key map for table navigation.
    /// </summary>
    public TableKeyMap KeyMap { get; init; } = new();

    /// <summary>
    /// Gets the help sub-model.
    /// </summary>
    public HelpModel Help { get; init; } = new();

    /// <summary>
    /// Gets the scroll offset (index of the first visible data row).
    /// </summary>
    internal int ScrollOffset { get; init; }

    /// <summary>
    /// Gets the currently selected row, or <c>null</c> if the table has no rows.
    /// </summary>
    public TableRow? SelectedRow =>
        Rows.Count > 0 && SelectedIndex >= 0 && SelectedIndex < Rows.Count
            ? Rows[SelectedIndex]
            : null;

    /// <summary>
    /// Sets the column definitions.
    /// </summary>
    /// <param name="columns">The column definitions to set.</param>
    /// <returns>The updated table model.</returns>
    public TableModel SetColumns(IReadOnlyList<TableColumn> columns)
    {
        return this with { Columns = columns };
    }

    /// <summary>
    /// Sets the row data and resets selection to the first row.
    /// </summary>
    /// <param name="rows">The rows to set.</param>
    /// <returns>The updated table model.</returns>
    public TableModel SetRows(IReadOnlyList<TableRow> rows)
    {
        return this with
        {
            Rows = rows,
            SelectedIndex = 0,
            ScrollOffset = 0,
        };
    }

    /// <summary>
    /// Sets focus on the table.
    /// </summary>
    /// <returns>The updated table model and an optional command.</returns>
    public (TableModel Model, Command? Command) Focus()
    {
        return (this with { Focused = true }, null);
    }

    /// <summary>
    /// Removes focus from the table.
    /// </summary>
    /// <returns>The updated table model and an optional command.</returns>
    public (TableModel Model, Command? Command) Blur()
    {
        return (this with { Focused = false }, null);
    }

    /// <summary>
    /// Gets the current cursor (selected row) index.
    /// </summary>
    /// <returns>The index of the currently selected row.</returns>
    public int Cursor() => SelectedIndex;

    /// <summary>
    /// Sets the cursor (selected row) to the specified index.
    /// </summary>
    /// <param name="index">The row index to select.</param>
    /// <returns>The updated model and an optional command.</returns>
    public (TableModel Model, Command? Command) SetCursor(int index)
    {
        if (Rows.Count == 0)
        {
            return (this, null);
        }

        var clamped = Math.Clamp(index, 0, Rows.Count - 1);
        var model = this with { SelectedIndex = clamped };
        return (EnsureVisible(model), null);
    }

    /// <summary>
    /// Moves the cursor up by the specified number of rows.
    /// </summary>
    /// <param name="n">Number of rows to move up.</param>
    /// <returns>The updated model and an optional command.</returns>
    public (TableModel Model, Command? Command) MoveUp(int n = 1)
    {
        return MoveSelection(-n);
    }

    /// <summary>
    /// Moves the cursor down by the specified number of rows.
    /// </summary>
    /// <param name="n">Number of rows to move down.</param>
    /// <returns>The updated model and an optional command.</returns>
    public (TableModel Model, Command? Command) MoveDown(int n = 1)
    {
        return MoveSelection(n);
    }

    /// <summary>
    /// Moves the cursor to the first row.
    /// </summary>
    /// <returns>The updated model and an optional command.</returns>
    public (TableModel Model, Command? Command) GotoTop()
    {
        return GoToTopInternal();
    }

    /// <summary>
    /// Moves the cursor to the last row.
    /// </summary>
    /// <returns>The updated model and an optional command.</returns>
    public (TableModel Model, Command? Command) GotoBottom()
    {
        return GoToBottomInternal();
    }

    /// <summary>
    /// Creates a table model from delimited values.
    /// </summary>
    /// <param name="data">The delimited data string (newline-separated rows, first row is headers).</param>
    /// <param name="separator">The column separator character.</param>
    /// <returns>A new table model populated from the data.</returns>
    public static TableModel FromValues(string data, char separator)
    {
        var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            return new TableModel();
        }

        var headerCells = lines[0].Split(separator);
        var columns = headerCells.Select(h => new TableColumn(h.Trim(), Math.Max(h.Trim().Length, 10))).ToList();

        var rows = new List<TableRow>();
        for (var i = 1; i < lines.Length; i++)
        {
            var cells = lines[i].Split(separator).Select(c => c.Trim()).ToList();
            rows.Add(new TableRow(cells));
        }

        return new TableModel { Columns = columns, Rows = rows };
    }

    /// <summary>
    /// Renders the help view for the table.
    /// </summary>
    public void HelpView(IRenderSurface surface)
    {
        Help.SetKeyMap(KeyMap).View(surface);
    }

    /// <inheritdoc />
    public Command? Init() => null;

    /// <inheritdoc />
    public (TableModel Model, Command? Command) Update(Message message)
    {
        if (!Focused)
        {
            return message switch
            {
                FocusMessage => Focus(),
                _ => (this, null),
            };
        }

        switch (message)
        {
            case KeyMessage km:
                return HandleKey(km);

            case FocusMessage:
                return Focus();

            case BlurMessage:
                return Blur();

            case WindowSizeMessage ws:
                return (this with { Width = ws.Width, Height = ws.Height }, null);

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        RenderHeader(surface);
        RenderBody(surface);
    }

    private void RenderHeader(IRenderSurface surface)
    {
        var x = 0;
        for (var i = 0; i < Columns.Count; i++)
        {
            if (i > 0)
            {
                x++; // 1-space padding between columns
            }

            var col = Columns[i];
            var text = TruncateText(col.Title, col.Width);
            surface.SetString(x, 0, text, Styles.Header);
            x += col.Width;
        }
    }

    private void RenderBody(IRenderSurface surface)
    {
        var bodyHeight = Math.Max(0, surface.Viewport.Height - 1);

        for (var y = 0; y < bodyHeight; y++)
        {
            var rowIndex = ScrollOffset + y;
            if (rowIndex >= Rows.Count)
            {
                break;
            }

            var row = Rows[rowIndex];
            var isSelected = Focused && rowIndex == SelectedIndex;
            var style = isSelected ? Styles.Selected : Styles.Cell;

            var x = 0;
            for (var ci = 0; ci < Columns.Count; ci++)
            {
                if (ci > 0)
                {
                    x++; // 1-space padding between columns
                }

                var col = Columns[ci];
                var cellText = ci < row.Cells.Count ? row.Cells[ci] : string.Empty;
                var text = TruncateText(cellText, col.Width);

                // Pad to full column width for consistent selection highlighting
                text = text.PadRight(col.Width);
                surface.SetString(x, y + 1, text, style);
                x += col.Width;
            }
        }
    }

    private int BodyHeight => Math.Max(0, (Height > 0 ? Height : MinHeight) - 1);

    private (TableModel Model, Command? Command) HandleKey(KeyMessage km)
    {
        if (KeyMap.LineUp.Matches(km))
        {
            return MoveSelection(-1);
        }

        if (KeyMap.LineDown.Matches(km))
        {
            return MoveSelection(1);
        }

        if (KeyMap.PageUp.Matches(km))
        {
            return MoveSelection(-BodyHeight);
        }

        if (KeyMap.PageDown.Matches(km))
        {
            return MoveSelection(BodyHeight);
        }

        if (KeyMap.HalfPageUp.Matches(km))
        {
            return MoveSelection(-(BodyHeight / 2));
        }

        if (KeyMap.HalfPageDown.Matches(km))
        {
            return MoveSelection(BodyHeight / 2);
        }

        if (KeyMap.GoToTop.Matches(km))
        {
            return GoToTopInternal();
        }

        if (KeyMap.GoToBottom.Matches(km))
        {
            return GoToBottomInternal();
        }

        // Forward to help sub-doodad
        var (help, helpCmd) = Help.Update(km);
        if (help != Help)
        {
            return (this with { Help = help }, helpCmd);
        }

        return (this, null);
    }

    private (TableModel Model, Command? Command) MoveSelection(int delta)
    {
        if (Rows.Count == 0)
        {
            return (this, null);
        }

        var newIndex = Math.Clamp(SelectedIndex + delta, 0, Rows.Count - 1);
        var model = this with { SelectedIndex = newIndex };
        return (EnsureVisible(model), null);
    }

    private (TableModel Model, Command? Command) GoToTopInternal()
    {
        if (Rows.Count == 0)
        {
            return (this, null);
        }

        var model = this with { SelectedIndex = 0, ScrollOffset = 0 };
        return (model, null);
    }

    private (TableModel Model, Command? Command) GoToBottomInternal()
    {
        if (Rows.Count == 0)
        {
            return (this, null);
        }

        var lastIndex = Rows.Count - 1;
        var model = this with { SelectedIndex = lastIndex };
        return (EnsureVisible(model), null);
    }

    private static TableModel EnsureVisible(TableModel model)
    {
        var bodyHeight = model.BodyHeight;
        if (bodyHeight <= 0)
        {
            return model;
        }

        var offset = ScrollHelper.EnsureVisible(model.SelectedIndex, model.ScrollOffset, bodyHeight);

        // Clamp scroll offset
        var maxOffset = Math.Max(0, model.Rows.Count - bodyHeight);
        offset = Math.Clamp(offset, 0, maxOffset);

        return model with { ScrollOffset = offset };
    }

    private static string TruncateText(string text, int maxWidth)
    {
        if (maxWidth <= 0)
        {
            return string.Empty;
        }

        if (text.Length <= maxWidth)
        {
            return text;
        }

        if (maxWidth <= 1)
        {
            return "\u2026";
        }

        return string.Concat(text.AsSpan(0, maxWidth - 1), "\u2026");
    }
}
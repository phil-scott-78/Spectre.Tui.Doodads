using Spectre.Tui.Doodads.Doodads.Cursor;

namespace Spectre.Tui.Doodads.Doodads.TextArea;

/// <summary>
/// A multi-line text editing area with line numbers and scrolling.
/// </summary>
public record TextAreaModel : IDoodad<TextAreaModel>, ISizedRenderable
{
    /// <summary>
    /// Gets the prompt prefix.
    /// </summary>
    public string Prompt { get; init; } = string.Empty;

    /// <summary>
    /// Gets the placeholder text shown when empty.
    /// </summary>
    public string Placeholder { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether to show line numbers.
    /// </summary>
    public bool ShowLineNumbers { get; init; }

    /// <summary>
    /// Gets the character limit (0 = unlimited).
    /// </summary>
    public int CharLimit { get; init; }

    /// <summary>
    /// Gets the minimum display width.
    /// </summary>
    public int MinWidth { get; init; } = 40;

    /// <summary>
    /// Gets the minimum display height.
    /// </summary>
    public int MinHeight { get; init; } = 6;

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
    /// Returns the desired size: fills available width and height.
    /// </summary>
    public Size Measure(Size availableSize) => new(availableSize.Width, availableSize.Height);

    /// <summary>
    /// Gets the maximum width.
    /// </summary>
    public int MaxWidth { get; init; }

    /// <summary>
    /// Gets the maximum height.
    /// </summary>
    public int MaxHeight { get; init; }

    /// <summary>
    /// Gets the embedded cursor doodad.
    /// </summary>
    public CursorModel Cursor { get; init; } = new();

    /// <summary>
    /// Gets a value indicating whether the text area is focused.
    /// </summary>
    public bool Focused { get; init; }

    /// <summary>
    /// Gets the line number style.
    /// </summary>
    public Appearance LineNumberStyle { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style applied when the text area is focused.
    /// </summary>
    public Appearance FocusedStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style applied when the text area is blurred.
    /// </summary>
    public Appearance BlurredStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the character displayed for lines past the end of the buffer.
    /// </summary>
    public string EndOfBufferCharacter { get; init; } = "~";

    /// <summary>
    /// Gets the optional validation error.
    /// </summary>
    public string? Err { get; init; }

    /// <summary>
    /// Gets the optional dynamic prompt function (given line index, returns prompt string).
    /// </summary>
    public Func<int, string>? PromptFunc { get; init; }

    /// <summary>
    /// Gets the prompt width when using a dynamic prompt function.
    /// </summary>
    public int PromptFuncWidth { get; init; }

    /// <summary>
    /// Gets the key map.
    /// </summary>
    public TextAreaKeyMap KeyMap { get; init; } = new();

    /// <summary>
    /// Gets the lines of text.
    /// </summary>
    internal ImmutableArray<ImmutableArray<Rune>> Lines { get; init; } =
        [ImmutableArray<Rune>.Empty];

    /// <summary>
    /// Gets the yank (clipboard) buffer.
    /// </summary>
    internal ImmutableArray<Rune> YankBuffer { get; init; } = [];

    /// <summary>
    /// Gets the current row.
    /// </summary>
    internal int Row { get; init; }

    /// <summary>
    /// Gets the current column.
    /// </summary>
    internal int Col { get; init; }

    /// <summary>
    /// Gets the vertical scroll offset.
    /// </summary>
    internal int RowOffset { get; init; }

    /// <summary>
    /// Gets the horizontal scroll offset.
    /// </summary>
    internal int ColOffset { get; init; }

    /// <inheritdoc />
    public Command? Init()
    {
        var cmd = Cursor.Init();
        if (Focused)
        {
            return Commands.Batch(cmd, Commands.Message(new FocusMessage()));
        }

        return cmd;
    }

    /// <inheritdoc />
    public (TextAreaModel Model, Command? Command) Update(Message message)
    {
        if (!Focused)
        {
            return message switch
            {
                FocusMessage => Focus(),
                _ => (this, null),
            };
        }

        if (message is KeyMessage km)
        {
            return HandleKey(km);
        }

        if (message is FocusMessage)
        {
            return Focus();
        }

        if (message is BlurMessage)
        {
            return Blur();
        }

        if (message is WindowSizeMessage ws)
        {
            return (this with { Width = ws.Width, Height = ws.Height }, null);
        }

        // Forward to cursor
        var (cursor, cursorCmd) = Cursor.Update(message);
        return (this with { Cursor = cursor }, cursorCmd);
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        if (MaxWidth > 0)
        {
            width = Math.Min(width, MaxWidth);
        }

        if (MaxHeight > 0)
        {
            height = Math.Min(height, MaxHeight);
        }

        var lineNumWidth = ShowLineNumbers ? (Lines.Length.ToString().Length + 1) : 0;

        if (Lines is [{ Length: 0 } _] && !string.IsNullOrEmpty(Placeholder))
        {
            surface.SetString(lineNumWidth, 0, Placeholder,
                new Appearance { Decoration = Decoration.Dim });
            return;
        }

        for (var visRow = 0; visRow < height; visRow++)
        {
            var lineIdx = RowOffset + visRow;
            if (lineIdx >= Lines.Length)
            {
                break;
            }

            var y = visRow;

            if (ShowLineNumbers)
            {
                var lineNum = (lineIdx + 1).ToString().PadLeft(lineNumWidth - 1);
                surface.SetString(0, y, lineNum, LineNumberStyle);
            }

            var line = Lines[lineIdx];
            var startCol = ColOffset;
            var x = lineNumWidth;
            var textStyle = Focused ? FocusedStyle : BlurredStyle;

            for (var ci = startCol; ci < line.Length && x < width; ci++)
            {
                var ch = line[ci].ToString();
                if (Focused && lineIdx == Row && ci == Col)
                {
                    var cursorChar = Cursor.SetChar(ch, textStyle);
                    surface.Render(cursorChar, new Rectangle(x, y, 1, 1));
                }
                else
                {
                    surface.SetString(x, y, ch, textStyle);
                }

                x++;
            }

            // Render cursor at end of line
            if (Focused && lineIdx == Row && Col >= line.Length)
            {
                if (x < width)
                {
                    var cursorChar = Cursor.SetChar(" ");
                    surface.Render(cursorChar, new Rectangle(x, y, 1, 1));
                }
            }
        }
    }

    /// <summary>
    /// Sets the text area content.
    /// </summary>
    public TextAreaModel SetValue(string text)
    {
        var lines = text.Split('\n')
            .Select(l => l.EnumerateRunes().ToImmutableArray())
            .ToImmutableArray();

        if (lines.Length == 0)
        {
            lines = [ImmutableArray<Rune>.Empty];
        }

        return this with
        {
            Lines = lines,
            Row = 0,
            Col = 0,
            RowOffset = 0,
            ColOffset = 0,
        };
    }

    /// <summary>
    /// Gets the text area content as a string.
    /// </summary>
    public string GetValue()
    {
        return string.Join("\n", Lines.Select(
            line => string.Concat(line.Select(r => r.ToString()))));
    }

    /// <summary>
    /// Sets focus and starts cursor blinking.
    /// </summary>
    public (TextAreaModel Model, Command? Command) Focus()
    {
        var (cursor, cmd) = Cursor.Focus();
        return (this with { Focused = true, Cursor = cursor }, cmd);
    }

    /// <summary>
    /// Removes focus.
    /// </summary>
    public (TextAreaModel Model, Command? Command) Blur()
    {
        var (cursor, cmd) = Cursor.Blur();
        return (this with { Focused = false, Cursor = cursor }, cmd);
    }

    /// <summary>
    /// Inserts a string at the cursor position.
    /// </summary>
    public TextAreaModel InsertString(string text)
    {
        var model = this;
        foreach (var rune in text.EnumerateRunes())
        {
            if (rune == new Rune('\n'))
            {
                model = model.InsertNewline();
            }
            else
            {
                model = model.InsertRune(rune);
            }
        }

        return model;
    }

    /// <summary>
    /// Inserts a single rune at the cursor position.
    /// </summary>
    /// <param name="rune">The rune to insert.</param>
    /// <returns>The updated model.</returns>
    public TextAreaModel InsertRune(Rune rune)
    {
        return InsertRuneInternal(rune);
    }

    /// <summary>
    /// Gets the total character count of the text area content.
    /// </summary>
    public int Length()
    {
        var count = 0;
        for (var i = 0; i < Lines.Length; i++)
        {
            count += Lines[i].Length;
            if (i < Lines.Length - 1)
            {
                count++; // newline
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the number of lines in the text area.
    /// </summary>
    public int LineCount() => Lines.Length;

    /// <summary>
    /// Gets the current line index (zero-based).
    /// </summary>
    public int Line() => Row;

    /// <summary>
    /// Gets detailed information about the current cursor position.
    /// </summary>
    public LineInfo LineInfo()
    {
        var charOffset = 0;
        for (var i = 0; i < Row; i++)
        {
            charOffset += Lines[i].Length + 1; // +1 for newline
        }

        charOffset += Col;

        var lineNumWidth = ShowLineNumbers ? (Lines.Length.ToString().Length + 1) : 0;
        var effectiveWidth = Width > 0 ? Width : MinWidth;
        return new LineInfo(
            LineNumber: Row,
            ColumnOffset: Col,
            Height: Lines.Length,
            Width: Lines[Row].Length,
            CharOffset: charOffset,
            RowOffset: RowOffset,
            ColumnWidth: effectiveWidth - lineNumWidth);
    }

    /// <summary>
    /// Sets the cursor column position on the current line.
    /// </summary>
    /// <param name="col">The column position.</param>
    /// <returns>The updated model.</returns>
    public TextAreaModel SetCursor(int col)
    {
        var clamped = Math.Clamp(col, 0, Lines[Row].Length);
        return EnsureVisible(this with { Col = clamped });
    }

    /// <summary>
    /// Moves the cursor to the start of the current line.
    /// </summary>
    /// <returns>The updated model.</returns>
    public TextAreaModel CursorStart()
    {
        return EnsureVisible(this with { Col = 0 });
    }

    /// <summary>
    /// Moves the cursor to the end of the current line.
    /// </summary>
    /// <returns>The updated model.</returns>
    public TextAreaModel CursorEnd()
    {
        return EnsureVisible(this with { Col = Lines[Row].Length });
    }

    /// <summary>
    /// Moves the cursor up one line.
    /// </summary>
    /// <returns>The updated model.</returns>
    public TextAreaModel CursorUp()
    {
        return MoveUp();
    }

    /// <summary>
    /// Moves the cursor down one line.
    /// </summary>
    /// <returns>The updated model.</returns>
    public TextAreaModel CursorDown()
    {
        return MoveDown();
    }

    /// <summary>
    /// Sets the width of the text area.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <returns>The updated model.</returns>
    public TextAreaModel SetWidth(int width)
    {
        return this with { MinWidth = width };
    }

    /// <summary>
    /// Sets the height of the text area.
    /// </summary>
    /// <param name="height">The new height.</param>
    /// <returns>The updated model.</returns>
    public TextAreaModel SetHeight(int height)
    {
        return this with { MinHeight = height };
    }

    /// <summary>
    /// Sets a dynamic prompt function with a given width.
    /// </summary>
    /// <param name="width">The prompt width in columns.</param>
    /// <param name="promptFunc">A function that takes a line index and returns the prompt string.</param>
    /// <returns>The updated model.</returns>
    public TextAreaModel SetPromptFunc(int width, Func<int, string> promptFunc)
    {
        return this with { PromptFunc = promptFunc, PromptFuncWidth = width };
    }

    /// <summary>
    /// Resets the text area to its initial empty state.
    /// </summary>
    /// <returns>The updated model.</returns>
    public TextAreaModel Reset()
    {
        return this with
        {
            Lines = [ImmutableArray<Rune>.Empty],
            Row = 0,
            Col = 0,
            RowOffset = 0,
            ColOffset = 0,
            YankBuffer = [],
            Err = null,
        };
    }

    private (TextAreaModel Model, Command? Command) HandleKey(KeyMessage km)
    {
        if (KeyMap.CharacterForward.Matches(km))
        {
            return ResetIdleAfterKey(MoveRight());
        }

        if (KeyMap.CharacterBackward.Matches(km))
        {
            return ResetIdleAfterKey(MoveLeft());
        }

        if (KeyMap.LineUp.Matches(km))
        {
            return ResetIdleAfterKey(MoveUp());
        }

        if (KeyMap.LineDown.Matches(km))
        {
            return ResetIdleAfterKey(MoveDown());
        }

        if (KeyMap.PageUp.Matches(km))
        {
            return ResetIdleAfterKey(PageUpMove());
        }

        if (KeyMap.PageDown.Matches(km))
        {
            return ResetIdleAfterKey(PageDownMove());
        }

        if (KeyMap.LineStart.Matches(km))
        {
            return ResetIdleAfterKey(this with { Col = 0 });
        }

        if (KeyMap.LineEnd.Matches(km))
        {
            return ResetIdleAfterKey(this with { Col = Lines[Row].Length });
        }

        if (KeyMap.DeleteCharBackward.Matches(km))
        {
            return ResetIdleAfterKey(DeleteBackward());
        }

        if (KeyMap.DeleteCharForward.Matches(km))
        {
            return ResetIdleAfterKey(DeleteForward());
        }

        if (KeyMap.DeleteToEnd.Matches(km))
        {
            return ResetIdleAfterKey(DeleteToLineEnd());
        }

        if (KeyMap.DeleteToStart.Matches(km))
        {
            return ResetIdleAfterKey(DeleteToLineStart());
        }

        if (KeyMap.InsertNewline.Matches(km))
        {
            return ResetIdleAfterKey(InsertNewline());
        }

        // Transpose character backward (ctrl+t)
        if (KeyMap.TransposeCharacterBackward.Matches(km))
        {
            return ResetIdleAfterKey(TransposeCharacterBackward());
        }

        // Paste (ctrl+v)
        if (KeyMap.Paste.Matches(km))
        {
            return ResetIdleAfterKey(PasteFromYankBuffer());
        }

        // Alt key combinations for word transforms
        if (km.Alt && KeyMap.UppercaseWordForward.Matches(km))
        {
            return ResetIdleAfterKey(UppercaseWordForward());
        }

        if (km.Alt && KeyMap.LowercaseWordForward.Matches(km))
        {
            return ResetIdleAfterKey(LowercaseWordForward());
        }

        if (km.Alt && KeyMap.CapitalizeWordForward.Matches(km))
        {
            return ResetIdleAfterKey(CapitalizeWordForward());
        }

        // Input begin/end (ctrl+home / ctrl+end)
        if (km is { Ctrl: true, Key: Key.Home })
        {
            return ResetIdleAfterKey(InputBegin());
        }

        if (km is { Ctrl: true, Key: Key.End })
        {
            return ResetIdleAfterKey(InputEnd());
        }

        // Character input
        if (km is { Key: Key.Char, Runes.Length: > 0 })
        {
            var model = this;
            foreach (var rune in km.Runes)
            {
                model = model.InsertRuneInternal(rune);
            }

            return ResetIdleAfterKey(model);
        }

        if (km.Key == Key.Space)
        {
            return ResetIdleAfterKey(InsertRuneInternal(new Rune(' ')));
        }

        return (this, null);
    }

    private (TextAreaModel Model, Command? Command) ResetIdleAfterKey(TextAreaModel updatedModel)
    {
        var (cursor, cursorCmd) = updatedModel.Cursor.ResetIdle();
        return (updatedModel with { Cursor = cursor }, cursorCmd);
    }

    private TextAreaModel InsertRuneInternal(Rune rune)
    {
        if (CharLimit > 0 && Length() >= CharLimit)
        {
            return this;
        }

        var line = Lines[Row];
        var newLine = line.Insert(Col, rune);
        var newLines = Lines.SetItem(Row, newLine);
        return EnsureVisible(this with { Lines = newLines, Col = Col + 1 });
    }

    private TextAreaModel InsertNewline()
    {
        if (CharLimit > 0 && Length() >= CharLimit)
        {
            return this;
        }

        var line = Lines[Row];
        var before = line.Take(Col).ToImmutableArray();
        var after = line.Skip(Col).ToImmutableArray();

        var newLines = Lines
            .SetItem(Row, before)
            .Insert(Row + 1, after);

        return EnsureVisible(this with
        {
            Lines = newLines,
            Row = Row + 1,
            Col = 0,
        });
    }

    private TextAreaModel DeleteBackward()
    {
        if (Col > 0)
        {
            var line = Lines[Row];
            var newLine = line.RemoveAt(Col - 1);
            var newLines = Lines.SetItem(Row, newLine);
            return EnsureVisible(this with { Lines = newLines, Col = Col - 1 });
        }

        if (Row > 0)
        {
            var prevLine = Lines[Row - 1];
            var curLine = Lines[Row];
            var merged = prevLine.AddRange(curLine);
            var newLines = Lines
                .SetItem(Row - 1, merged)
                .RemoveAt(Row);
            return EnsureVisible(this with
            {
                Lines = newLines,
                Row = Row - 1,
                Col = prevLine.Length,
            });
        }

        return this;
    }

    private TextAreaModel DeleteForward()
    {
        var line = Lines[Row];
        if (Col < line.Length)
        {
            var newLine = line.RemoveAt(Col);
            var newLines = Lines.SetItem(Row, newLine);
            return this with { Lines = newLines };
        }

        if (Row < Lines.Length - 1)
        {
            var nextLine = Lines[Row + 1];
            var merged = line.AddRange(nextLine);
            var newLines = Lines
                .SetItem(Row, merged)
                .RemoveAt(Row + 1);
            return this with { Lines = newLines };
        }

        return this;
    }

    private TextAreaModel DeleteToLineEnd()
    {
        var line = Lines[Row];
        var yanked = line.Skip(Col).ToImmutableArray();
        var newLine = line.Take(Col).ToImmutableArray();
        var newLines = Lines.SetItem(Row, newLine);
        return this with { Lines = newLines, YankBuffer = yanked };
    }

    private TextAreaModel DeleteToLineStart()
    {
        var line = Lines[Row];
        var yanked = line.Take(Col).ToImmutableArray();
        var newLine = line.Skip(Col).ToImmutableArray();
        var newLines = Lines.SetItem(Row, newLine);
        return this with { Lines = newLines, Col = 0, YankBuffer = yanked };
    }

    private TextAreaModel MoveRight()
    {
        var line = Lines[Row];
        if (Col < line.Length)
        {
            return EnsureVisible(this with { Col = Col + 1 });
        }

        if (Row < Lines.Length - 1)
        {
            return EnsureVisible(this with { Row = Row + 1, Col = 0 });
        }

        return this;
    }

    private TextAreaModel MoveLeft()
    {
        if (Col > 0)
        {
            return EnsureVisible(this with { Col = Col - 1 });
        }

        if (Row > 0)
        {
            return EnsureVisible(this with { Row = Row - 1, Col = Lines[Row - 1].Length });
        }

        return this;
    }

    private TextAreaModel MoveUp()
    {
        if (Row > 0)
        {
            var newRow = Row - 1;
            var newCol = Math.Min(Col, Lines[newRow].Length);
            return EnsureVisible(this with { Row = newRow, Col = newCol });
        }

        return this;
    }

    private TextAreaModel MoveDown()
    {
        if (Row < Lines.Length - 1)
        {
            var newRow = Row + 1;
            var newCol = Math.Min(Col, Lines[newRow].Length);
            return EnsureVisible(this with { Row = newRow, Col = newCol });
        }

        return this;
    }

    private TextAreaModel PageUpMove()
    {
        var visibleHeight = Height > 0 ? Height : MinHeight;
        var newRow = Math.Max(0, Row - visibleHeight);
        var newCol = Math.Min(Col, Lines[newRow].Length);
        return EnsureVisible(this with { Row = newRow, Col = newCol });
    }

    private TextAreaModel PageDownMove()
    {
        var visibleHeight = Height > 0 ? Height : MinHeight;
        var newRow = Math.Min(Lines.Length - 1, Row + visibleHeight);
        var newCol = Math.Min(Col, Lines[newRow].Length);
        return EnsureVisible(this with { Row = newRow, Col = newCol });
    }

    private TextAreaModel UppercaseWordForward()
    {
        var line = Lines[Row];
        var start = Col;
        var end = Col;

        // Find word end
        while (end < line.Length && !Rune.IsWhiteSpace(line[end]))
        {
            end++;
        }

        if (start == end)
        {
            return this;
        }

        var newLine = line;
        for (var i = start; i < end; i++)
        {
            newLine = newLine.SetItem(i, Rune.ToUpperInvariant(newLine[i]));
        }

        var newLines = Lines.SetItem(Row, newLine);
        return EnsureVisible(this with { Lines = newLines, Col = end });
    }

    private TextAreaModel LowercaseWordForward()
    {
        var line = Lines[Row];
        var start = Col;
        var end = Col;

        while (end < line.Length && !Rune.IsWhiteSpace(line[end]))
        {
            end++;
        }

        if (start == end)
        {
            return this;
        }

        var newLine = line;
        for (var i = start; i < end; i++)
        {
            newLine = newLine.SetItem(i, Rune.ToLowerInvariant(newLine[i]));
        }

        var newLines = Lines.SetItem(Row, newLine);
        return EnsureVisible(this with { Lines = newLines, Col = end });
    }

    private TextAreaModel CapitalizeWordForward()
    {
        var line = Lines[Row];
        var start = Col;
        var end = Col;

        while (end < line.Length && !Rune.IsWhiteSpace(line[end]))
        {
            end++;
        }

        if (start == end)
        {
            return this;
        }

        var newLine = line;
        newLine = newLine.SetItem(start, Rune.ToUpperInvariant(newLine[start]));
        for (var i = start + 1; i < end; i++)
        {
            newLine = newLine.SetItem(i, Rune.ToLowerInvariant(newLine[i]));
        }

        var newLines = Lines.SetItem(Row, newLine);
        return EnsureVisible(this with { Lines = newLines, Col = end });
    }

    private TextAreaModel TransposeCharacterBackward()
    {
        if (Col < 2)
        {
            return this;
        }

        var line = Lines[Row];
        if (Col > line.Length)
        {
            return this;
        }

        var a = line[Col - 2];
        var b = line[Col - 1];
        var newLine = line.SetItem(Col - 2, b).SetItem(Col - 1, a);
        var newLines = Lines.SetItem(Row, newLine);
        return this with { Lines = newLines };
    }

    private TextAreaModel InputBegin()
    {
        return EnsureVisible(this with { Row = 0, Col = 0 });
    }

    private TextAreaModel InputEnd()
    {
        var lastRow = Lines.Length - 1;
        return EnsureVisible(this with { Row = lastRow, Col = Lines[lastRow].Length });
    }

    private TextAreaModel PasteFromYankBuffer()
    {
        if (YankBuffer.IsEmpty)
        {
            return this;
        }

        var model = this;
        foreach (var rune in YankBuffer)
        {
            if (rune == new Rune('\n'))
            {
                model = model.InsertNewline();
            }
            else
            {
                model = model.InsertRuneInternal(rune);
            }
        }

        return model;
    }

    private static TextAreaModel EnsureVisible(TextAreaModel model)
    {
        var rowOffset = model.RowOffset;
        var colOffset = model.ColOffset;
        var effectiveHeight = model.Height > 0 ? model.Height : model.MinHeight;
        var effectiveWidth = model.Width > 0 ? model.Width : model.MinWidth;

        if (model.Row < rowOffset)
        {
            rowOffset = model.Row;
        }

        if (model.Row >= rowOffset + effectiveHeight)
        {
            rowOffset = model.Row - effectiveHeight + 1;
        }

        var lineNumWidth = model.ShowLineNumbers
            ? (model.Lines.Length.ToString().Length + 1)
            : 0;
        var contentWidth = effectiveWidth - lineNumWidth;

        if (model.Col < colOffset)
        {
            colOffset = model.Col;
        }

        if (model.Col >= colOffset + contentWidth)
        {
            colOffset = model.Col - contentWidth + 1;
        }

        return model with { RowOffset = rowOffset, ColOffset = colOffset };
    }
}
using Spectre.Tui.Doodads.Doodads.Cursor;

namespace Spectre.Tui.Doodads.Doodads.TextInput;

/// <summary>
/// A single-line text input field with cursor, scrolling, validation, and echo mode masking.
/// </summary>
public record TextInputModel : IDoodad<TextInputModel>, ISizedRenderable
{
    /// <summary>
    /// Gets the prompt prefix text (e.g., "&gt; ").
    /// </summary>
    public string Prompt { get; init; } = string.Empty;

    /// <summary>
    /// Gets the placeholder text shown when the value is empty.
    /// </summary>
    public string Placeholder { get; init; } = string.Empty;

    /// <summary>
    /// Gets the echo mode controlling how characters are displayed.
    /// </summary>
    public EchoMode EchoMode { get; init; } = EchoMode.Normal;

    /// <summary>
    /// Gets the character used for password masking.
    /// </summary>
    public char EchoCharacter { get; init; } = '*';

    /// <summary>
    /// Gets the maximum number of characters allowed (0 = unlimited).
    /// </summary>
    public int CharLimit { get; init; }

    /// <summary>
    /// Gets the minimum display width of the input field.
    /// </summary>
    public int MinWidth { get; init; } = 40;

    /// <summary>
    /// Gets the minimum display height of the input field (always 1).
    /// </summary>
    public int MinHeight { get; init; } = 1;

    /// <summary>
    /// Gets the actual rendered width (updated via <see cref="WindowSizeMessage"/>).
    /// Falls back to <see cref="MinWidth"/> when zero.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Returns the desired size: fills available width, always 1 row tall.
    /// </summary>
    public Size Measure(Size availableSize) => new(availableSize.Width, 1);

    /// <summary>
    /// Gets the embedded cursor doodad.
    /// </summary>
    public CursorModel Cursor { get; init; } = new();

    /// <summary>
    /// Gets a value indicating whether the input is focused.
    /// </summary>
    public bool Focused { get; init; }

    /// <summary>
    /// Gets the optional validation function. Returns true if the value is valid.
    /// </summary>
    public Func<string, bool>? Validate { get; init; }

    /// <summary>
    /// Gets the style applied to the prompt text.
    /// </summary>
    public Appearance PromptStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style applied to the input text.
    /// </summary>
    public Appearance TextStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style applied to the placeholder text.
    /// </summary>
    public Appearance PlaceholderStyle { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style applied to suggestion completion text.
    /// </summary>
    public Appearance CompletionStyle { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the maximum display width (0 = unlimited, uses MinWidth).
    /// </summary>
    public int MaxWidth { get; init; }

    /// <summary>
    /// Gets the available suggestions for autocomplete.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether suggestions are shown.
    /// </summary>
    public bool ShowSuggestions { get; init; } = true;

    /// <summary>
    /// Gets the key map for this input.
    /// </summary>
    public TextInputKeyMap KeyMap { get; init; } = new();

    /// <summary>
    /// Gets the current value as runes.
    /// </summary>
    internal ImmutableArray<Rune> Value { get; init; } = [];

    /// <summary>
    /// Gets the cursor position in runes.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Gets the horizontal scroll offset.
    /// </summary>
    internal int Offset { get; init; }

    /// <summary>
    /// Gets the indices of matched suggestions.
    /// </summary>
    internal IReadOnlyList<int> MatchedSuggestionIndices { get; init; } = [];

    /// <summary>
    /// Gets the selected suggestion index within matched suggestions.
    /// </summary>
    internal int SelectedSuggestionIndex { get; init; }

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
    public (TextInputModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case FocusMessage:
                return Focus();

            case BlurMessage:
                return Blur();

            case WindowSizeMessage ws:
                return (this with { Width = ws.Width }, null);

            case KeyMessage when !Focused:
                return (this, null);

            case KeyMessage km:
                return HandleKey(km);

            default:
                // Forward to cursor for blink handling
                var (cursor, cursorCmd) = Cursor.Update(message);
                return (this with { Cursor = cursor }, cursorCmd);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var x = 0;
        var viewportWidth = Math.Max(0, surface.Viewport.Width);
        var effectiveWidth = MaxWidth > 0 ? Math.Min(viewportWidth, MaxWidth) : viewportWidth;

        // Render prompt
        if (Prompt.Length > 0)
        {
            var pos = surface.SetString(0, 0, Prompt, PromptStyle);
            x = pos.X;
        }

        var availableWidth = effectiveWidth - x;
        if (availableWidth <= 0)
        {
            return;
        }

        // Show placeholder when empty and not focused
        if (Value.IsEmpty && !Focused)
        {
            surface.SetString(x, 0, TruncateToWidth(Placeholder, availableWidth), PlaceholderStyle);
            return;
        }

        // Show placeholder when empty and focused
        if (Value.IsEmpty && Focused)
        {
            // Render cursor at the start position
            var cursorChar = Placeholder.Length > 0 ? Placeholder[..1] : " ";
            surface.Render(Cursor.SetChar(cursorChar, PlaceholderStyle), new Rectangle(x, 0, 1, 1));
            if (Placeholder.Length > 1)
            {
                surface.SetString(x + 1, 0, TruncateToWidth(Placeholder[1..], availableWidth - 1), PlaceholderStyle);
            }

            return;
        }

        // Render value with echo mode handling
        if (EchoMode == EchoMode.None)
        {
            // In None mode, render nothing for the value, just the cursor
            if (Focused)
            {
                surface.Render(Cursor.SetChar(" "), new Rectangle(x, 0, 1, 1));
            }

            return;
        }

        // Calculate visible portion based on offset
        var visibleRunes = GetVisibleRunes(availableWidth);

        // Render the visible text
        var renderX = x;
        for (var i = 0; i < visibleRunes.Length; i++)
        {
            var runeIndex = Offset + i;
            var displayChar = GetDisplayChar(runeIndex);
            var isCursorPos = Focused && (runeIndex == Position);

            if (isCursorPos)
            {
                surface.Render(Cursor.SetChar(displayChar, TextStyle), new Rectangle(renderX, 0, 1, 1));
            }
            else
            {
                surface.SetString(renderX, 0, displayChar, TextStyle);
            }

            renderX += UnicodeWidth(displayChar);
        }

        // If cursor is at the end of the value
        if (Focused && Position >= Value.Length)
        {
            // Show suggestion completion inline
            var suggestion = ShowSuggestions ? CurrentSuggestion() : null;
            if (suggestion is not null)
            {
                var completion = suggestion[Value.Length..];
                if (completion.Length > 0)
                {
                    var completionChar = completion[..1];
                    surface.Render(Cursor.SetChar(completionChar, CompletionStyle), new Rectangle(renderX, 0, 1, 1));
                    renderX++;
                    if (completion.Length > 1)
                    {
                        var remaining = completion[1..];
                        var maxRemaining = availableWidth - (renderX - x);
                        if (maxRemaining > 0)
                        {
                            surface.SetString(renderX, 0, TruncateToWidth(remaining, maxRemaining), CompletionStyle);
                        }
                    }
                }
                else
                {
                    surface.Render(Cursor.SetChar(" "), new Rectangle(renderX, 0, 1, 1));
                }
            }
            else
            {
                surface.Render(Cursor.SetChar(" "), new Rectangle(renderX, 0, 1, 1));
            }
        }
    }

    /// <summary>
    /// Sets the input value and moves the cursor to the end.
    /// </summary>
    public TextInputModel SetValue(string text)
    {
        var runes = EnumerateRunes(text).ToImmutableArray();
        var position = runes.Length;
        var model = this with { Value = runes, Position = position };
        return model.AdjustOffset();
    }

    /// <summary>
    /// Gets the current value as a string.
    /// </summary>
    public string GetValue()
    {
        if (Value.IsEmpty)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(Value.Length);
        foreach (var rune in Value)
        {
            sb.Append(rune.ToString());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Sets the input to focused state.
    /// </summary>
    public (TextInputModel Model, Command? Command) Focus()
    {
        var (cursor, cmd) = Cursor.Focus();
        var model = this with { Focused = true, Cursor = cursor };
        model = model.UpdateCursorChar();
        return (model, cmd);
    }

    /// <summary>
    /// Sets the input to unfocused state.
    /// </summary>
    public (TextInputModel Model, Command? Command) Blur()
    {
        var (cursor, cmd) = Cursor.Blur();
        return (this with { Focused = false, Cursor = cursor }, cmd);
    }

    /// <summary>
    /// Sets the cursor position within the value.
    /// </summary>
    public TextInputModel SetCursorPosition(int position)
    {
        var clamped = Math.Clamp(position, 0, Value.Length);
        var model = this with { Position = clamped };
        model = model.AdjustOffset();
        return model.UpdateCursorChar();
    }

    /// <summary>
    /// Moves the cursor to the start of the input.
    /// </summary>
    public TextInputModel CursorStart()
    {
        return MoveToStart();
    }

    /// <summary>
    /// Moves the cursor to the end of the input.
    /// </summary>
    public TextInputModel CursorEnd()
    {
        return MoveToEnd();
    }

    /// <summary>
    /// Resets the input to its initial empty state.
    /// </summary>
    public TextInputModel Reset()
    {
        return this with
        {
            Value = [],
            Position = 0,
            Offset = 0,
            MatchedSuggestionIndices = [],
            SelectedSuggestionIndex = 0,
        };
    }

    /// <summary>
    /// Gets the cursor mode of the embedded cursor.
    /// </summary>
    public CursorMode CursorMode()
    {
        return Cursor.Mode;
    }

    /// <summary>
    /// Sets the cursor mode of the embedded cursor.
    /// </summary>
    /// <param name="mode">The cursor mode to set.</param>
    /// <returns>The updated model.</returns>
    public TextInputModel SetCursorMode(CursorMode mode)
    {
        return this with { Cursor = Cursor with { Mode = mode } };
    }

    /// <summary>
    /// Sets the available suggestions for autocomplete.
    /// </summary>
    /// <param name="suggestions">The suggestion strings.</param>
    /// <returns>The updated model.</returns>
    public TextInputModel SetSuggestions(IReadOnlyList<string> suggestions)
    {
        var model = this with { Suggestions = suggestions };
        return model.UpdateMatchedSuggestions();
    }

    /// <summary>
    /// Gets all available suggestions.
    /// </summary>
    public IReadOnlyList<string> AvailableSuggestions() => Suggestions;

    /// <summary>
    /// Gets the suggestions that match the current input value.
    /// </summary>
    public IReadOnlyList<string> MatchedSuggestions()
    {
        return MatchedSuggestionIndices.Select(i => Suggestions[i]).ToList();
    }

    /// <summary>
    /// Gets the currently selected suggestion, or null if none.
    /// </summary>
    public string? CurrentSuggestion()
    {
        if (MatchedSuggestionIndices.Count == 0)
        {
            return null;
        }

        var idx = Math.Clamp(SelectedSuggestionIndex, 0, MatchedSuggestionIndices.Count - 1);
        return Suggestions[MatchedSuggestionIndices[idx]];
    }

    /// <summary>
    /// Gets the index of the currently selected suggestion within matched suggestions.
    /// </summary>
    public int CurrentSuggestionIndex() => SelectedSuggestionIndex;

    private TextInputModel UpdateMatchedSuggestions()
    {
        var value = GetValue();
        if (string.IsNullOrEmpty(value) || Suggestions.Count == 0)
        {
            return this with { MatchedSuggestionIndices = [], SelectedSuggestionIndex = 0 };
        }

        var matches = new List<int>();
        for (var i = 0; i < Suggestions.Count; i++)
        {
            if (Suggestions[i].StartsWith(value, StringComparison.OrdinalIgnoreCase) && !string.Equals(Suggestions[i], value, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(i);
            }
        }

        return this with { MatchedSuggestionIndices = matches, SelectedSuggestionIndex = 0 };
    }

    private TextInputModel AcceptSuggestion()
    {
        var suggestion = CurrentSuggestion();
        if (suggestion is null)
        {
            return this;
        }

        return SetValue(suggestion);
    }

    private TextInputModel PrevSuggestion()
    {
        if (MatchedSuggestionIndices.Count == 0)
        {
            return this;
        }

        var prev = SelectedSuggestionIndex - 1;
        if (prev < 0)
        {
            prev = MatchedSuggestionIndices.Count - 1;
        }

        return this with { SelectedSuggestionIndex = prev };
    }

    private (TextInputModel Model, Command? Command) HandleKey(KeyMessage km)
    {
        // Tab: accept suggestion or cycle through suggestions
        if (km.Key == Key.Tab && ShowSuggestions && MatchedSuggestionIndices.Count > 0)
        {
            return (AcceptSuggestion(), null);
        }

        if (km.Key == Key.ShiftTab && ShowSuggestions && MatchedSuggestionIndices.Count > 0)
        {
            return (PrevSuggestion(), null);
        }

        // Character input
        if (km is { Key: Key.Char, Runes.Length: > 0 })
        {
            return InsertRunes(km.Runes);
        }

        if (km.Key == Key.Space)
        {
            return InsertRunes([new Rune(' ')]);
        }

        // Cursor movement
        if (KeyMap.CharForward.Matches(km) && !km.Alt)
        {
            return (MoveForward(), null);
        }

        if (KeyMap.CharBackward.Matches(km) && !km.Alt)
        {
            return (MoveBackward(), null);
        }

        // Word movement (Alt + arrow or Alt + f/b/d)
        if (km.Alt && KeyMap.WordForward.Matches(km))
        {
            return (MoveWordForward(), null);
        }

        if (km.Alt && KeyMap.WordBackward.Matches(km))
        {
            return (MoveWordBackward(), null);
        }

        if (km.Alt && KeyMap.DeleteWordForward.Matches(km))
        {
            return (DeleteWordForward(), null);
        }

        if (KeyMap.LineStart.Matches(km))
        {
            return (MoveToStart(), null);
        }

        if (KeyMap.LineEnd.Matches(km))
        {
            return (MoveToEnd(), null);
        }

        // Deletion
        if (KeyMap.DeleteCharBackward.Matches(km))
        {
            return (DeleteBackward(), null);
        }

        if (KeyMap.DeleteCharForward.Matches(km) && !km.Alt)
        {
            return (DeleteForward(), null);
        }

        if (KeyMap.DeleteWordBackward.Matches(km))
        {
            return (DeleteWordBackward(), null);
        }

        if (KeyMap.DeleteToLineStart.Matches(km))
        {
            return (DeleteToStart(), null);
        }

        if (KeyMap.DeleteToLineEnd.Matches(km))
        {
            return (DeleteToEnd(), null);
        }

        return (this, null);
    }

    private (TextInputModel Model, Command? Command) InsertRunes(Rune[] runes)
    {
        var newValue = Value;
        var newPos = Position;

        foreach (var rune in runes)
        {
            if (CharLimit > 0 && newValue.Length >= CharLimit)
            {
                break;
            }

            newValue = newValue.Insert(newPos, rune);
            newPos++;
        }

        // Validate the new value
        var candidate = this with { Value = newValue, Position = newPos };
        if (Validate is not null && !Validate(candidate.GetValue()))
        {
            return (this, null);
        }

        candidate = candidate.AdjustOffset();
        candidate = candidate.UpdateMatchedSuggestions();
        return (candidate.UpdateCursorChar(), null);
    }

    private TextInputModel MoveForward()
    {
        if (Position >= Value.Length)
        {
            return this;
        }

        var model = this with { Position = Position + 1 };
        model = model.AdjustOffset();
        return model.UpdateCursorChar();
    }

    private TextInputModel MoveBackward()
    {
        if (Position <= 0)
        {
            return this;
        }

        var model = this with { Position = Position - 1 };
        model = model.AdjustOffset();
        return model.UpdateCursorChar();
    }

    private TextInputModel MoveWordForward()
    {
        if (Position >= Value.Length)
        {
            return this;
        }

        var pos = Position;

        // Skip current word characters
        while (pos < Value.Length && !Rune.IsWhiteSpace(Value[pos]))
        {
            pos++;
        }

        // Skip whitespace
        while (pos < Value.Length && Rune.IsWhiteSpace(Value[pos]))
        {
            pos++;
        }

        var model = this with { Position = pos };
        model = model.AdjustOffset();
        return model.UpdateCursorChar();
    }

    private TextInputModel MoveWordBackward()
    {
        if (Position <= 0)
        {
            return this;
        }

        var pos = Position;

        // Skip whitespace behind cursor
        while (pos > 0 && Rune.IsWhiteSpace(Value[pos - 1]))
        {
            pos--;
        }

        // Skip word characters
        while (pos > 0 && !Rune.IsWhiteSpace(Value[pos - 1]))
        {
            pos--;
        }

        var model = this with { Position = pos };
        model = model.AdjustOffset();
        return model.UpdateCursorChar();
    }

    private TextInputModel MoveToStart()
    {
        var model = this with { Position = 0 };
        model = model.AdjustOffset();
        return model.UpdateCursorChar();
    }

    private TextInputModel MoveToEnd()
    {
        var model = this with { Position = Value.Length };
        model = model.AdjustOffset();
        return model.UpdateCursorChar();
    }

    private TextInputModel DeleteBackward()
    {
        if (Position <= 0 || Value.IsEmpty)
        {
            return this;
        }

        var newValue = Value.RemoveAt(Position - 1);
        var candidate = this with { Value = newValue, Position = Position - 1 };

        if (Validate is not null && !Validate(candidate.GetValue()))
        {
            return this;
        }

        candidate = candidate.AdjustOffset();
        return candidate.UpdateCursorChar();
    }

    private TextInputModel DeleteForward()
    {
        if (Position >= Value.Length)
        {
            return this;
        }

        var newValue = Value.RemoveAt(Position);
        var candidate = this with { Value = newValue };

        if (Validate is not null && !Validate(candidate.GetValue()))
        {
            return this;
        }

        candidate = candidate.AdjustOffset();
        return candidate.UpdateCursorChar();
    }

    private TextInputModel DeleteWordBackward()
    {
        if (Position <= 0)
        {
            return this;
        }

        var start = Position;

        // Skip whitespace behind cursor
        while (start > 0 && Rune.IsWhiteSpace(Value[start - 1]))
        {
            start--;
        }

        // Skip word characters
        while (start > 0 && !Rune.IsWhiteSpace(Value[start - 1]))
        {
            start--;
        }

        var newValue = Value.RemoveRange(start, Position - start);
        var candidate = this with { Value = newValue, Position = start };

        if (Validate is not null && !Validate(candidate.GetValue()))
        {
            return this;
        }

        candidate = candidate.AdjustOffset();
        return candidate.UpdateCursorChar();
    }

    private TextInputModel DeleteWordForward()
    {
        if (Position >= Value.Length)
        {
            return this;
        }

        var end = Position;

        // Skip current word characters
        while (end < Value.Length && !Rune.IsWhiteSpace(Value[end]))
        {
            end++;
        }

        // Skip whitespace
        while (end < Value.Length && Rune.IsWhiteSpace(Value[end]))
        {
            end++;
        }

        var newValue = Value.RemoveRange(Position, end - Position);
        var candidate = this with { Value = newValue };

        if (Validate is not null && !Validate(candidate.GetValue()))
        {
            return this;
        }

        candidate = candidate.AdjustOffset();
        return candidate.UpdateCursorChar();
    }

    private TextInputModel DeleteToStart()
    {
        if (Position <= 0)
        {
            return this;
        }

        var newValue = Value.RemoveRange(0, Position);
        var candidate = this with { Value = newValue, Position = 0 };

        if (Validate is not null && !Validate(candidate.GetValue()))
        {
            return this;
        }

        candidate = candidate.AdjustOffset();
        return candidate.UpdateCursorChar();
    }

    private TextInputModel DeleteToEnd()
    {
        if (Position >= Value.Length)
        {
            return this;
        }

        var newValue = Value.RemoveRange(Position, Value.Length - Position);
        var candidate = this with { Value = newValue };

        if (Validate is not null && !Validate(candidate.GetValue()))
        {
            return this;
        }

        candidate = candidate.AdjustOffset();
        return candidate.UpdateCursorChar();
    }

    private TextInputModel AdjustOffset()
    {
        var promptWidth = CalculateStringWidth(Prompt);
        var baseWidth = Width > 0 ? Width : MinWidth;
        var effectiveWidth = MaxWidth > 0 ? Math.Min(baseWidth, MaxWidth) : baseWidth;
        var availableWidth = effectiveWidth - promptWidth;
        if (availableWidth <= 0)
        {
            return this with { Offset = 0 };
        }

        var offset = Offset;

        // If cursor is before the visible window, scroll left
        if (Position < offset)
        {
            offset = Position;
        }

        // If cursor is beyond the visible window, scroll right
        var cursorDisplayWidth = CalculateRuneWidthFromOffset(offset, Position);
        if (cursorDisplayWidth >= availableWidth)
        {
            // Move offset forward until cursor fits
            offset = Position;
            var width = 0;
            while (offset > 0)
            {
                var runeWidth = RuneDisplayWidth(offset - 1);
                if (width + runeWidth >= availableWidth)
                {
                    break;
                }

                width += runeWidth;
                offset--;
            }
        }

        return this with { Offset = offset };
    }

    private TextInputModel UpdateCursorChar()
    {
        var ch = Position < Value.Length
            ? GetDisplayChar(Position)
            : " ";
        var style = Position < Value.Length ? TextStyle : Appearance.Plain;
        return this with { Cursor = Cursor.SetChar(ch, style) };
    }

    private string GetDisplayChar(int runeIndex)
    {
        if (runeIndex < 0 || runeIndex >= Value.Length)
        {
            return " ";
        }

        return EchoMode switch
        {
            EchoMode.Password => EchoCharacter.ToString(),
            EchoMode.None => string.Empty,
            _ => Value[runeIndex].ToString(),
        };
    }

    private ImmutableArray<Rune> GetVisibleRunes(int availableWidth)
    {
        var result = ImmutableArray.CreateBuilder<Rune>();
        var width = 0;

        for (var i = Offset; i < Value.Length; i++)
        {
            var runeWidth = RuneDisplayWidth(i);
            if (width + runeWidth > availableWidth)
            {
                break;
            }

            result.Add(Value[i]);
            width += runeWidth;
        }

        return result.ToImmutable();
    }

    private int CalculateRuneWidthFromOffset(int from, int to)
    {
        var width = 0;
        for (var i = from; i < to && i < Value.Length; i++)
        {
            width += RuneDisplayWidth(i);
        }

        return width;
    }

    private int RuneDisplayWidth(int runeIndex)
    {
        if (EchoMode == EchoMode.Password)
        {
            return 1;
        }

        if (EchoMode == EchoMode.None)
        {
            return 0;
        }

        return Math.Max(1, UnicodeWidth(Value[runeIndex].ToString()));
    }

    private static int CalculateStringWidth(string text)
    {
        var width = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            width += Math.Max(1, UnicodeWidth(rune.ToString()));
        }

        return width;
    }

    private static int UnicodeWidth(string text)
    {
        var width = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            width += Wcwidth.UnicodeCalculator.GetWidth(rune) switch
            {
                -1 => 0,
                var w => w,
            };
        }

        return Math.Max(width, text.Length > 0 ? 1 : 0);
    }

    private static string TruncateToWidth(string text, int maxWidth)
    {
        var sb = new StringBuilder();
        var width = 0;

        foreach (var rune in text.EnumerateRunes())
        {
            var runeWidth = Math.Max(1, Wcwidth.UnicodeCalculator.GetWidth(rune) switch
            {
                -1 => 0,
                var w => w,
            });

            if (width + runeWidth > maxWidth)
            {
                break;
            }

            sb.Append(rune);
            width += runeWidth;
        }

        return sb.ToString();
    }

    private static IEnumerable<Rune> EnumerateRunes(string text)
    {
        return text.EnumerateRunes();
    }
}
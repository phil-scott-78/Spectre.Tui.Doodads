using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.TextInput;

/// <summary>
/// Key bindings for the text input doodad.
/// </summary>
public record TextInputKeyMap : IKeyMap
{
    /// <summary>
    /// Gets the binding for moving the cursor one character forward.
    /// </summary>
    public KeyBinding CharForward { get; init; } = KeyBinding.For(Key.Right, Key.CtrlF)
        .WithAlt(false)
        .WithHelp("→/ctrl+f", "character forward");

    /// <summary>
    /// Gets the binding for moving the cursor one character backward.
    /// </summary>
    public KeyBinding CharBackward { get; init; } = KeyBinding.For(Key.Left, Key.CtrlB)
        .WithAlt(false)
        .WithHelp("←/ctrl+b", "character backward");

    /// <summary>
    /// Gets the binding for moving the cursor one word forward.
    /// </summary>
    public KeyBinding WordForward { get; init; } = KeyBinding.For(Key.Right, Key.Char)
        .WithRunes(new Rune('f'))
        .WithAlt()
        .WithHelp("alt+→/alt+f", "word forward");

    /// <summary>
    /// Gets the binding for moving the cursor one word backward.
    /// </summary>
    public KeyBinding WordBackward { get; init; } = KeyBinding.For(Key.Left, Key.Char)
        .WithRunes(new Rune('b'))
        .WithAlt()
        .WithHelp("alt+←/alt+b", "word backward");

    /// <summary>
    /// Gets the binding for moving the cursor to the start of the line.
    /// </summary>
    public KeyBinding LineStart { get; init; } = KeyBinding.For(Key.Home, Key.CtrlA)
        .WithHelp("home/ctrl+a", "line start");

    /// <summary>
    /// Gets the binding for moving the cursor to the end of the line.
    /// </summary>
    public KeyBinding LineEnd { get; init; } = KeyBinding.For(Key.End, Key.CtrlE)
        .WithHelp("end/ctrl+e", "line end");

    /// <summary>
    /// Gets the binding for deleting the character before the cursor.
    /// </summary>
    public KeyBinding DeleteCharBackward { get; init; } = KeyBinding.For(Key.Backspace)
        .WithHelp("backspace", "delete character backward");

    /// <summary>
    /// Gets the binding for deleting the character after the cursor.
    /// </summary>
    public KeyBinding DeleteCharForward { get; init; } = KeyBinding.For(Key.Delete, Key.CtrlD)
        .WithAlt(false)
        .WithHelp("del/ctrl+d", "delete character forward");

    /// <summary>
    /// Gets the binding for deleting the word before the cursor.
    /// </summary>
    public KeyBinding DeleteWordBackward { get; init; } = KeyBinding.For(Key.CtrlW)
        .WithHelp("ctrl+w", "delete word backward");

    /// <summary>
    /// Gets the binding for deleting the word after the cursor.
    /// </summary>
    public KeyBinding DeleteWordForward { get; init; } = KeyBinding.For(Key.Delete, Key.Char)
        .WithRunes(new Rune('d'))
        .WithAlt()
        .WithHelp("alt+d", "delete word forward");

    /// <summary>
    /// Gets the binding for deleting from cursor to the start of the line.
    /// </summary>
    public KeyBinding DeleteToLineStart { get; init; } = KeyBinding.For(Key.CtrlU)
        .WithHelp("ctrl+u", "delete to line start");

    /// <summary>
    /// Gets the binding for deleting from cursor to the end of the line.
    /// </summary>
    public KeyBinding DeleteToLineEnd { get; init; } = KeyBinding.For(Key.CtrlK)
        .WithHelp("ctrl+k", "delete to line end");

    /// <summary>
    /// Gets the binding for pasting from clipboard.
    /// </summary>
    public KeyBinding Paste { get; init; } = KeyBinding.For(Key.CtrlV)
        .WithHelp("ctrl+v", "paste");

    /// <inheritdoc />
    public IEnumerable<KeyBinding> ShortHelp()
    {
        yield return CharForward;
        yield return CharBackward;
        yield return LineStart;
        yield return LineEnd;
        yield return DeleteCharBackward;
    }

    /// <inheritdoc />
    public IEnumerable<IEnumerable<KeyBinding>> FullHelp()
    {
        yield return
        [
            CharForward,
            CharBackward,
            WordForward,
            WordBackward,
            LineStart,
            LineEnd,
        ];

        yield return
        [
            DeleteCharBackward,
            DeleteCharForward,
            DeleteWordBackward,
            DeleteWordForward,
            DeleteToLineStart,
            DeleteToLineEnd,
        ];
    }
}
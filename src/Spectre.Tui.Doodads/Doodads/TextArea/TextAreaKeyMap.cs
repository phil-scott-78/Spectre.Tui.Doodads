using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.TextArea;

/// <summary>
/// Key bindings for the text area doodad.
/// </summary>
public sealed class TextAreaKeyMap : IKeyMap
{
    public KeyBinding CharacterForward { get; init; } = KeyBinding.For(Key.Right, Key.CtrlF)
        .WithHelp("→", "move right");

    public KeyBinding CharacterBackward { get; init; } = KeyBinding.For(Key.Left, Key.CtrlB)
        .WithHelp("←", "move left");

    public KeyBinding LineUp { get; init; } = KeyBinding.For(Key.Up, Key.CtrlP)
        .WithHelp("↑", "move up");

    public KeyBinding LineDown { get; init; } = KeyBinding.For(Key.Down, Key.CtrlN)
        .WithHelp("↓", "move down");

    public KeyBinding WordForward { get; init; } = KeyBinding.For(Key.Right, Key.Char)
        .WithRunes(new Rune('f'))
        .WithAlt()
        .WithHelp("alt+→/alt+f", "word forward");

    public KeyBinding WordBackward { get; init; } = KeyBinding.For(Key.Left, Key.Char)
        .WithRunes(new Rune('b'))
        .WithAlt()
        .WithHelp("alt+←/alt+b", "word backward");

    public KeyBinding DeleteWordBackward { get; init; } = KeyBinding.For(Key.CtrlW)
        .WithHelp("ctrl+w", "delete word backward");

    public KeyBinding DeleteWordForward { get; init; } = KeyBinding.For(Key.Delete, Key.Char)
        .WithRunes(new Rune('d'))
        .WithAlt()
        .WithHelp("alt+d", "delete word forward");

    public KeyBinding LineStart { get; init; } = KeyBinding.For(Key.Home, Key.CtrlA)
        .WithHelp("home", "line start");

    public KeyBinding LineEnd { get; init; } = KeyBinding.For(Key.End, Key.CtrlE)
        .WithHelp("end", "line end");

    public KeyBinding DeleteCharForward { get; init; } = KeyBinding.For(Key.Delete, Key.CtrlD)
        .WithHelp("del", "delete char");

    public KeyBinding DeleteCharBackward { get; init; } = KeyBinding.For(Key.Backspace, Key.CtrlH)
        .WithHelp("bksp", "delete back");

    public KeyBinding DeleteToEnd { get; init; } = KeyBinding.For(Key.CtrlK)
        .WithHelp("ctrl+k", "delete to end");

    public KeyBinding DeleteToStart { get; init; } = KeyBinding.For(Key.CtrlU)
        .WithHelp("ctrl+u", "delete to start");

    public KeyBinding InsertNewline { get; init; } = KeyBinding.For(Key.Enter)
        .WithHelp("enter", "new line");

    public KeyBinding PageUp { get; init; } = KeyBinding.For(Key.PageUp)
        .WithHelp("pgup", "page up");

    public KeyBinding PageDown { get; init; } = KeyBinding.For(Key.PageDown)
        .WithHelp("pgdn", "page down");

    public KeyBinding UppercaseWordForward { get; init; } = KeyBinding.For(Key.Char)
        .WithRunes(new Rune('u'))
        .WithAlt()
        .WithHelp("alt+u", "uppercase word");

    public KeyBinding LowercaseWordForward { get; init; } = KeyBinding.For(Key.Char)
        .WithRunes(new Rune('l'))
        .WithAlt()
        .WithHelp("alt+l", "lowercase word");

    public KeyBinding CapitalizeWordForward { get; init; } = KeyBinding.For(Key.Char)
        .WithRunes(new Rune('c'))
        .WithAlt()
        .WithHelp("alt+c", "capitalize word");

    public KeyBinding TransposeCharacterBackward { get; init; } = KeyBinding.For(Key.CtrlT)
        .WithHelp("ctrl+t", "transpose chars");

    public KeyBinding InputBegin { get; init; } = KeyBinding.For(Key.Home)
        .WithCtrl()
        .WithHelp("ctrl+home", "input begin");

    public KeyBinding InputEnd { get; init; } = KeyBinding.For(Key.End)
        .WithCtrl()
        .WithHelp("ctrl+end", "input end");

    public KeyBinding Paste { get; init; } = KeyBinding.For(Key.CtrlV)
        .WithHelp("ctrl+v", "paste");

    public IEnumerable<KeyBinding> ShortHelp()
    {
        yield return CharacterForward;
        yield return CharacterBackward;
        yield return LineUp;
        yield return LineDown;
        yield return InsertNewline;
    }

    public IEnumerable<IEnumerable<KeyBinding>> FullHelp()
    {
        yield return
        [
            CharacterForward, CharacterBackward, LineUp, LineDown,
            WordForward, WordBackward, LineStart, LineEnd, PageUp, PageDown,
        ];
        yield return
        [
            DeleteCharForward, DeleteCharBackward, DeleteWordForward, DeleteWordBackward,
            DeleteToEnd, DeleteToStart, InsertNewline,
        ];
    }
}
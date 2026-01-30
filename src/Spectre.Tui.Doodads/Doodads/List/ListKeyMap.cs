using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// Key bindings for the list doodad.
/// </summary>
public record ListKeyMap : IKeyMap
{
    /// <summary>
    /// Gets the binding for moving the cursor up.
    /// </summary>
    public KeyBinding CursorUp { get; init; } = KeyBinding.For(Key.Up, Key.Char)
        .WithRunes(new Rune('k'))
        .WithHelp("\u2191/k", "up");

    /// <summary>
    /// Gets the binding for moving the cursor down.
    /// </summary>
    public KeyBinding CursorDown { get; init; } = KeyBinding.For(Key.Down, Key.Char)
        .WithRunes(new Rune('j'))
        .WithHelp("\u2193/j", "down");

    /// <summary>
    /// Gets the binding for jumping to the first item.
    /// </summary>
    public KeyBinding GoToStart { get; init; } = KeyBinding.For(Key.Home, Key.Char)
        .WithRunes(new Rune('g'))
        .WithHelp("home/g", "go to start");

    /// <summary>
    /// Gets the binding for jumping to the last item.
    /// </summary>
    public KeyBinding GoToEnd { get; init; } = KeyBinding.For(Key.End, Key.Char)
        .WithRunes(new Rune('G'))
        .WithHelp("end/G", "go to end");

    /// <summary>
    /// Gets the binding for entering filter mode.
    /// </summary>
    public KeyBinding StartFilter { get; init; } = KeyBinding.For(Key.Char)
        .WithRunes(new Rune('/'))
        .WithHelp("/", "filter");

    /// <summary>
    /// Gets the binding for accepting the current filter.
    /// </summary>
    public KeyBinding AcceptFilter { get; init; } = KeyBinding.For(Key.Enter)
        .WithHelp("enter", "apply");

    /// <summary>
    /// Gets the binding for canceling the current filter.
    /// </summary>
    public KeyBinding CancelFilter { get; init; } = KeyBinding.For(Key.Escape)
        .WithHelp("esc", "cancel");

    /// <summary>
    /// Gets the binding for navigating to the next page.
    /// </summary>
    public KeyBinding NextPage { get; init; } = KeyBinding.For(Key.Right, Key.PageDown)
        .WithHelp("\u2192/pgdn", "next page");

    /// <summary>
    /// Gets the binding for navigating to the previous page.
    /// </summary>
    public KeyBinding PrevPage { get; init; } = KeyBinding.For(Key.Left, Key.PageUp)
        .WithHelp("\u2190/pgup", "prev page");

    /// <summary>
    /// Gets the binding for toggling help display.
    /// </summary>
    public KeyBinding ToggleHelp { get; init; } = KeyBinding.For(Key.Char)
        .WithRunes(new Rune('?'))
        .WithHelp("?", "toggle help");

    /// <summary>
    /// Gets the binding for quitting.
    /// </summary>
    public KeyBinding Quit { get; init; } = KeyBinding.For(Key.CtrlC, Key.Char)
        .WithRunes(new Rune('q'))
        .WithHelp("q/ctrl+c", "quit");

    /// <summary>
    /// Gets the binding for clearing the current filter.
    /// </summary>
    public KeyBinding ClearFilter { get; init; } = KeyBinding.For(Key.Escape)
        .WithHelp("esc", "clear filter");

    /// <summary>
    /// Gets the binding for showing full help.
    /// </summary>
    public KeyBinding ShowFullHelp { get; init; } = KeyBinding.For(Key.Char)
        .WithRunes(new Rune('?'))
        .WithHelp("?", "more help");

    /// <summary>
    /// Gets the binding for closing full help.
    /// </summary>
    public KeyBinding CloseFullHelp { get; init; } = KeyBinding.For(Key.Escape)
        .WithHelp("esc", "close help");

    /// <summary>
    /// Gets the binding for force quitting.
    /// </summary>
    public KeyBinding ForceQuit { get; init; } = KeyBinding.For(Key.CtrlC)
        .WithHelp("ctrl+c", "force quit");

    /// <inheritdoc />
    public IEnumerable<KeyBinding> ShortHelp()
    {
        yield return CursorUp;
        yield return CursorDown;
        yield return StartFilter;
        yield return ToggleHelp;
        yield return Quit;
    }

    /// <inheritdoc />
    public IEnumerable<IEnumerable<KeyBinding>> FullHelp()
    {
        yield return [CursorUp, CursorDown, GoToStart, GoToEnd];
        yield return [NextPage, PrevPage];
        yield return [StartFilter, AcceptFilter, CancelFilter, ClearFilter];
        yield return [ToggleHelp, ShowFullHelp, CloseFullHelp];
        yield return [Quit, ForceQuit];
    }
}
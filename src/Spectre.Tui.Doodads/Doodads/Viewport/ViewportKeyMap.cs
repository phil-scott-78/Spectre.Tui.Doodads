using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.Viewport;

/// <summary>
/// Key bindings for the viewport doodad.
/// </summary>
public record ViewportKeyMap : IKeyMap
{
    /// <summary>
    /// Gets the binding for scrolling down one page.
    /// </summary>
    public KeyBinding PageDown { get; init; } = KeyBinding.For(Key.PageDown)
        .WithHelp("pgdn", "page down");

    /// <summary>
    /// Gets the binding for scrolling up one page.
    /// </summary>
    public KeyBinding PageUp { get; init; } = KeyBinding.For(Key.PageUp)
        .WithHelp("pgup", "page up");

    /// <summary>
    /// Gets the binding for scrolling down half a page.
    /// </summary>
    public KeyBinding HalfPageDown { get; init; } = KeyBinding.For(Key.CtrlD)
        .WithHelp("ctrl+d", "half page down");

    /// <summary>
    /// Gets the binding for scrolling up half a page.
    /// </summary>
    public KeyBinding HalfPageUp { get; init; } = KeyBinding.For(Key.CtrlU)
        .WithHelp("ctrl+u", "half page up");

    /// <summary>
    /// Gets the binding for scrolling down one line.
    /// </summary>
    public KeyBinding LineDown { get; init; } = KeyBinding.For(Key.Down)
        .WithHelp("\u2193", "line down");

    /// <summary>
    /// Gets the binding for scrolling up one line.
    /// </summary>
    public KeyBinding LineUp { get; init; } = KeyBinding.For(Key.Up)
        .WithHelp("\u2191", "line up");

    /// <summary>
    /// Gets the binding for scrolling left.
    /// </summary>
    public KeyBinding Left { get; init; } = KeyBinding.For(Key.Left)
        .WithHelp("\u2190", "scroll left");

    /// <summary>
    /// Gets the binding for scrolling right.
    /// </summary>
    public KeyBinding Right { get; init; } = KeyBinding.For(Key.Right)
        .WithHelp("\u2192", "scroll right");

    /// <summary>
    /// Gets the binding for scrolling to the top.
    /// </summary>
    public KeyBinding Home { get; init; } = KeyBinding.For(Key.Home)
        .WithHelp("home", "go to top");

    /// <summary>
    /// Gets the binding for scrolling to the bottom.
    /// </summary>
    public KeyBinding End { get; init; } = KeyBinding.For(Key.End)
        .WithHelp("end", "go to bottom");

    /// <inheritdoc />
    public IEnumerable<KeyBinding> ShortHelp()
    {
        yield return PageDown;
        yield return PageUp;
        yield return LineDown;
        yield return LineUp;
        yield return Home;
        yield return End;
    }

    /// <inheritdoc />
    public IEnumerable<IEnumerable<KeyBinding>> FullHelp()
    {
        yield return [LineUp, LineDown, Left, Right];
        yield return [HalfPageUp, HalfPageDown, PageUp, PageDown];
        yield return [Home, End];
    }
}
using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.Table;

/// <summary>
/// Key bindings for the table doodad.
/// </summary>
public record TableKeyMap : IKeyMap
{
    /// <summary>
    /// Gets the binding for moving the selection up one row.
    /// </summary>
    public KeyBinding LineUp { get; init; } = KeyBinding.For(Key.Up)
        .WithHelp("\u2191/k", "up");

    /// <summary>
    /// Gets the binding for moving the selection down one row.
    /// </summary>
    public KeyBinding LineDown { get; init; } = KeyBinding.For(Key.Down)
        .WithHelp("\u2193/j", "down");

    /// <summary>
    /// Gets the binding for scrolling up one full page.
    /// </summary>
    public KeyBinding PageUp { get; init; } = KeyBinding.For(Key.PageUp, Key.CtrlB)
        .WithHelp("pgup/ctrl+b", "page up");

    /// <summary>
    /// Gets the binding for scrolling down one full page.
    /// </summary>
    public KeyBinding PageDown { get; init; } = KeyBinding.For(Key.PageDown, Key.CtrlF)
        .WithHelp("pgdn/ctrl+f", "page down");

    /// <summary>
    /// Gets the binding for scrolling up half a page.
    /// </summary>
    public KeyBinding HalfPageUp { get; init; } = KeyBinding.For(Key.CtrlU)
        .WithHelp("ctrl+u", "half page up");

    /// <summary>
    /// Gets the binding for scrolling down half a page.
    /// </summary>
    public KeyBinding HalfPageDown { get; init; } = KeyBinding.For(Key.CtrlD)
        .WithHelp("ctrl+d", "half page down");

    /// <summary>
    /// Gets the binding for jumping to the first row.
    /// </summary>
    public KeyBinding GoToTop { get; init; } = KeyBinding.For(Key.Home)
        .WithHelp("home/g", "go to top");

    /// <summary>
    /// Gets the binding for jumping to the last row.
    /// </summary>
    public KeyBinding GoToBottom { get; init; } = KeyBinding.For(Key.End)
        .WithHelp("end/G", "go to bottom");

    /// <inheritdoc />
    public IEnumerable<KeyBinding> ShortHelp()
    {
        yield return LineUp;
        yield return LineDown;
        yield return PageUp;
        yield return PageDown;
        yield return GoToTop;
        yield return GoToBottom;
    }

    /// <inheritdoc />
    public IEnumerable<IEnumerable<KeyBinding>> FullHelp()
    {
        yield return [LineUp, LineDown];
        yield return [HalfPageUp, HalfPageDown, PageUp, PageDown];
        yield return [GoToTop, GoToBottom];
    }
}
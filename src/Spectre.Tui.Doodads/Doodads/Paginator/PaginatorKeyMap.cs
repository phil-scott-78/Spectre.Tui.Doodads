using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.Paginator;

/// <summary>
/// Key bindings for the paginator doodad.
/// </summary>
public record PaginatorKeyMap : IKeyMap
{
    /// <summary>
    /// Gets the binding for navigating to the previous page.
    /// </summary>
    public KeyBinding PrevPage { get; init; } = KeyBinding.For(Key.Left, Key.PageUp)
        .WithHelp("\u2190/pgup", "prev page");

    /// <summary>
    /// Gets the binding for navigating to the next page.
    /// </summary>
    public KeyBinding NextPage { get; init; } = KeyBinding.For(Key.Right, Key.PageDown)
        .WithHelp("\u2192/pgdn", "next page");

    /// <inheritdoc />
    public IEnumerable<KeyBinding> ShortHelp()
    {
        yield return PrevPage;
        yield return NextPage;
    }

    /// <inheritdoc />
    public IEnumerable<IEnumerable<KeyBinding>> FullHelp()
    {
        yield return [PrevPage, NextPage];
    }
}
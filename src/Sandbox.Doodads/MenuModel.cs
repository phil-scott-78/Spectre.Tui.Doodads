using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.List;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// A menu entry for the example launcher.
/// </summary>
public record MenuEntry(string Name, string Description) : IListItemWithDescription
{
    public string FilterValue => Name;
    public string Title => Name;
}

/// <summary>
/// The result of selecting a menu entry.
/// </summary>
public record MenuSelection(int Index);

/// <summary>
/// Menu model for choosing which example to run.
/// </summary>
public record MenuModel : IDoodad<MenuModel>, ISizedRenderable
{
    public int MinWidth => 50;
    public int MinHeight => 21;

    /// <summary>
    /// Gets the list of menu entries.
    /// </summary>
    public ListModel<MenuEntry> List { get; init; } = new ListModel<MenuEntry>()
    {
        Title = "Spectre.Tui Doodads - Examples",
        MinWidth = 50,
        MinHeight = 20,
        ShowFilter = false,
        ShowHelp = false,
        ShowPagination = false,
        ShowStatusBar = false,
        FilteringEnabled = false,
        Delegate = new DefaultListItemDelegate(),
    }.SetItems([
        new MenuEntry("Counter", "A simple counter with up/down keys"),
        new MenuEntry("Todo List", "A todo list with add, toggle, and delete"),
        new MenuEntry("Text Editor", "A multi-line text editor with status bar"),
        new MenuEntry("Dashboard", "A multi-panel display dashboard"),
        new MenuEntry("Flex Layout", "Flex layout boxes showing dimensions"),
        new MenuEntry("Form", "A multi-input form with validation"),
        new MenuEntry("Speed Test", "Network speed test with live progress"),
    ]);

    /// <summary>
    /// Gets the selected example index, or null if no selection has been made.
    /// </summary>
    public int? SelectedExample { get; init; }

    /// <inheritdoc />
    public Command Init()
    {
        return List.Init();
    }

    /// <inheritdoc />
    public (MenuModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case KeyMessage { Key: Key.Enter }:
                var selected = List.SelectedIndex;
                return (this with { SelectedExample = selected }, Commands.Quit());

            case KeyMessage { Key: Key.Escape }:
            case KeyMessage { Key: Key.CtrlC }:
                return (this, Commands.Quit());

            case KeyMessage { Char: 'q' }:
                return (this, Commands.Quit());

            default:
                var (updatedList, listCmd) = List.Update(message);
                return (this with { List = updatedList }, listCmd);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        var listHeight = Math.Max(0, height - 1);

        surface.Render(List, new Rectangle(0, 0, width, listHeight));

        var helpY = listHeight;
        var helpStyle = new Appearance { Decoration = Decoration.Dim };
        surface.SetString(0, helpY, "Enter:select  q/Esc:quit", helpStyle);
    }
}
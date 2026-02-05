using System.Text;
using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Doodads.List;
using Spectre.Tui.Doodads.Doodads.TextInput;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// A todo item that can be displayed in a list.
/// </summary>
public record TodoItem(string Text, bool Done) : IListItemWithDescription
{
    public string FilterValue => Text;
    public string Title => Done ? $"[x] {Text}" : $"[ ] {Text}";
    public string Description => Done ? "Completed" : "Pending";
}

/// <summary>
/// A todo list example composing ListModel and TextInputModel.
/// </summary>
public record TodoModel : IDoodad<TodoModel>, ISizedRenderable
{
    public int MinWidth => 60;
    public int MinHeight => 22;

    /// <summary>
    /// Gets the list of todo items.
    /// </summary>
    public ListModel<TodoItem> List { get; init; } = new ListModel<TodoItem>()
    {
        Title = "Todo List",
        MinWidth = 60,
        MinHeight = 20,
        ShowFilter = true,
        ShowHelp = false,
        Delegate = new DefaultTodoItemDelegate(),
    }.SetItems([
        new TodoItem("Buy groceries", false),
        new TodoItem("Walk the dog", false),
        new TodoItem("Write documentation", true),
        new TodoItem("Review pull requests", false),
    ]);

    /// <summary>
    /// Gets the text input for adding new items.
    /// </summary>
    public TextInputModel Input { get; init; } = new()
    {
        Prompt = "New item: ",
        Placeholder = "Enter todo text...",
        MinWidth = 60,
    };

    /// <summary>
    /// Gets a value indicating whether the user is currently adding a new item.
    /// </summary>
    public bool Adding { get; init; }

    /// <inheritdoc />
    public Command Init()
    {
        return List.Init();
    }

    /// <inheritdoc />
    public (TodoModel Model, Command? Command) Update(Message message)
    {
        if (Adding)
        {
            return HandleAddingMode(message);
        }

        return HandleNormalMode(message);
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        var footerLines = Adding ? 2 : 1;
        var listHeight = Math.Max(0, height - footerLines);

        // Render the list
        if (listHeight > 0 && width > 0)
        {
            surface.Render(List, new Rectangle(0, 0, width, listHeight));
        }

        var y = listHeight;

        if (Adding)
        {
            surface.Render(Input, new Rectangle(0, y, width, 1));
            y++;
            surface.SetString(0, y, "Enter to confirm, Esc to cancel",
                new Appearance { Decoration = Decoration.Dim });
        }
        else
        {
            surface.SetString(0, y, "a:add  x:toggle  d:delete  q:quit",
                new Appearance { Decoration = Decoration.Dim });
        }
    }

    private (TodoModel Model, Command? Command) HandleNormalMode(Message message)
    {
        switch (message)
        {
            case KeyMessage { Key: Key.Char, Runes.Length: > 0 } km when km.Runes[0] == new Rune('a'):
                return EnterAddMode();

            case KeyMessage { Key: Key.Char, Runes.Length: > 0 } km when km.Runes[0] == new Rune('x'):
                return ToggleSelected();

            case KeyMessage { Key: Key.Char, Runes.Length: > 0 } km when km.Runes[0] == new Rune('d'):
                return DeleteSelected();

            case KeyMessage { Key: Key.Escape }:
            case KeyMessage { Key: Key.CtrlC }:
                return (this, Commands.Quit());

            default:
                return this.Forward(message, m => m.List, (m, v) => m with { List = v });
        }
    }

    private (TodoModel Model, Command? Command) HandleAddingMode(Message message)
    {
        switch (message)
        {
            case KeyMessage { Key: Key.Enter }:
                return ConfirmAdd();

            case KeyMessage { Key: Key.Escape }:
                return CancelAdd();

            default:
                return this.Forward(message, m => m.Input, (m, v) => m with { Input = v });
        }
    }

    private (TodoModel Model, Command? Command) EnterAddMode()
    {
        var (focusedInput, cmd) = Input.Focus();
        focusedInput = focusedInput.SetValue(string.Empty);
        return (this with { Adding = true, Input = focusedInput }, cmd);
    }

    private (TodoModel Model, Command? Command) ConfirmAdd()
    {
        var text = Input.GetValue().Trim();
        if (text.Length == 0)
        {
            return CancelAdd();
        }

        var newItem = new TodoItem(text, false);
        var allItems = List.Items.Append(newItem).ToList();
        var updatedList = List.SetItems(allItems);

        var (blurredInput, cmd) = Input.Blur();
        return (this with
        {
            Adding = false,
            Input = blurredInput,
            List = updatedList,
        }, cmd);
    }

    private (TodoModel Model, Command? Command) CancelAdd()
    {
        var (blurredInput, cmd) = Input.Blur();
        return (this with { Adding = false, Input = blurredInput }, cmd);
    }

    private (TodoModel Model, Command? Command) ToggleSelected()
    {
        var selected = List.SelectedItem;
        if (selected is null)
        {
            return (this, null);
        }

        var toggled = selected with { Done = !selected.Done };
        var items = List.Items.ToList();
        var index = List.SelectedIndex;
        items[index] = toggled;
        var updatedList = List.SetItems(items);
        updatedList = updatedList with { SelectedIndex = index };
        return (this with { List = updatedList }, null);
    }

    private (TodoModel Model, Command? Command) DeleteSelected()
    {
        if (List.Items.Count == 0)
        {
            return (this, null);
        }

        var items = List.Items.ToList();
        items.RemoveAt(List.SelectedIndex);
        var updatedList = List.SetItems(items);
        return (this with { List = updatedList }, null);
    }
}

/// <summary>
/// Custom delegate for rendering todo items.
/// </summary>
public sealed class DefaultTodoItemDelegate : IListItemDelegate<TodoItem>
{
    /// <inheritdoc />
    public int Height => 2;

    /// <inheritdoc />
    public int Spacing => 0;

    /// <inheritdoc />
    public void Render(IRenderSurface surface, TodoItem item, int index, bool selected)
    {
        var cursor = selected ? "> " : "  ";
        var title = new Label(cursor + item.Title, selected ? "bold" : "");

        surface.Layout($"""
            {title}
              {item.Description:dim}
            """);
    }
}
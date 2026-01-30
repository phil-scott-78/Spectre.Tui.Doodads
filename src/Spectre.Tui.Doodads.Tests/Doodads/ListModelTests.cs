using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Doodads.List;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class ListModelTests
{
    private record TestItem(string Name) : IListItem
    {
        public string FilterValue => Name;
    }

    private sealed class TestDelegate : IListItemDelegate<TestItem>
    {
        public int Height => 1;
        public int Spacing => 0;
        public void Render(IRenderSurface surface, TestItem item, int index, bool selected) { }
    }

    private static ListModel<TestItem> CreateList(params string[] names)
    {
        var items = names.Select(n => new TestItem(n)).ToList();
        var list = new ListModel<TestItem>
        {
            Delegate = new TestDelegate(),
            MinWidth = 40,
            MinHeight = 20,
        };
        return list.SetItems(items);
    }

    private static KeyMessage MakeKey(Key key, char? c = null, bool shift = false, bool ctrl = false)
    {
        var runes = c.HasValue ? [new Rune(c.Value)] : Array.Empty<Rune>();
        return new KeyMessage { Key = key, Runes = runes, Shift = shift, Ctrl = ctrl };
    }

    [Fact]
    public void SetItems_Should_Set_Items()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        list.Items.Count.ShouldBe(3);
    }

    [Fact]
    public void SetItems_Should_Reset_SelectedIndex()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");
        var (moved, _) = list.Update(MakeKey(Key.Down));
        moved.SelectedIndex.ShouldBe(1);

        var reset = moved.SetItems([new TestItem("X"), new TestItem("Y")]);
        reset.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void SetItems_Should_Reset_Pagination()
    {
        var list = CreateList("A", "B", "C");
        var reset = list.SetItems([new TestItem("X")]);

        reset.Paginator.Page.ShouldBe(0);
        reset.Paginator.TotalPages.ShouldBe(1);
    }

    [Fact]
    public void SelectedItem_Should_Return_Correct_Item()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        list.SelectedItem.ShouldNotBeNull();
        list.SelectedItem!.Name.ShouldBe("Alpha");
    }

    [Fact]
    public void SelectedItem_Should_Return_Default_When_Empty()
    {
        var list = CreateList();

        list.SelectedItem.ShouldBeNull();
    }

    [Fact]
    public void CursorDown_Should_Move_Selection_Down()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        var (updated, _) = list.Update(MakeKey(Key.Down));

        updated.SelectedIndex.ShouldBe(1);
        updated.SelectedItem!.Name.ShouldBe("Beta");
    }

    [Fact]
    public void CursorUp_Should_Move_Selection_Up()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");
        var (moved, _) = list.Update(MakeKey(Key.Down));
        moved.SelectedIndex.ShouldBe(1);

        var (updated, _) = moved.Update(MakeKey(Key.Up));

        updated.SelectedIndex.ShouldBe(0);
        updated.SelectedItem!.Name.ShouldBe("Alpha");
    }

    [Fact]
    public void CursorDown_Should_Wrap_To_Start()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        var (s1, _) = list.Update(MakeKey(Key.Down));
        var (s2, _) = s1.Update(MakeKey(Key.Down));
        var (s3, _) = s2.Update(MakeKey(Key.Down));

        s3.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void CursorUp_Should_Wrap_To_End()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        var (updated, _) = list.Update(MakeKey(Key.Up));

        updated.SelectedIndex.ShouldBe(2);
        updated.SelectedItem!.Name.ShouldBe("Gamma");
    }

    [Fact]
    public void GoToStart_Should_Move_To_First_Item()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");
        var (moved, _) = list.Update(MakeKey(Key.Down));
        var (moved2, _) = moved.Update(MakeKey(Key.Down));
        moved2.SelectedIndex.ShouldBe(2);

        var (updated, _) = moved2.Update(MakeKey(Key.Home));

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void GoToEnd_Should_Move_To_Last_Item()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        var (updated, _) = list.Update(MakeKey(Key.End));

        updated.SelectedIndex.ShouldBe(2);
        updated.SelectedItem!.Name.ShouldBe("Gamma");
    }

    [Fact]
    public void Vim_J_Should_Move_Down()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        var (updated, _) = list.Update(MakeKey(Key.Char, 'j'));

        updated.SelectedIndex.ShouldBe(1);
    }

    [Fact]
    public void Vim_K_Should_Move_Up()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");
        var (moved, _) = list.Update(MakeKey(Key.Down));

        var (updated, _) = moved.Update(MakeKey(Key.Char, 'k'));

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void Vim_G_Should_Go_To_Start()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");
        var (moved, _) = list.Update(MakeKey(Key.Down));

        var (updated, _) = moved.Update(MakeKey(Key.Char, 'g'));

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void Vim_Shift_G_Should_Go_To_End()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        var (updated, _) = list.Update(MakeKey(Key.Char, 'G'));

        updated.SelectedIndex.ShouldBe(2);
    }

    [Fact]
    public void Slash_Should_Enter_Filter_Mode()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");

        var (updated, _) = list.Update(MakeKey(Key.Char, '/'));

        updated.FilterState.ShouldBe(ListFilterState.Filtering);
    }

    [Fact]
    public void Filter_Escape_Should_Cancel_Filter()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");
        var (filtering, _) = list.Update(MakeKey(Key.Char, '/'));
        filtering.FilterState.ShouldBe(ListFilterState.Filtering);

        var (cancelled, _) = filtering.Update(MakeKey(Key.Escape));

        cancelled.FilterState.ShouldBe(ListFilterState.Unfiltered);
        cancelled.Items.Count.ShouldBe(3);
    }

    [Fact]
    public void Filter_Enter_With_Empty_Should_Return_To_Unfiltered()
    {
        var list = CreateList("Alpha", "Beta", "Gamma");
        var (filtering, _) = list.Update(MakeKey(Key.Char, '/'));

        var (applied, _) = filtering.Update(MakeKey(Key.Enter));

        applied.FilterState.ShouldBe(ListFilterState.Unfiltered);
        applied.Items.Count.ShouldBe(3);
    }

    [Fact]
    public void SetSize_Should_Update_Dimensions()
    {
        var list = CreateList("Alpha");

        var resized = list.SetSize(100, 50);

        resized.MinWidth.ShouldBe(100);
        resized.MinHeight.ShouldBe(50);
    }

    [Fact]
    public void SetSize_Should_Update_Help_Width()
    {
        var list = CreateList("Alpha");

        var resized = list.SetSize(100, 50);

        resized.Help.MinWidth.ShouldBe(100);
    }

    [Fact]
    public void Init_Should_Return_Command()
    {
        var list = CreateList("Alpha");

        var cmd = list.Init();

        // Spinner init returns a tick command
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Quit_With_Q_Should_Return_Quit_Command()
    {
        var list = CreateList("Alpha", "Beta");

        var (_, cmd) = list.Update(MakeKey(Key.Char, 'q'));

        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Quit_With_CtrlC_Should_Return_Quit_Command()
    {
        var list = CreateList("Alpha", "Beta");

        var (_, cmd) = list.Update(MakeKey(Key.CtrlC));

        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void NewStatusMessage_Should_Set_Message()
    {
        var list = CreateList("Alpha");

        var (updated, cmd) = list.NewStatusMessage("Hello!");

        updated.StatusMessage.ShouldBe("Hello!");
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void SetShowSpinner_True_Should_Enable_Spinner()
    {
        var list = CreateList("Alpha");

        var (updated, cmd) = list.SetShowSpinner(true);

        updated.ShowSpinner.ShouldBeTrue();
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void SetShowSpinner_False_Should_Disable_Spinner()
    {
        var list = CreateList("Alpha");
        var (spinning, _) = list.SetShowSpinner(true);

        var (updated, cmd) = spinning.SetShowSpinner(false);

        updated.ShowSpinner.ShouldBeFalse();
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Empty_List_Cursor_Down_Should_Not_Throw()
    {
        var list = CreateList();

        var (updated, _) = list.Update(MakeKey(Key.Down));

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void Empty_List_Cursor_Up_Should_Not_Throw()
    {
        var list = CreateList();

        var (updated, _) = list.Update(MakeKey(Key.Up));

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void FilterState_Should_Be_Unfiltered_Initially()
    {
        var list = CreateList("Alpha");

        list.FilterState.ShouldBe(ListFilterState.Unfiltered);
    }

    [Fact]
    public void FilteringEnabled_False_Should_Prevent_Filter_Mode()
    {
        var list = CreateList("Alpha", "Beta") with { FilteringEnabled = false };

        var (updated, _) = list.Update(MakeKey(Key.Char, '/'));

        updated.FilterState.ShouldBe(ListFilterState.Unfiltered);
    }

    [Fact]
    public void Pagination_Should_Be_Set_On_SetItems()
    {
        var items = Enumerable.Range(1, 25).Select(i => new TestItem($"Item {i}")).ToList();
        var list = new ListModel<TestItem>
        {
            Delegate = new TestDelegate(),
        };
        list = list with { Paginator = list.Paginator with { PerPage = 10 } };
        list = list.SetItems(items);

        list.Paginator.TotalPages.ShouldBe(3);
    }

    [Fact]
    public void StatusMessage_Should_Auto_Dismiss()
    {
        var list = CreateList("Alpha");
        var (withStatus, _) = list.NewStatusMessage("Temporary");
        withStatus.StatusMessage.ShouldBe("Temporary");

        // Simulate the status message timeout
        var (dismissed, _) = withStatus.Update(new ListStatusMessage { Id = withStatus.StatusId });

        dismissed.StatusMessage.ShouldBe(string.Empty);
    }

    [Fact]
    public void StatusMessage_Stale_Should_Not_Dismiss()
    {
        var list = CreateList("Alpha");
        var (withStatus, _) = list.NewStatusMessage("Temporary");

        // Send a stale status message with wrong Id
        var (notDismissed, _) = withStatus.Update(new ListStatusMessage { Id = -999 });

        notDismissed.StatusMessage.ShouldBe("Temporary");
    }
}
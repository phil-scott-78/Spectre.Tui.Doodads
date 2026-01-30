using Shouldly;
using Spectre.Tui.Doodads.Doodads.Table;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class TableModelTests
{
    private static TableModel CreateTable(int rowCount = 10)
    {
        var columns = new[] { new TableColumn("Name", 10), new TableColumn("Value", 10) };
        var rows = Enumerable.Range(0, rowCount)
            .Select(i => new TableRow([$"Item{i}", $"Val{i}"]))
            .ToList();
        return new TableModel { MinWidth = 25, MinHeight = 6, Focused = true }
            .SetColumns(columns)
            .SetRows(rows);
    }

    [Fact]
    public void Default_State_Should_Have_Empty_Columns_And_Rows()
    {
        var table = new TableModel();

        table.Columns.ShouldBeEmpty();
        table.Rows.ShouldBeEmpty();
        table.SelectedIndex.ShouldBe(0);
        table.SelectedRow.ShouldBeNull();
    }

    [Fact]
    public void SetColumns_Should_Set_Column_Definitions()
    {
        var columns = new[] { new TableColumn("A", 5), new TableColumn("B", 8) };
        var table = new TableModel().SetColumns(columns);

        table.Columns.Count.ShouldBe(2);
        table.Columns[0].Title.ShouldBe("A");
        table.Columns[0].Width.ShouldBe(5);
        table.Columns[1].Title.ShouldBe("B");
        table.Columns[1].Width.ShouldBe(8);
    }

    [Fact]
    public void SetRows_Should_Set_Rows_And_Reset_Selection()
    {
        var table = CreateTable() with { SelectedIndex = 5 };

        var rows = new[] { new TableRow(["X", "Y"]) };
        var updated = table.SetRows(rows);

        updated.Rows.Count.ShouldBe(1);
        updated.SelectedIndex.ShouldBe(0);
        updated.ScrollOffset.ShouldBe(0);
    }

    [Fact]
    public void SelectedRow_Should_Return_Current_Row()
    {
        var table = CreateTable();

        table.SelectedRow.ShouldNotBeNull();
        table.SelectedRow!.Cells[0].ShouldBe("Item0");
    }

    [Fact]
    public void SelectedRow_Should_Return_Null_When_Empty()
    {
        var table = new TableModel();

        table.SelectedRow.ShouldBeNull();
    }

    [Fact]
    public void Init_Should_Return_Null()
    {
        var table = new TableModel();

        table.Init().ShouldBeNull();
    }

    [Fact]
    public void MoveDown_Should_Advance_Selection()
    {
        var table = CreateTable();
        var msg = new KeyMessage { Key = Key.Down };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(1);
    }

    [Fact]
    public void MoveUp_Should_Decrement_Selection()
    {
        var table = CreateTable() with { SelectedIndex = 3 };
        var msg = new KeyMessage { Key = Key.Up };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(2);
    }

    [Fact]
    public void MoveUp_At_Top_Should_Stay_At_Zero()
    {
        var table = CreateTable();
        var msg = new KeyMessage { Key = Key.Up };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void MoveDown_At_Bottom_Should_Stay_At_Last()
    {
        var table = CreateTable() with { SelectedIndex = 9 };
        var msg = new KeyMessage { Key = Key.Down };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(9);
    }

    [Fact]
    public void PageDown_Should_Move_By_Body_Height()
    {
        // Height = 6, body height = 5 (header takes 1 row)
        var table = CreateTable();
        var msg = new KeyMessage { Key = Key.PageDown };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(5);
    }

    [Fact]
    public void PageUp_Should_Move_By_Body_Height()
    {
        var table = CreateTable() with { SelectedIndex = 7 };
        var msg = new KeyMessage { Key = Key.PageUp };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(2);
    }

    [Fact]
    public void HalfPageDown_Should_Move_By_Half_Body_Height()
    {
        // Body height = 5, half = 2
        var table = CreateTable();
        var msg = new KeyMessage { Key = Key.CtrlD };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(2);
    }

    [Fact]
    public void HalfPageUp_Should_Move_By_Half_Body_Height()
    {
        var table = CreateTable() with { SelectedIndex = 6 };
        var msg = new KeyMessage { Key = Key.CtrlU };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(4);
    }

    [Fact]
    public void GoToTop_Should_Select_First_Row()
    {
        var table = CreateTable() with { SelectedIndex = 7 };
        var msg = new KeyMessage { Key = Key.Home };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(0);
        updated.ScrollOffset.ShouldBe(0);
    }

    [Fact]
    public void GoToBottom_Should_Select_Last_Row()
    {
        var table = CreateTable();
        var msg = new KeyMessage { Key = Key.End };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(9);
    }

    [Fact]
    public void Selection_Should_Clamp_To_Valid_Range()
    {
        // PageDown from index 8 with body height 5 would go to 13, clamp to 9
        var table = CreateTable() with { SelectedIndex = 8 };
        var msg = new KeyMessage { Key = Key.PageDown };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(9);
    }

    [Fact]
    public void ScrollOffset_Should_Adjust_When_Selection_Below_Viewport()
    {
        // Height = 6, body = 5, starting at row 0
        // Move down 6 times to get to index 6 which is out of view
        var table = CreateTable();
        for (var i = 0; i < 6; i++)
        {
            var (next, _) = table.Update(new KeyMessage { Key = Key.Down });
            table = next;
        }

        // Selected index 6, body height 5 => scroll offset should be at least 2
        table.SelectedIndex.ShouldBe(6);
        table.ScrollOffset.ShouldBeGreaterThan(0);
        // Selected row should be visible: ScrollOffset <= SelectedIndex < ScrollOffset + BodyHeight
        table.ScrollOffset.ShouldBeLessThanOrEqualTo(table.SelectedIndex);
        (table.ScrollOffset + 5).ShouldBeGreaterThan(table.SelectedIndex);
    }

    [Fact]
    public void ScrollOffset_Should_Adjust_When_Selection_Above_Viewport()
    {
        // Start at bottom, then move to top
        var table = CreateTable() with { SelectedIndex = 9, ScrollOffset = 5 };
        var msg = new KeyMessage { Key = Key.Home };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(0);
        updated.ScrollOffset.ShouldBe(0);
    }

    [Fact]
    public void Focus_Should_Set_Focused_True()
    {
        var table = new TableModel();

        var (focused, cmd) = table.Focus();

        focused.Focused.ShouldBeTrue();
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Blur_Should_Set_Focused_False()
    {
        var table = new TableModel { Focused = true };

        var (blurred, cmd) = table.Blur();

        blurred.Focused.ShouldBeFalse();
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Unfocused_Should_Ignore_Key_Input()
    {
        var table = CreateTable() with { Focused = false };
        var msg = new KeyMessage { Key = Key.Down };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void Unfocused_Should_Handle_FocusMessage()
    {
        var table = CreateTable() with { Focused = false };

        var (updated, _) = table.Update(new FocusMessage());

        updated.Focused.ShouldBeTrue();
    }

    [Fact]
    public void Focused_Should_Handle_BlurMessage()
    {
        var table = CreateTable();

        var (updated, _) = table.Update(new BlurMessage());

        updated.Focused.ShouldBeFalse();
    }

    [Fact]
    public void WindowSizeMessage_Should_Update_Dimensions()
    {
        var table = CreateTable();
        var msg = new WindowSizeMessage { Width = 100, Height = 50 };

        var (updated, _) = table.Update(msg);

        updated.Width.ShouldBe(100);
        updated.Height.ShouldBe(50);
    }

    [Fact]
    public void Empty_Table_Navigation_Should_Not_Throw()
    {
        var table = new TableModel { Focused = true };

        var (down, _) = table.Update(new KeyMessage { Key = Key.Down });
        down.SelectedIndex.ShouldBe(0);

        var (up, _) = table.Update(new KeyMessage { Key = Key.Up });
        up.SelectedIndex.ShouldBe(0);

        var (home, _) = table.Update(new KeyMessage { Key = Key.Home });
        home.SelectedIndex.ShouldBe(0);

        var (end, _) = table.Update(new KeyMessage { Key = Key.End });
        end.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void GoToBottom_Should_Adjust_ScrollOffset()
    {
        // 10 rows, body height = 5
        var table = CreateTable();
        var msg = new KeyMessage { Key = Key.End };

        var (updated, _) = table.Update(msg);

        // Selected = 9, body = 5 => offset should be 5 (rows 5-9 visible)
        updated.SelectedIndex.ShouldBe(9);
        updated.ScrollOffset.ShouldBe(5);
    }

    [Fact]
    public void PageDown_Clamps_At_End()
    {
        // Start at row 7, page down by body height 5 => would be 12, clamp to 9
        var table = CreateTable() with { SelectedIndex = 7, ScrollOffset = 3 };
        var msg = new KeyMessage { Key = Key.PageDown };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(9);
    }

    [Fact]
    public void PageUp_Clamps_At_Start()
    {
        var table = CreateTable() with { SelectedIndex = 2 };
        var msg = new KeyMessage { Key = Key.PageUp };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void Single_Row_Table_Should_Work()
    {
        var columns = new[] { new TableColumn("Col", 10) };
        var rows = new[] { new TableRow(["Only"]) };
        var table = new TableModel { MinWidth = 15, MinHeight = 5, Focused = true }
            .SetColumns(columns)
            .SetRows(rows);

        table.SelectedIndex.ShouldBe(0);
        table.SelectedRow!.Cells[0].ShouldBe("Only");

        var (down, _) = table.Update(new KeyMessage { Key = Key.Down });
        down.SelectedIndex.ShouldBe(0);

        var (up, _) = table.Update(new KeyMessage { Key = Key.Up });
        up.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void CtrlB_Should_PageUp()
    {
        var table = CreateTable() with { SelectedIndex = 7 };
        var msg = new KeyMessage { Key = Key.CtrlB };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(2);
    }

    [Fact]
    public void CtrlF_Should_PageDown()
    {
        var table = CreateTable();
        var msg = new KeyMessage { Key = Key.CtrlF };

        var (updated, _) = table.Update(msg);

        updated.SelectedIndex.ShouldBe(5);
    }
}
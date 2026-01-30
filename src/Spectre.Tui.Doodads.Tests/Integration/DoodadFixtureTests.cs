using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Doodads.Stopwatch;
using Spectre.Tui.Doodads.Doodads.Table;
using Spectre.Tui.Doodads.Doodads.TextInput;
using Spectre.Tui.Doodads.Doodads.Viewport;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Integration;

public sealed class DoodadFixtureTests
{
    [Fact]
    public void Should_Create_Fixture_With_Model()
    {
        // Given
        var model = new TextInputModel { MinWidth = 40 };

        // When
        var fixture = new DoodadFixture<TextInputModel>(model);

        // Then
        fixture.Model.ShouldBe(model);
        fixture.Size.ShouldBe(new Size(80, 24));
    }

    [Fact]
    public void Should_Create_Fixture_With_Custom_Size()
    {
        // Given
        var model = new TextInputModel { MinWidth = 40 };
        var size = new Size(40, 10);

        // When
        var fixture = new DoodadFixture<TextInputModel>(model, size);

        // Then
        fixture.Size.ShouldBe(size);
    }

    [Fact]
    public void Should_Send_Key_And_Update_Model()
    {
        // Given
        var model = new TextInputModel { MinWidth = 40, Focused = true };
        var fixture = new DoodadFixture<TextInputModel>(model);

        // When
        fixture.SendKey(Key.Char);

        // Then - no runes means no character inserted, but model should have been updated
        fixture.Model.GetValue().ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_Send_Char_And_Update_Model()
    {
        // Given
        var model = new TextInputModel { MinWidth = 40, Focused = true };
        var fixture = new DoodadFixture<TextInputModel>(model);

        // When
        fixture.SendChar('A');

        // Then
        fixture.Model.GetValue().ShouldBe("A");
    }

    [Fact]
    public void TextInput_Full_Lifecycle()
    {
        // Given
        var model = new TextInputModel { MinWidth = 40, Focused = true };
        var fixture = new DoodadFixture<TextInputModel>(model);

        // When - type "Hi"
        fixture.SendChar('H');
        fixture.SendChar('i');

        // Then
        fixture.Model.GetValue().ShouldBe("Hi");

        // When - backspace
        fixture.SendKey(Key.Backspace);

        // Then
        fixture.Model.GetValue().ShouldBe("H");
    }

    [Fact]
    public void TextInput_Multiple_Operations()
    {
        // Given
        var model = new TextInputModel { MinWidth = 40, Focused = true };
        var fixture = new DoodadFixture<TextInputModel>(model);

        // When - type "Hello", go home, delete forward, go end, add "!"
        fixture
            .SendChar('H')
            .SendChar('e')
            .SendChar('l')
            .SendChar('l')
            .SendChar('o')
            .SendKey(Key.Home)
            .SendKey(Key.Delete)
            .SendKey(Key.End)
            .SendChar('!');

        // Then
        fixture.Model.GetValue().ShouldBe("ello!");
    }

    [Fact]
    public void Stopwatch_Start_Should_Set_Running()
    {
        // Given
        var model = new StopwatchModel();
        var (started, _) = model.Start();

        // When
        var fixture = new DoodadFixture<StopwatchModel>(started);

        // Then
        fixture.Model.Running.ShouldBeTrue();
    }

    [Fact]
    public void Stopwatch_Start_Stop_Lifecycle()
    {
        // Given
        var model = new StopwatchModel();

        // When - start
        var (started, _) = model.Start();
        var fixture = new DoodadFixture<StopwatchModel>(started);
        fixture.Model.Running.ShouldBeTrue();

        // When - stop
        var (stopped, _) = fixture.Model.Stop();
        fixture = new DoodadFixture<StopwatchModel>(stopped);
        fixture.Model.Running.ShouldBeFalse();

        // When - reset
        var (reset, _) = fixture.Model.Reset();
        reset.Elapsed.ShouldBe(TimeSpan.Zero);
        reset.Running.ShouldBeFalse();
    }

    [Fact]
    public void Viewport_Navigation_Lifecycle()
    {
        // Given - 10 lines of content, viewport height 5
        var content = string.Join("\n", Enumerable.Range(0, 10).Select(i => $"Line {i}"));
        var model = new ViewportModel { MinWidth = 40, MinHeight = 5 }.SetContent(content);
        var fixture = new DoodadFixture<ViewportModel>(model);

        // When - scroll down
        fixture.SendKey(Key.Down);

        // Then
        fixture.Model.YOffset.ShouldBe(1);

        // When - scroll to top via Home
        fixture.SendKey(Key.Home);

        // Then
        fixture.Model.YOffset.ShouldBe(0);
    }

    [Fact]
    public void Viewport_PageDown_Navigation()
    {
        // Given - 20 lines of content, viewport height 5
        var content = string.Join("\n", Enumerable.Range(0, 20).Select(i => $"Line {i}"));
        var model = new ViewportModel { MinWidth = 40, MinHeight = 5 }.SetContent(content);
        var fixture = new DoodadFixture<ViewportModel>(model);

        // When - page down
        fixture.SendKey(Key.PageDown);

        // Then - should scroll by viewport height
        fixture.Model.YOffset.ShouldBe(5);

        // When - go to end
        fixture.SendKey(Key.End);

        // Then
        fixture.Model.AtBottom.ShouldBeTrue();
    }

    [Fact]
    public void Table_Navigation_Lifecycle()
    {
        // Given
        var columns = new[] { new TableColumn("Name", 10), new TableColumn("Value", 10) };
        var rows = Enumerable.Range(0, 10)
            .Select(i => new TableRow([$"Item{i}", $"Val{i}"]))
            .ToList();
        var model = new TableModel { MinWidth = 25, MinHeight = 6, Focused = true }
            .SetColumns(columns)
            .SetRows(rows);
        var fixture = new DoodadFixture<TableModel>(model);

        // When - move down twice
        fixture.SendKey(Key.Down);
        fixture.SendKey(Key.Down);

        // Then
        fixture.Model.SelectedIndex.ShouldBe(2);

        // When - go to top
        fixture.SendKey(Key.Home);

        // Then
        fixture.Model.SelectedIndex.ShouldBe(0);
    }

    [Fact]
    public void Table_GoToBottom_And_Back()
    {
        // Given
        var columns = new[] { new TableColumn("Col", 10) };
        var rows = Enumerable.Range(0, 5)
            .Select(i => new TableRow([$"Row{i}"]))
            .ToList();
        var model = new TableModel { MinWidth = 15, MinHeight = 4, Focused = true }
            .SetColumns(columns)
            .SetRows(rows);
        var fixture = new DoodadFixture<TableModel>(model);

        // When - go to bottom
        fixture.SendKey(Key.End);

        // Then
        fixture.Model.SelectedIndex.ShouldBe(4);

        // When - go to top
        fixture.SendKey(Key.Home);

        // Then
        fixture.Model.SelectedIndex.ShouldBe(0);
        fixture.Model.ScrollOffset.ShouldBe(0);
    }

    [Fact]
    public void Composition_Should_Forward_Messages()
    {
        // A TextInputModel is a valid IDoodad that composes CursorModel internally.
        // Verify that sending characters through the fixture works end-to-end,
        // including internal cursor state updates.
        var model = new TextInputModel { MinWidth = 40, Focused = true };
        var fixture = new DoodadFixture<TextInputModel>(model);

        // Type characters
        fixture
            .SendChar('T')
            .SendChar('e')
            .SendChar('s')
            .SendChar('t');

        // The TextInputModel should have the value and the internal cursor
        // should be at position 4 (end of "Test")
        fixture.Model.GetValue().ShouldBe("Test");
        fixture.Model.Position.ShouldBe(4);
    }

    [Fact]
    public void SendKeys_Should_Send_Multiple_Keys()
    {
        // Given - viewport with content
        var content = string.Join("\n", Enumerable.Range(0, 20).Select(i => $"Line {i}"));
        var model = new ViewportModel { MinWidth = 40, MinHeight = 5 }.SetContent(content);
        var fixture = new DoodadFixture<ViewportModel>(model);

        // When - send multiple down keys
        fixture.SendKeys(Key.Down, Key.Down, Key.Down);

        // Then
        fixture.Model.YOffset.ShouldBe(3);
    }

    [Fact]
    public void Init_Should_Process_Startup_Command()
    {
        // Given - a stopwatch that starts running
        var model = new StopwatchModel { Running = true };
        var fixture = new DoodadFixture<StopwatchModel>(model);

        // When - Init processes the tick command which uses Task.Delay and will throw
        // OperationCanceledException. The fixture should handle this gracefully.
        fixture.Init();

        // Then - model should still be accessible
        fixture.Model.Running.ShouldBeTrue();
    }

    [Fact]
    public void Render_Should_Produce_Output()
    {
        // Given - a viewport with some content
        var model = new ViewportModel { MinWidth = 10, MinHeight = 3 }
            .SetContent("Hello\nWorld\nTest");
        var fixture = new DoodadFixture<ViewportModel>(model, new Size(10, 3));

        // When
        var result = fixture.Render();

        // Then - should have some non-empty output
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Send_Should_Return_Fixture_For_Chaining()
    {
        // Given
        var model = new TextInputModel { MinWidth = 40, Focused = true };
        var fixture = new DoodadFixture<TextInputModel>(model);

        // When
        var returned = fixture.Send(new KeyMessage { Key = Key.Char, Runes = [new Rune('X')] });

        // Then
        returned.ShouldBeSameAs(fixture);
        fixture.Model.GetValue().ShouldBe("X");
    }
}
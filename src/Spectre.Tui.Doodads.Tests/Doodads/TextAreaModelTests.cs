using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Doodads.TextArea;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class TextAreaModelTests
{
    private static TextAreaModel CreateFocused(string text = "")
    {
        var model = new TextAreaModel { MinWidth = 40, MinHeight = 6 };
        if (!string.IsNullOrEmpty(text))
        {
            model = model.SetValue(text);
        }

        return model.Focus().Model;
    }

    [Fact]
    public void SetValue_Should_Set_Content()
    {
        // Given
        var model = new TextAreaModel();

        // When
        var updated = model.SetValue("Hello\nWorld");

        // Then
        updated.GetValue().ShouldBe("Hello\nWorld");
    }

    [Fact]
    public void GetValue_Should_Return_Empty_For_Default()
    {
        // Given
        var model = new TextAreaModel();

        // When / Then
        model.GetValue().ShouldBe(string.Empty);
    }

    [Fact]
    public void InsertString_Should_Add_Text()
    {
        // Given
        var model = CreateFocused();

        // When
        var updated = model.InsertString("Hello");

        // Then
        updated.GetValue().ShouldBe("Hello");
    }

    [Fact]
    public void InsertString_With_Newline_Should_Create_Lines()
    {
        // Given
        var model = CreateFocused();

        // When
        var updated = model.InsertString("Hello\nWorld");

        // Then
        updated.GetValue().ShouldBe("Hello\nWorld");
    }

    [Fact]
    public void Update_With_Character_Key_Should_Insert()
    {
        // Given
        var model = CreateFocused();
        var msg = new KeyMessage { Key = Key.Char, Runes = [new Rune('A')] };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.GetValue().ShouldBe("A");
    }

    [Fact]
    public void Update_With_Enter_Should_Insert_Newline()
    {
        // Given
        var model = CreateFocused().InsertString("AB");
        // Move cursor between A and B
        model = model with { Col = 1 };
        var msg = new KeyMessage { Key = Key.Enter };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.GetValue().ShouldBe("A\nB");
    }

    [Fact]
    public void Update_With_Backspace_Should_Delete_Backward()
    {
        // Given
        var model = CreateFocused().InsertString("AB");
        var msg = new KeyMessage { Key = Key.Backspace };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.GetValue().ShouldBe("A");
    }

    [Fact]
    public void Update_With_Delete_Should_Delete_Forward()
    {
        // Given
        var model = CreateFocused().InsertString("AB");
        model = model with { Col = 0 };
        var msg = new KeyMessage { Key = Key.Delete };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.GetValue().ShouldBe("B");
    }

    [Fact]
    public void Update_With_Home_Should_Move_To_Line_Start()
    {
        // Given
        var model = CreateFocused().InsertString("Hello");
        var msg = new KeyMessage { Key = Key.Home };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.Col.ShouldBe(0);
    }

    [Fact]
    public void Update_With_End_Should_Move_To_Line_End()
    {
        // Given
        var model = CreateFocused().InsertString("Hello");
        model = model with { Col = 0 };
        var msg = new KeyMessage { Key = Key.End };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.Col.ShouldBe(5);
    }

    [Fact]
    public void Focus_Should_Set_Focused()
    {
        // Given
        var model = new TextAreaModel();

        // When
        var (updated, _) = model.Focus();

        // Then
        updated.Focused.ShouldBeTrue();
    }

    [Fact]
    public void Blur_Should_Set_Not_Focused()
    {
        // Given
        var model = CreateFocused();

        // When
        var (updated, _) = model.Blur();

        // Then
        updated.Focused.ShouldBeFalse();
    }

    [Fact]
    public void Backspace_At_Start_Of_Line_Should_Merge_With_Previous()
    {
        // Given
        var model = CreateFocused().InsertString("Hello\nWorld");
        model = model with { Row = 1, Col = 0 };
        var msg = new KeyMessage { Key = Key.Backspace };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.GetValue().ShouldBe("HelloWorld");
        updated.Row.ShouldBe(0);
        updated.Col.ShouldBe(5);
    }

    [Fact]
    public void Delete_At_End_Of_Line_Should_Merge_With_Next()
    {
        // Given
        var model = CreateFocused().InsertString("Hello\nWorld");
        model = model with { Row = 0, Col = 5 };
        var msg = new KeyMessage { Key = Key.Delete };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.GetValue().ShouldBe("HelloWorld");
    }

    [Fact]
    public void Arrow_Up_Should_Move_Up()
    {
        // Given
        var model = CreateFocused().InsertString("Line1\nLine2");
        model = model with { Row = 1, Col = 2 };
        var msg = new KeyMessage { Key = Key.Up };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.Row.ShouldBe(0);
        updated.Col.ShouldBe(2);
    }

    [Fact]
    public void Arrow_Down_Should_Move_Down()
    {
        // Given
        var model = CreateFocused().InsertString("Line1\nLine2");
        model = model with { Row = 0, Col = 2 };
        var msg = new KeyMessage { Key = Key.Down };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.Row.ShouldBe(1);
        updated.Col.ShouldBe(2);
    }

    [Fact]
    public void CharLimit_Should_Prevent_Insertion()
    {
        // Given
        var model = CreateFocused() with { CharLimit = 3 };
        model = model.InsertString("ABC");
        var msg = new KeyMessage { Key = Key.Char, Runes = [new Rune('D')] };

        // When
        var (updated, _) = model.Update(msg);

        // Then
        updated.GetValue().ShouldBe("ABC");
    }
}
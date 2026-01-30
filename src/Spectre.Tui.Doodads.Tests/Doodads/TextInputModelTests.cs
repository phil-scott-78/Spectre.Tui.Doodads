using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Doodads.TextInput;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class TextInputModelTests
{
    private static TextInputModel CreateFocused()
    {
        var model = new TextInputModel { MinWidth = 40, Focused = true };
        return model;
    }

    [Fact]
    public void SetValue_Should_Set_And_Get()
    {
        var model = new TextInputModel();
        var updated = model.SetValue("Hello");

        updated.GetValue().ShouldBe("Hello");
    }

    [Fact]
    public void GetValue_Should_Return_Empty_For_Default()
    {
        var model = new TextInputModel();
        model.GetValue().ShouldBe(string.Empty);
    }

    [Fact]
    public void Update_With_Char_Should_Insert()
    {
        var model = CreateFocused();
        var msg = new KeyMessage { Key = Key.Char, Runes = [new Rune('A')] };

        var (updated, _) = model.Update(msg);

        updated.GetValue().ShouldBe("A");
    }

    [Fact]
    public void Update_With_Backspace_Should_Delete()
    {
        var model = CreateFocused().SetValue("AB");
        var msg = new KeyMessage { Key = Key.Backspace };

        var (updated, _) = model.Update(msg);

        updated.GetValue().ShouldBe("A");
    }

    [Fact]
    public void Update_With_Delete_Should_Delete_Forward()
    {
        var model = CreateFocused().SetValue("AB").SetCursorPosition(0);
        var msg = new KeyMessage { Key = Key.Delete };

        var (updated, _) = model.Update(msg);

        updated.GetValue().ShouldBe("B");
    }

    [Fact]
    public void Update_With_Home_Should_Move_To_Start()
    {
        var model = CreateFocused().SetValue("Hello");
        var msg = new KeyMessage { Key = Key.Home };

        var (updated, _) = model.Update(msg);

        updated.Position.ShouldBe(0);
    }

    [Fact]
    public void Update_With_End_Should_Move_To_End()
    {
        var model = CreateFocused().SetValue("Hello").SetCursorPosition(0);
        var msg = new KeyMessage { Key = Key.End };

        var (updated, _) = model.Update(msg);

        updated.Position.ShouldBe(5);
    }

    [Fact]
    public void Focus_Should_Set_Focused()
    {
        var model = new TextInputModel();
        var (updated, _) = model.Focus();

        updated.Focused.ShouldBeTrue();
    }

    [Fact]
    public void Blur_Should_Set_Not_Focused()
    {
        var model = CreateFocused();
        var (updated, _) = model.Blur();

        updated.Focused.ShouldBeFalse();
    }

    [Fact]
    public void CharLimit_Should_Prevent_Insertion()
    {
        var model = CreateFocused() with { CharLimit = 3 };
        model = model.SetValue("ABC");
        var msg = new KeyMessage { Key = Key.Char, Runes = [new Rune('D')] };

        var (updated, _) = model.Update(msg);

        updated.GetValue().ShouldBe("ABC");
    }

    [Fact]
    public void Validation_Should_Reject_Invalid_Input()
    {
        var model = CreateFocused() with
        {
            Validate = value => value.All(char.IsDigit),
        };
        model = model.SetValue("123");
        var msg = new KeyMessage { Key = Key.Char, Runes = [new Rune('A')] };

        var (updated, _) = model.Update(msg);

        updated.GetValue().ShouldBe("123");
    }

    [Fact]
    public void Password_EchoMode_Should_Not_Affect_GetValue()
    {
        var model = CreateFocused() with { EchoMode = EchoMode.Password };
        model = model.SetValue("secret");

        model.GetValue().ShouldBe("secret");
    }

    [Fact]
    public void Unfocused_Should_Ignore_Key_Input()
    {
        var model = new TextInputModel();
        var msg = new KeyMessage { Key = Key.Char, Runes = [new Rune('A')] };

        var (updated, _) = model.Update(msg);

        updated.GetValue().ShouldBe(string.Empty);
    }
}
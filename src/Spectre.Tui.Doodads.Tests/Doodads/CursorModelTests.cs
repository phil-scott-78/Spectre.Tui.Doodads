using Shouldly;
using Spectre.Tui.Doodads.Doodads.Cursor;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class CursorModelTests
{
    [Fact]
    public void Init_When_Blink_And_Focused_Should_Return_Command()
    {
        // Given
        var cursor = new CursorModel { Mode = CursorMode.Blink, Focused = true };

        // When
        var cmd = cursor.Init();

        // Then
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Init_When_Not_Focused_Should_Return_Null()
    {
        // Given
        var cursor = new CursorModel { Mode = CursorMode.Blink, Focused = false };

        // When
        var cmd = cursor.Init();

        // Then
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Init_When_Static_Should_Return_Null()
    {
        // Given
        var cursor = new CursorModel { Mode = CursorMode.Static, Focused = true };

        // When
        var cmd = cursor.Init();

        // Then
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Focus_Should_Set_Focused_And_Visible()
    {
        // Given
        var cursor = new CursorModel();

        // When
        var (focused, cmd) = cursor.Focus();

        // Then
        focused.Focused.ShouldBeTrue();
        focused.Visible.ShouldBeTrue();
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Blur_Should_Set_Not_Focused()
    {
        // Given
        var cursor = new CursorModel { Focused = true };

        // When
        var (blurred, cmd) = cursor.Blur();

        // Then
        blurred.Focused.ShouldBeFalse();
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Blur_Should_Increment_Tag()
    {
        // Given
        var cursor = new CursorModel { Focused = true };

        // When
        var (blurred, _) = cursor.Blur();

        // Then
        blurred.Tag.ShouldBe(cursor.Tag + 1);
    }

    [Fact]
    public void Update_FocusMessage_Should_Focus()
    {
        // Given
        var cursor = new CursorModel();

        // When
        var (updated, cmd) = cursor.Update(new FocusMessage());

        // Then
        updated.Focused.ShouldBeTrue();
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Update_BlurMessage_Should_Blur()
    {
        // Given
        var cursor = new CursorModel { Focused = true };

        // When
        var (updated, _) = cursor.Update(new BlurMessage());

        // Then
        updated.Focused.ShouldBeFalse();
    }

    [Fact]
    public void SetChar_Should_Set_Character()
    {
        // Given
        var cursor = new CursorModel();

        // When
        var updated = cursor.SetChar("X");

        // Then
        updated.Character.ShouldBe("X");
    }

    [Fact]
    public void Update_With_Unrelated_Message_Should_Return_Unchanged()
    {
        // Given
        var cursor = new CursorModel();
        var message = new QuitMessage();

        // When
        var (updated, cmd) = cursor.Update(message);

        // Then
        updated.ShouldBe(cursor);
        cmd.ShouldBeNull();
    }
}
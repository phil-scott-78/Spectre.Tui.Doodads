using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Messages;

public sealed class MessageTests
{
    [Fact]
    public void KeyMessage_Should_Have_Expected_Properties()
    {
        // Given / When
        var message = new KeyMessage
        {
            Key = Key.Up,
            Runes = [new Rune('a')],
            Alt = true,
            Shift = false,
            Ctrl = true,
        };

        // Then
        message.Key.ShouldBe(Key.Up);
        message.Runes.Length.ShouldBe(1);
        message.Alt.ShouldBeTrue();
        message.Shift.ShouldBeFalse();
        message.Ctrl.ShouldBeTrue();
    }

    [Fact]
    public void WindowSizeMessage_Should_Have_Expected_Properties()
    {
        // Given / When
        var message = new WindowSizeMessage { Width = 80, Height = 25 };

        // Then
        message.Width.ShouldBe(80);
        message.Height.ShouldBe(25);
    }

    [Fact]
    public void QuitMessage_Should_Be_A_Message()
    {
        // Given / When
        var message = new QuitMessage();

        // Then
        message.ShouldBeAssignableTo<Message>();
    }

    [Fact]
    public void TickMessage_Should_Have_Expected_Properties()
    {
        // Given / When
        var time = DateTimeOffset.UtcNow;
        var message = new TickMessage { Time = time, Id = 42, Tag = 7 };

        // Then
        message.Time.ShouldBe(time);
        message.Id.ShouldBe(42);
        message.Tag.ShouldBe(7);
    }
}
using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Messages;

public sealed class KeyMessageTests
{
    [Fact]
    public void Char_Should_Return_Character_For_Char_Key()
    {
        // Given
        var message = new KeyMessage { Key = Key.Char, Runes = [new Rune('q')] };

        // When / Then
        message.Char.ShouldBe('q');
    }

    [Fact]
    public void Char_Should_Return_Null_For_Non_Char_Key()
    {
        // Given
        var message = new KeyMessage { Key = Key.Up };

        // When / Then
        message.Char.ShouldBeNull();
    }

    [Fact]
    public void Char_Should_Return_Null_For_Empty_Runes()
    {
        // Given
        var message = new KeyMessage { Key = Key.Char, Runes = [] };

        // When / Then
        message.Char.ShouldBeNull();
    }

    [Fact]
    public void Char_Should_Return_Null_For_Non_Bmp_Rune()
    {
        // Given — emoji U+1F600 is outside BMP
        var message = new KeyMessage { Key = Key.Char, Runes = [new Rune(0x1F600)] };

        // When / Then
        message.Char.ShouldBeNull();
    }
}

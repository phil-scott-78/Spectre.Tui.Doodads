using System.Text;
using Shouldly;
using Spectre.Tui.Doodads.Doodads.RuneUtil;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class UnicodeWidthTests
{
    [Fact]
    public void GetWidth_Rune_Returns_One_For_Ascii()
    {
        // Given
        var rune = new Rune('A');

        // When
        var width = UnicodeWidth.GetWidth(rune);

        // Then
        width.ShouldBe(1);
    }

    [Fact]
    public void GetWidth_Rune_Returns_Two_For_Cjk()
    {
        // Given — U+4E16 '世' is a CJK character (double-width)
        var rune = new Rune('\u4E16');

        // When
        var width = UnicodeWidth.GetWidth(rune);

        // Then
        width.ShouldBe(2);
    }

    [Fact]
    public void GetWidth_Rune_Returns_Zero_For_Control_Char()
    {
        // Given — U+0000 is a control character
        var rune = new Rune('\0');

        // When
        var width = UnicodeWidth.GetWidth(rune);

        // Then
        width.ShouldBe(0);
    }

    [Fact]
    public void GetWidth_String_Sums_Rune_Widths()
    {
        // Given — "AB" = 1 + 1
        var text = "AB";

        // When
        var width = UnicodeWidth.GetWidth(text);

        // Then
        width.ShouldBe(2);
    }

    [Fact]
    public void GetWidth_String_Handles_Wide_Chars()
    {
        // Given — "世界" = 2 + 2
        var text = "\u4E16\u754C";

        // When
        var width = UnicodeWidth.GetWidth(text);

        // Then
        width.ShouldBe(4);
    }

    [Fact]
    public void GetWidth_String_Returns_Zero_For_Empty()
    {
        // Given / When
        var width = UnicodeWidth.GetWidth(string.Empty);

        // Then
        width.ShouldBe(0);
    }

    [Fact]
    public void GetDisplayWidth_Rune_Returns_At_Least_One()
    {
        // Given — control char has raw width 0, but display width is at least 1
        var rune = new Rune('\0');

        // When
        var width = UnicodeWidth.GetDisplayWidth(rune);

        // Then
        width.ShouldBe(1);
    }

    [Fact]
    public void GetDisplayWidth_Rune_Preserves_Wide_Width()
    {
        // Given
        var rune = new Rune('\u4E16');

        // When
        var width = UnicodeWidth.GetDisplayWidth(rune);

        // Then
        width.ShouldBe(2);
    }

    [Fact]
    public void GetDisplayWidth_String_Sums_Display_Widths()
    {
        // Given — "A世" = 1 + 2
        var text = "A\u4E16";

        // When
        var width = UnicodeWidth.GetDisplayWidth(text);

        // Then
        width.ShouldBe(3);
    }

    [Fact]
    public void TruncateToWidth_Truncates_At_Boundary()
    {
        // Given — "Hello" at max 3 should give "Hel"
        var text = "Hello";

        // When
        var result = UnicodeWidth.TruncateToWidth(text, 3);

        // Then
        result.ShouldBe("Hel");
    }

    [Fact]
    public void TruncateToWidth_Does_Not_Split_Wide_Char()
    {
        // Given — "A世B" = widths 1,2,1. Max 2 → only "A" fits (世 needs 2 more)
        var text = "A\u4E16B";

        // When
        var result = UnicodeWidth.TruncateToWidth(text, 2);

        // Then
        result.ShouldBe("A");
    }

    [Fact]
    public void TruncateToWidth_Returns_Full_String_When_Within_Limit()
    {
        // Given
        var text = "Hi";

        // When
        var result = UnicodeWidth.TruncateToWidth(text, 10);

        // Then
        result.ShouldBe("Hi");
    }

    [Fact]
    public void TruncateToWidth_Returns_Empty_For_Zero_Width()
    {
        // Given
        var text = "Hello";

        // When
        var result = UnicodeWidth.TruncateToWidth(text, 0);

        // Then
        result.ShouldBe(string.Empty);
    }
}

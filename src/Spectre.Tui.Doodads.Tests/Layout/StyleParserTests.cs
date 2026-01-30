using Shouldly;
using Spectre.Console;
using Spectre.Tui.Doodads.Layout;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class StyleParserTests
{
    [Fact]
    public void Should_Parse_Bold()
    {
        // Given / When
        var result = StyleParser.Parse("bold");

        // Then
        result.Decoration.ShouldBe(Decoration.Bold);
    }

    [Fact]
    public void Should_Parse_Dim()
    {
        // Given / When
        var result = StyleParser.Parse("dim");

        // Then
        result.Decoration.ShouldBe(Decoration.Dim);
    }

    [Fact]
    public void Should_Parse_Italic()
    {
        // Given / When
        var result = StyleParser.Parse("italic");

        // Then
        result.Decoration.ShouldBe(Decoration.Italic);
    }

    [Fact]
    public void Should_Parse_Underline()
    {
        // Given / When
        var result = StyleParser.Parse("underline");

        // Then
        result.Decoration.ShouldBe(Decoration.Underline);
    }

    [Fact]
    public void Should_Parse_Invert()
    {
        // Given / When
        var result = StyleParser.Parse("invert");

        // Then
        result.Decoration.ShouldBe(Decoration.Invert);
    }

    [Fact]
    public void Should_Parse_Strikethrough()
    {
        // Given / When
        var result = StyleParser.Parse("strikethrough");

        // Then
        result.Decoration.ShouldBe(Decoration.Strikethrough);
    }

    [Fact]
    public void Should_Combine_Multiple_Decorations()
    {
        // Given / When
        var result = StyleParser.Parse("bold underline");

        // Then
        result.Decoration.ShouldBe(Decoration.Bold | Decoration.Underline);
    }

    [Fact]
    public void Should_Parse_Named_Foreground_Color()
    {
        // Given / When
        var result = StyleParser.Parse("text-red");

        // Then
        result.Foreground.ShouldBe(Color.Red);
    }

    [Fact]
    public void Should_Parse_Named_Background_Color()
    {
        // Given / When
        var result = StyleParser.Parse("bg-blue");

        // Then
        result.Background.ShouldBe(Color.Blue);
    }

    [Fact]
    public void Should_Parse_Hex_Foreground_Color()
    {
        // Given / When
        var result = StyleParser.Parse("text-#ff8800");

        // Then
        result.Foreground.ShouldBe(new Color(255, 136, 0));
    }

    [Fact]
    public void Should_Parse_Hex_Background_Color()
    {
        // Given / When
        var result = StyleParser.Parse("bg-#001122");

        // Then
        result.Background.ShouldBe(new Color(0, 17, 34));
    }

    [Fact]
    public void Should_Parse_Full_Compound_Style()
    {
        // Given / When
        var result = StyleParser.Parse("bold text-white bg-red");

        // Then
        result.Decoration.ShouldBe(Decoration.Bold);
        result.Foreground.ShouldBe(Color.White);
        result.Background.ShouldBe(Color.Red);
    }

    [Fact]
    public void Should_Parse_Grey_And_Gray_Aliases()
    {
        // Given / When
        var grey = StyleParser.Parse("text-grey");
        var gray = StyleParser.Parse("text-gray");

        // Then
        grey.Foreground.ShouldBe(Color.Grey);
        gray.Foreground.ShouldBe(Color.Grey);
    }

    [Fact]
    public void Should_Ignore_Unknown_Tokens()
    {
        // Given / When
        var result = StyleParser.Parse("bald text-nope bold");

        // Then — only bold should be applied, unknown tokens silently ignored
        result.Decoration.ShouldBe(Decoration.Bold);
    }

    [Fact]
    public void Should_Return_Plain_For_Empty_Format()
    {
        // Given / When
        var result = StyleParser.Parse("");

        // Then
        result.ShouldBe(Appearance.Plain);
    }

    [Fact]
    public void Should_Handle_Extra_Whitespace()
    {
        // Given / When
        var result = StyleParser.Parse("  bold   text-red  ");

        // Then
        result.Decoration.ShouldBe(Decoration.Bold);
        result.Foreground.ShouldBe(Color.Red);
    }

    [Fact]
    public void Should_Parse_Black()
    {
        // Given / When
        var result = StyleParser.Parse("text-black");

        // Then
        result.Foreground.ShouldBe(Color.Black);
    }

    [Fact]
    public void Should_Parse_Yellow()
    {
        // Given / When
        var result = StyleParser.Parse("text-yellow");

        // Then
        result.Foreground.ShouldBe(Color.Yellow);
    }

    [Fact]
    public void Should_Parse_Green()
    {
        // Given / When
        var result = StyleParser.Parse("text-green");

        // Then
        result.Foreground.ShouldBe(Color.Green);
    }

    [Fact]
    public void Should_Parse_Cyan()
    {
        // Given / When
        var result = StyleParser.Parse("text-cyan");

        // Then
        result.Foreground.ShouldBe(Color.Cyan1);
    }

    [Fact]
    public void Should_Parse_Magenta()
    {
        // Given / When
        var result = StyleParser.Parse("text-magenta");

        // Then
        result.Foreground.ShouldBe(Color.Magenta1);
    }
}
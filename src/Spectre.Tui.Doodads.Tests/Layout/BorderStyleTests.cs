using Shouldly;
using Spectre.Tui.Doodads.Layout;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class BorderStyleTests
{
    [Fact]
    public void Default_Should_Match_Normal()
    {
        // Given
        var defaultStyle = new BorderStyle();

        // Then
        defaultStyle.ShouldBe(BorderStyle.Normal);
    }

    [Fact]
    public void Normal_Should_Have_Expected_Characters()
    {
        // Given
        var style = BorderStyle.Normal;

        // Then
        style.Top.ShouldBe("\u2500");
        style.Bottom.ShouldBe("\u2500");
        style.Left.ShouldBe("\u2502");
        style.Right.ShouldBe("\u2502");
        style.TopLeft.ShouldBe("\u250c");
        style.TopRight.ShouldBe("\u2510");
        style.BottomLeft.ShouldBe("\u2514");
        style.BottomRight.ShouldBe("\u2518");
    }

    [Fact]
    public void Rounded_Should_Have_Rounded_Corners()
    {
        // Given
        var style = BorderStyle.Rounded;

        // Then — edges same as normal
        style.Top.ShouldBe("\u2500");
        style.Bottom.ShouldBe("\u2500");
        style.Left.ShouldBe("\u2502");
        style.Right.ShouldBe("\u2502");
        // Corners are rounded
        style.TopLeft.ShouldBe("\u256d");
        style.TopRight.ShouldBe("\u256e");
        style.BottomLeft.ShouldBe("\u2570");
        style.BottomRight.ShouldBe("\u256f");
    }

    [Fact]
    public void Thick_Should_Have_Thick_Characters()
    {
        // Given
        var style = BorderStyle.Thick;

        // Then
        style.Top.ShouldBe("\u2501");
        style.Bottom.ShouldBe("\u2501");
        style.Left.ShouldBe("\u2503");
        style.Right.ShouldBe("\u2503");
        style.TopLeft.ShouldBe("\u250f");
        style.TopRight.ShouldBe("\u2513");
        style.BottomLeft.ShouldBe("\u2517");
        style.BottomRight.ShouldBe("\u251b");
    }

    [Fact]
    public void Double_Should_Have_Double_Line_Characters()
    {
        // Given
        var style = BorderStyle.Double;

        // Then
        style.Top.ShouldBe("\u2550");
        style.Bottom.ShouldBe("\u2550");
        style.Left.ShouldBe("\u2551");
        style.Right.ShouldBe("\u2551");
        style.TopLeft.ShouldBe("\u2554");
        style.TopRight.ShouldBe("\u2557");
        style.BottomLeft.ShouldBe("\u255a");
        style.BottomRight.ShouldBe("\u255d");
    }

    [Fact]
    public void Hidden_Should_Have_Spaces()
    {
        // Given
        var style = BorderStyle.Hidden;

        // Then
        style.Top.ShouldBe(" ");
        style.Bottom.ShouldBe(" ");
        style.Left.ShouldBe(" ");
        style.Right.ShouldBe(" ");
        style.TopLeft.ShouldBe(" ");
        style.TopRight.ShouldBe(" ");
        style.BottomLeft.ShouldBe(" ");
        style.BottomRight.ShouldBe(" ");
    }

    [Fact]
    public void Ascii_Should_Have_Ascii_Characters()
    {
        // Given
        var style = BorderStyle.Ascii;

        // Then
        style.Top.ShouldBe("-");
        style.Bottom.ShouldBe("-");
        style.Left.ShouldBe("|");
        style.Right.ShouldBe("|");
        style.TopLeft.ShouldBe("+");
        style.TopRight.ShouldBe("+");
        style.BottomLeft.ShouldBe("+");
        style.BottomRight.ShouldBe("+");
    }

    [Fact]
    public void Block_Should_Have_Full_Block_Characters()
    {
        // Given
        var style = BorderStyle.Block;

        // Then — all positions use full block █
        style.Top.ShouldBe("\u2588");
        style.Bottom.ShouldBe("\u2588");
        style.Left.ShouldBe("\u2588");
        style.Right.ShouldBe("\u2588");
        style.TopLeft.ShouldBe("\u2588");
        style.TopRight.ShouldBe("\u2588");
        style.BottomLeft.ShouldBe("\u2588");
        style.BottomRight.ShouldBe("\u2588");
    }

    [Fact]
    public void OuterHalfBlock_Should_Have_Expected_Characters()
    {
        // Given
        var style = BorderStyle.OuterHalfBlock;

        // Then
        style.Top.ShouldBe("\u2580");
        style.Bottom.ShouldBe("\u2584");
        style.Left.ShouldBe("\u258c");
        style.Right.ShouldBe("\u2590");
        style.TopLeft.ShouldBe("\u259b");
        style.TopRight.ShouldBe("\u259c");
        style.BottomLeft.ShouldBe("\u2599");
        style.BottomRight.ShouldBe("\u259f");
    }

    [Fact]
    public void InnerHalfBlock_Should_Have_Expected_Characters()
    {
        // Given
        var style = BorderStyle.InnerHalfBlock;

        // Then
        style.Top.ShouldBe("\u2584");
        style.Bottom.ShouldBe("\u2580");
        style.Left.ShouldBe("\u2590");
        style.Right.ShouldBe("\u258c");
        style.TopLeft.ShouldBe("\u2597");
        style.TopRight.ShouldBe("\u2596");
        style.BottomLeft.ShouldBe("\u259d");
        style.BottomRight.ShouldBe("\u2598");
    }

    [Fact]
    public void Custom_Style_Via_With_Expression()
    {
        // Given
        var custom = BorderStyle.Normal with { TopLeft = "*", TopRight = "*" };

        // Then
        custom.TopLeft.ShouldBe("*");
        custom.TopRight.ShouldBe("*");
        // Other properties unchanged
        custom.Top.ShouldBe("\u2500");
        custom.BottomLeft.ShouldBe("\u2514");
    }

    [Fact]
    public void All_Predefined_Styles_Should_Have_Non_Null_Characters()
    {
        // Given
        BorderStyle[] styles =
        [
            BorderStyle.Normal,
            BorderStyle.Rounded,
            BorderStyle.Thick,
            BorderStyle.Double,
            BorderStyle.Hidden,
            BorderStyle.Ascii,
            BorderStyle.Block,
            BorderStyle.OuterHalfBlock,
            BorderStyle.InnerHalfBlock,
        ];

        // Then
        foreach (var style in styles)
        {
            style.Top.ShouldNotBeNull();
            style.Bottom.ShouldNotBeNull();
            style.Left.ShouldNotBeNull();
            style.Right.ShouldNotBeNull();
            style.TopLeft.ShouldNotBeNull();
            style.TopRight.ShouldNotBeNull();
            style.BottomLeft.ShouldNotBeNull();
            style.BottomRight.ShouldNotBeNull();
        }
    }
}

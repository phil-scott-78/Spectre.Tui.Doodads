using Shouldly;
using Spectre.Tui.Doodads.Rendering;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Rendering;

public sealed class FallbackCharacterStageTests
{
    private static string RenderThrough(string text)
    {
        var fixture = new TuiFixture(new Size(text.Length, 1));
        return fixture.Render(surface =>
        {
            var stage = new FallbackCharacterStage(surface);
            stage.SetString(0, 0, text, Appearance.Plain);
        });
    }

    [Fact]
    public void Normal_Style_Corners_Should_Map_To_Plus()
    {
        // Given — Normal style corners: ┌ ┐ └ ┘
        var text = "\u250c\u2510\u2514\u2518";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("++++");
    }

    [Fact]
    public void Normal_Style_Horizontal_Should_Map_To_Dash()
    {
        // Given — Normal horizontal: ─
        var text = "\u2500\u2500\u2500";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("---");
    }

    [Fact]
    public void Normal_Style_Vertical_Should_Map_To_Pipe()
    {
        // Given — Normal vertical: │
        var text = "\u2502\u2502";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("||");
    }

    [Fact]
    public void Rounded_Corners_Should_Map_To_Plus()
    {
        // Given — Rounded corners: ╭ ╮ ╰ ╯
        var text = "\u256d\u256e\u2570\u256f";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("++++");
    }

    [Fact]
    public void Thick_Style_Should_Map_Correctly()
    {
        // Given — Thick: ━ ┃ ┏ ┓ ┗ ┛
        var text = "\u2501\u2503\u250f\u2513\u2517\u251b";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("-|++++");
    }

    [Fact]
    public void Double_Style_Should_Map_Correctly()
    {
        // Given — Double: ═ ║ ╔ ╗ ╚ ╝
        var text = "\u2550\u2551\u2554\u2557\u255a\u255d";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("-|++++");
    }

    [Fact]
    public void Block_Style_Should_Map_To_Ascii()
    {
        // Given — Block: █ (used for all positions)
        var text = "\u2588";

        // When
        var result = RenderThrough(text);

        // Then — █ should map to some ASCII character (first mapping wins: - from Top/Bottom)
        result.Length.ShouldBe(1);
        result[0].ShouldBeLessThanOrEqualTo('\u007F');
    }

    [Fact]
    public void OuterHalfBlock_Should_Map_To_Ascii()
    {
        // Given — OuterHalfBlock: ▀ ▄ ▌ ▐ ▛ ▜ ▙ ▟
        var text = "\u2580\u2584\u258c\u2590\u259b\u259c\u2599\u259f";

        // When
        var result = RenderThrough(text);

        // Then — all characters should be ASCII
        foreach (var c in result)
        {
            c.ShouldBeLessThanOrEqualTo('\u007F');
        }
    }

    [Fact]
    public void InnerHalfBlock_Should_Map_To_Ascii()
    {
        // Given — InnerHalfBlock: ▄ ▀ ▐ ▌ ▗ ▖ ▝ ▘
        var text = "\u2597\u2596\u259d\u2598";

        // When
        var result = RenderThrough(text);

        // Then — all corner characters should map to +
        result.ShouldBe("++++");
    }

    [Fact]
    public void T_Junctions_Should_Map_To_Plus()
    {
        // Given — T-junctions and cross: ┬ ┴ ├ ┤ ┼
        var text = "\u252c\u2534\u251c\u2524\u253c";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("+++++");
    }

    [Fact]
    public void Ascii_Text_Should_Pass_Through_Unchanged()
    {
        // Given
        var text = "Hello!";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("Hello!");
    }

    [Fact]
    public void Mixed_Content_Should_Map_Only_Box_Drawing()
    {
        // Given — mix of box-drawing and regular text
        var text = "\u250cHi\u2510";

        // When
        var result = RenderThrough(text);

        // Then
        result.ShouldBe("+Hi+");
    }
}

using Shouldly;
using Spectre.Tui.Doodads.Doodads.Divider;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class DividerTests
{
    [Fact]
    public void MinWidth_Should_Be_One()
    {
        var divider = new Divider();
        divider.MinWidth.ShouldBe(1);
    }

    [Fact]
    public void MinHeight_Should_Be_One()
    {
        var divider = new Divider();
        divider.MinHeight.ShouldBe(1);
    }

    [Fact]
    public void Measure_Should_Fill_Available_Width()
    {
        var divider = new Divider();
        var size = divider.Measure(new Size(80, 24));

        size.Width.ShouldBe(80);
        size.Height.ShouldBe(1);
    }

    [Fact]
    public void Render_Should_Fill_Viewport_With_Default_Character()
    {
        // Given
        var divider = new Divider();
        var fixture = new TuiFixture(new Size(10, 1));

        // When
        var output = fixture.Render(divider);

        // Then
        output.ShouldContain("──────────");
    }

    [Fact]
    public void Render_Should_Use_Custom_Character()
    {
        // Given
        var divider = new Divider { Character = '=' };
        var fixture = new TuiFixture(new Size(10, 1));

        // When
        var output = fixture.Render(divider);

        // Then
        output.ShouldContain("==========");
    }

    [Fact]
    public void Default_Character_Should_Be_Box_Drawing_Horizontal()
    {
        var divider = new Divider();
        divider.Character.ShouldBe('\u2500');
    }
}
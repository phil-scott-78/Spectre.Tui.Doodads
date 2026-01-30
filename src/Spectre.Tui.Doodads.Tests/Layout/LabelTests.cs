using Shouldly;
using Spectre.Console;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Rendering;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class LabelTests
{
    [Fact]
    public void Should_Compute_Width_From_Text()
    {
        // Given / When
        var label = new Label("Hello", Appearance.Plain);

        // Then
        label.MinWidth.ShouldBe(5);
    }

    [Fact]
    public void Should_Return_Zero_Width_For_Empty_Text()
    {
        // Given / When
        var label = new Label("", Appearance.Plain);

        // Then
        label.MinWidth.ShouldBe(0);
    }

    [Fact]
    public void Should_Have_Height_Of_One()
    {
        // Given / When
        var label = new Label("anything", Appearance.Plain);

        // Then
        label.MinHeight.ShouldBe(1);
    }

    [Fact]
    public void Should_Construct_With_Appearance()
    {
        // Given
        var style = new Appearance { Decoration = Decoration.Bold, Foreground = Color.Red };

        // When
        var label = new Label("Test", style);

        // Then
        label.Text.ShouldBe("Test");
        label.Style.ShouldBe(style);
    }

    [Fact]
    public void Should_Construct_With_Style_Format_Bold_Yellow()
    {
        // Given / When
        var label = new Label("Warning", "bold text-yellow");

        // Then
        label.Text.ShouldBe("Warning");
        label.Style.Decoration.ShouldBe(Decoration.Bold);
        label.Style.Foreground.ShouldBe(Color.Yellow);
    }

    [Fact]
    public void Should_Construct_With_Style_Format_Dim()
    {
        // Given / When
        var label = new Label("Subtle", "dim");

        // Then
        label.Style.Decoration.ShouldBe(Decoration.Dim);
    }

    [Fact]
    public void Should_Construct_With_Hex_Color_Format()
    {
        // Given / When
        var label = new Label("Error", "text-#ff0000");

        // Then
        label.Style.Foreground.ShouldBe(new Color(255, 0, 0));
    }

    [Fact]
    public void Should_Render_Text_In_Layout()
    {
        // Given
        var fixture = new TuiFixture(new Size(30, 3));
        var label = new Label("Status OK", "bold text-green");

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{label}");
        });

        // Then
        output.ShouldContain("Status OK");
    }

    [Fact]
    public void Should_Render_Alongside_Other_Content_In_Layout()
    {
        // Given
        var fixture = new TuiFixture(new Size(40, 5));
        var label = new Label("Done!", "text-#00ff00");
        var title = "Results";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"""
                {title}

                {label}
                """);
        });

        // Then
        output.ShouldContain("Results");
        output.ShouldContain("Done!");
    }

    [Fact]
    public void Should_Update_Width_When_Text_Changes_Via_With()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        label.MinWidth.ShouldBe(2);

        // When
        var updated = label with { Text = "Hello World" };

        // Then
        updated.MinWidth.ShouldBe(11);
    }
}
using Shouldly;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Rendering;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class FlexRowTests
{
    [Fact]
    public void Single_Fill_Should_Render_At_Full_Width()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var label = new Label("Hello", Appearance.Plain);
        var row = Flex.Row().Add(label, FlexSize.Fill());

        // When
        var output = fixture.Render(row);

        // Then
        output.ShouldContain("Hello");
    }

    [Fact]
    public void Two_Fixed_Items_Should_Render_Side_By_Side()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var left = new Label("AA", Appearance.Plain);
        var right = new Label("BB", Appearance.Plain);
        var row = Flex.Row()
            .Add(left, FlexSize.Fixed(5))
            .Add(right, FlexSize.Fixed(5));

        // When
        var output = fixture.Render(row);

        // Then
        output.ShouldContain("AA");
        output.ShouldContain("BB");
    }

    [Fact]
    public void Gap_Should_Create_Space_Between_Items()
    {
        // Given — gap of 2 between two 3-char labels in 20-wide fixture
        var fixture = new TuiFixture(new Size(20, 3));
        var left = new Label("AAA", Appearance.Plain);
        var right = new Label("BBB", Appearance.Plain);
        var row = Flex.Row(gap: 2)
            .Add(left, FlexSize.Fixed(3))
            .Add(right, FlexSize.Fixed(3));

        // When
        var output = fixture.Render(row);

        // Then — items separated by gap
        var line = output.Split('\n')[0];
        line.ShouldContain("AAA");
        line.ShouldContain("BBB");
        // The gap means BB starts at column 5, not column 3
        line.IndexOf("BBB", StringComparison.Ordinal).ShouldBe(5);
    }

    [Fact]
    public void Empty_Row_Should_Render_Without_Error()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var row = Flex.Row();

        // When / Then — should not throw
        var output = fixture.Render(row);
        output.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Work_Inside_Layout_Interpolated_String()
    {
        // Given
        var fixture = new TuiFixture(new Size(30, 5));
        var label = new Label("Content", Appearance.Plain);
        var row = Flex.Row(minWidth: 30, minHeight: 1).Add(label, FlexSize.Fill());

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"""
                {"Title"}
                {row}
                """);
        });

        // Then
        output.ShouldContain("Title");
        output.ShouldContain("Content");
    }

    [Fact]
    public void Nested_FlexColumn_Inside_FlexRow_Should_Render()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var topLabel = new Label("Top", Appearance.Plain);
        var bottomLabel = new Label("Bot", Appearance.Plain);
        var col = Flex.Column()
            .Add(topLabel, FlexSize.Fixed(1))
            .Add(bottomLabel, FlexSize.Fixed(1));
        var sideLabel = new Label("Side", Appearance.Plain);
        var row = Flex.Row()
            .Add(col, FlexSize.Fixed(10))
            .Add(sideLabel, FlexSize.Fixed(10));

        // When
        var output = fixture.Render(row);

        // Then
        output.ShouldContain("Top");
        output.ShouldContain("Bot");
        output.ShouldContain("Side");
    }
}
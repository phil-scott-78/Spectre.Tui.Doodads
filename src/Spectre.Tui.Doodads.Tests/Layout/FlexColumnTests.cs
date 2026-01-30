using Shouldly;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class FlexColumnTests
{
    [Fact]
    public void Items_Should_Stack_Vertically()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var top = new Label("Top", Appearance.Plain);
        var bottom = new Label("Bot", Appearance.Plain);
        var col = Flex.Column()
            .Add(top, FlexSize.Fixed(1))
            .Add(bottom, FlexSize.Fixed(1));

        // When
        var output = fixture.Render(col);

        // Then
        var lines = output.Split('\n');
        lines[0].ShouldContain("Top");
        lines[1].ShouldContain("Bot");
    }

    [Fact]
    public void Height_Should_Distribute_By_Ratio()
    {
        // Given — 8 rows (fixture height) split 2:1 → ~5 + ~3
        var fixture = new TuiFixture(new Size(10, 8));
        var top = new Label("A", Appearance.Plain);
        var bottom = new Label("B", Appearance.Plain);
        var col = Flex.Column()
            .Add(top, FlexSize.Ratio(2))
            .Add(bottom, FlexSize.Ratio(1));

        // When
        var output = fixture.Render(col);

        // Then — top item occupies rows 0-4, bottom item starts at row 5
        var lines = output.Split('\n');
        lines[0].ShouldContain("A");
        lines[5].ShouldContain("B");
    }

    [Fact]
    public void Vertical_Gap_Should_Separate_Items()
    {
        // Given — gap of 1 between two items
        var fixture = new TuiFixture(new Size(20, 5));
        var top = new Label("Top", Appearance.Plain);
        var bottom = new Label("Bot", Appearance.Plain);
        var col = Flex.Column(gap: 1)
            .Add(top, FlexSize.Fixed(1))
            .Add(bottom, FlexSize.Fixed(1));

        // When
        var output = fixture.Render(col);

        // Then — top at row 0, gap at row 1, bottom at row 2
        var lines = output.Split('\n');
        lines[0].ShouldContain("Top");
        lines[2].ShouldContain("Bot");
    }

    [Fact]
    public void Empty_Column_Should_Render_Without_Error()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var col = Flex.Column();

        // When / Then — should not throw
        var output = fixture.Render(col);
        output.ShouldNotBeNull();
    }
}
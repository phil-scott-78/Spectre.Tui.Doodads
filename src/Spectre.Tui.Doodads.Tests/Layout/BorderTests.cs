using Shouldly;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Doodads.Progress;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Rendering;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class BorderTests
{
    [Fact]
    public void MinWidth_Should_Add_Two_To_Content_MinWidth()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label };

        // Then
        border.MinWidth.ShouldBe(label.MinWidth + 2);
    }

    [Fact]
    public void MinHeight_Should_Add_Two_To_Content_MinHeight()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label };

        // Then
        border.MinHeight.ShouldBe(label.MinHeight + 2);
    }

    [Fact]
    public void Measure_Should_Delegate_To_Content_And_Add_Chrome()
    {
        // Given — ProgressModel.Measure fills available width
        var progress = new ProgressModel { MinWidth = 10, ShowPercentage = false };
        var border = new Border { Content = progress };

        // When
        var result = border.Measure(new Size(30, 10));

        // Then — inner available is 28x8, progress measures as (28, 1),
        // so border measures as (30, 3)
        result.Width.ShouldBe(30);
        result.Height.ShouldBe(3);
    }

    [Fact]
    public void Should_Render_Border_Characters()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label };

        // When
        var output = fixture.Render(border);

        // Then — top-left corner should be present
        output.ShouldContain("\u250c");
        // Top-right corner
        output.ShouldContain("\u2510");
        // Bottom-left corner
        output.ShouldContain("\u2514");
        // Bottom-right corner
        output.ShouldContain("\u2518");
        // Content should be visible
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_Content_Inside_Border()
    {
        // Given
        var fixture = new TuiFixture(new Size(12, 5));
        var label = new Label("Test", Appearance.Plain);
        var border = new Border { Content = label };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — content "Test" should appear on the second line (row 1)
        lines[1].ShouldContain("Test");
    }

    [Fact]
    public void Should_Handle_Viewport_Too_Small_For_Border()
    {
        // Given — 1x1 viewport is too small for any border
        var fixture = new TuiFixture(new Size(1, 1));
        var label = new Label("X", Appearance.Plain);
        var border = new Border { Content = label };

        // When / Then — should not throw
        var output = fixture.Render(border);
        output.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Work_In_Layout_Template()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label };

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{border}");
        });

        // Then
        output.ShouldContain("\u250c");
        output.ShouldContain("Hi");
    }

    // --- Border Style Tests ---

    [Fact]
    public void Should_Render_With_Rounded_Style()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, BorderStyle = BorderStyle.Rounded };

        // When
        var output = fixture.Render(border);

        // Then — rounded corners
        output.ShouldContain("\u256d"); // ╭
        output.ShouldContain("\u256e"); // ╮
        output.ShouldContain("\u2570"); // ╰
        output.ShouldContain("\u256f"); // ╯
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_With_Double_Style()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, BorderStyle = BorderStyle.Double };

        // When
        var output = fixture.Render(border);

        // Then — double-line corners
        output.ShouldContain("\u2554"); // ╔
        output.ShouldContain("\u2557"); // ╗
        output.ShouldContain("\u255a"); // ╚
        output.ShouldContain("\u255d"); // ╝
        // Double-line edges
        output.ShouldContain("\u2550"); // ═
        output.ShouldContain("\u2551"); // ║
    }

    [Fact]
    public void Should_Render_With_Thick_Style()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, BorderStyle = BorderStyle.Thick };

        // When
        var output = fixture.Render(border);

        // Then — thick corners
        output.ShouldContain("\u250f"); // ┏
        output.ShouldContain("\u2513"); // ┓
        output.ShouldContain("\u2517"); // ┗
        output.ShouldContain("\u251b"); // ┛
    }

    [Fact]
    public void Should_Render_With_Ascii_Style()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, BorderStyle = BorderStyle.Ascii };

        // When
        var output = fixture.Render(border);

        // Then
        output.ShouldContain("+");
        output.ShouldContain("-");
        output.ShouldContain("|");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_With_Custom_Style()
    {
        // Given
        var custom = new BorderStyle
        {
            Top = "=",
            Bottom = "=",
            Left = "!",
            Right = "!",
            TopLeft = "#",
            TopRight = "#",
            BottomLeft = "#",
            BottomRight = "#",
        };
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, BorderStyle = custom };

        // When
        var output = fixture.Render(border);

        // Then
        output.ShouldContain("#");
        output.ShouldContain("=");
        output.ShouldContain("!");
        output.ShouldContain("AB");
    }

    // --- Side Visibility Tests ---

    [Fact]
    public void MinWidth_Should_Adjust_When_Left_Hidden()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label, ShowLeft = false };

        // Then
        border.MinWidth.ShouldBe(label.MinWidth + 1); // only right chrome
    }

    [Fact]
    public void MinWidth_Should_Adjust_When_Right_Hidden()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label, ShowRight = false };

        // Then
        border.MinWidth.ShouldBe(label.MinWidth + 1); // only left chrome
    }

    [Fact]
    public void MinWidth_Should_Adjust_When_Both_Sides_Hidden()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label, ShowLeft = false, ShowRight = false };

        // Then
        border.MinWidth.ShouldBe(label.MinWidth); // no horizontal chrome
    }

    [Fact]
    public void MinHeight_Should_Adjust_When_Top_Hidden()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label, ShowTop = false };

        // Then
        border.MinHeight.ShouldBe(label.MinHeight + 1); // only bottom chrome
    }

    [Fact]
    public void MinHeight_Should_Adjust_When_Bottom_Hidden()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label, ShowBottom = false };

        // Then
        border.MinHeight.ShouldBe(label.MinHeight + 1); // only top chrome
    }

    [Fact]
    public void MinHeight_Should_Adjust_When_Top_And_Bottom_Hidden()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var border = new Border { Content = label, ShowTop = false, ShowBottom = false };

        // Then
        border.MinHeight.ShouldBe(label.MinHeight); // no vertical chrome
    }

    [Fact]
    public void Measure_Should_Adjust_For_Hidden_Sides()
    {
        // Given
        var progress = new ProgressModel { MinWidth = 10, ShowPercentage = false };
        var border = new Border { Content = progress, ShowLeft = false, ShowTop = false };

        // When — inner available: width - 1 (right only), height - 1 (bottom only)
        var result = border.Measure(new Size(30, 10));

        // Then — inner available is 29x9, progress measures (29, 1),
        // so border measures (30, 2)
        result.Width.ShouldBe(30);
        result.Height.ShouldBe(2);
    }

    [Fact]
    public void Should_Render_Without_Top_Border()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, ShowTop = false };

        // When
        var output = fixture.Render(border);

        // Then — no top corners
        output.ShouldNotContain("\u250c");
        output.ShouldNotContain("\u2510");
        // Bottom corners still present
        output.ShouldContain("\u2514");
        output.ShouldContain("\u2518");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_Without_Bottom_Border()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, ShowBottom = false };

        // When
        var output = fixture.Render(border);

        // Then — no bottom corners
        output.ShouldNotContain("\u2514");
        output.ShouldNotContain("\u2518");
        // Top corners still present
        output.ShouldContain("\u250c");
        output.ShouldContain("\u2510");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_Without_Left_Border()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, ShowLeft = false };

        // When
        var output = fixture.Render(border);

        // Then — no left corners
        output.ShouldNotContain("\u250c");
        output.ShouldNotContain("\u2514");
        // Right corners still present
        output.ShouldContain("\u2510");
        output.ShouldContain("\u2518");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_Without_Right_Border()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, ShowRight = false };

        // When
        var output = fixture.Render(border);

        // Then — no right corners
        output.ShouldNotContain("\u2510");
        output.ShouldNotContain("\u2518");
        // Left corners still present
        output.ShouldContain("\u250c");
        output.ShouldContain("\u2514");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_With_Only_Top_And_Bottom()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, ShowLeft = false, ShowRight = false };

        // When
        var output = fixture.Render(border);

        // Then — no corners (both adjacent sides missing for all corners)
        output.ShouldNotContain("\u250c");
        output.ShouldNotContain("\u2510");
        output.ShouldNotContain("\u2514");
        output.ShouldNotContain("\u2518");
        // Top and bottom edges still present
        output.ShouldContain("\u2500");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_With_Only_Left_And_Right()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, ShowTop = false, ShowBottom = false };

        // When
        var output = fixture.Render(border);

        // Then — no corners
        output.ShouldNotContain("\u250c");
        output.ShouldNotContain("\u2510");
        output.ShouldNotContain("\u2514");
        output.ShouldNotContain("\u2518");
        // Side edges still present
        output.ShouldContain("\u2502");
        output.ShouldContain("AB");
    }

    // --- Title Tests ---

    [Fact]
    public void Should_Render_Title_In_Top_Border()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, Title = "Hello" };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — title should appear in the top border line
        lines[0].ShouldContain("Hello");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Title_Should_Be_Padded_With_Spaces()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, Title = "Test" };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — title should have space padding (rendered as • in test fixture)
        lines[0].ShouldContain("\u2022Test\u2022"); // •Test•
    }

    [Fact]
    public void Title_Should_Be_Left_Aligned_By_Default()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var label = new Label("X", Appearance.Plain);
        var border = new Border { Content = label, Title = "Hi" };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — title should appear near the start of the top line
        // Spaces render as • in test fixture
        var hiIndex = lines[0].IndexOf("\u2022Hi\u2022", StringComparison.Ordinal);
        hiIndex.ShouldBeGreaterThanOrEqualTo(0);
        // Should be near the left corner (position 1 for left-aligned)
        hiIndex.ShouldBeLessThan(5);
    }

    [Fact]
    public void Title_Should_Support_Center_Alignment()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var label = new Label("X", Appearance.Plain);
        var border = new Border
        {
            Content = label,
            Title = "Hi",
            TitleAlignment = TitleAlignment.Center,
        };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — title should be roughly centered
        // Spaces render as • in test fixture
        var hiIndex = lines[0].IndexOf("\u2022Hi\u2022", StringComparison.Ordinal);
        hiIndex.ShouldBeGreaterThan(5); // not near left
        hiIndex.ShouldBeLessThan(15); // not near right
    }

    [Fact]
    public void Title_Should_Support_Right_Alignment()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var label = new Label("X", Appearance.Plain);
        var border = new Border
        {
            Content = label,
            Title = "Hi",
            TitleAlignment = TitleAlignment.Right,
        };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — title should appear near the end of the top line
        // Spaces render as • in test fixture
        var hiIndex = lines[0].IndexOf("\u2022Hi\u2022", StringComparison.Ordinal);
        hiIndex.ShouldBeGreaterThan(10);
    }

    [Fact]
    public void Title_Should_Truncate_With_Ellipsis_When_Too_Long()
    {
        // Given — width 10: corner + 8 inner + corner, title area = 8, max title = 6
        var fixture = new TuiFixture(new Size(10, 3));
        var label = new Label("X", Appearance.Plain);
        var border = new Border { Content = label, Title = "VeryLongTitle" };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — should contain ellipsis character
        lines[0].ShouldContain("\u2026"); // …
        // Original title should NOT appear in full
        lines[0].ShouldNotContain("VeryLongTitle");
    }

    [Fact]
    public void Title_Should_Be_Ignored_When_Top_Hidden()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, Title = "Hello", ShowTop = false };

        // When
        var output = fixture.Render(border);

        // Then — title should not appear
        output.ShouldNotContain("Hello");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Null_Title_Should_Not_Render()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, Title = null };

        // When
        var output = fixture.Render(border);

        // Then — should render normally without title
        output.ShouldContain("\u250c");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Empty_Title_Should_Not_Render()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border { Content = label, Title = "" };

        // When
        var output = fixture.Render(border);

        // Then — should render normally without title content modification
        output.ShouldContain("\u250c");
        output.ShouldContain("AB");
    }

    [Fact]
    public void Should_Render_Title_With_Different_Border_Style()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("AB", Appearance.Plain);
        var border = new Border
        {
            Content = label,
            BorderStyle = BorderStyle.Rounded,
            Title = "Info",
        };

        // When
        var output = fixture.Render(border);
        var lines = output.Split('\n');

        // Then — rounded corners with title
        output.ShouldContain("\u256d"); // ╭
        lines[0].ShouldContain("Info");
        output.ShouldContain("AB");
    }
}

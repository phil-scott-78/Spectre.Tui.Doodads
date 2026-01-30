using Shouldly;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Doodads.Progress;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Rendering;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class PaddingTests
{
    [Fact]
    public void MinWidth_Should_Add_Left_And_Right_To_Content()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var padded = new Padding { Content = label, Left = 2, Right = 3 };

        // Then
        padded.MinWidth.ShouldBe(label.MinWidth + 2 + 3);
    }

    [Fact]
    public void MinHeight_Should_Add_Top_And_Bottom_To_Content()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var padded = new Padding { Content = label, Top = 1, Bottom = 2 };

        // Then
        padded.MinHeight.ShouldBe(label.MinHeight + 1 + 2);
    }

    [Fact]
    public void Measure_Should_Delegate_To_Content_And_Add_Padding()
    {
        // Given — ProgressModel.Measure fills available width
        var progress = new ProgressModel { MinWidth = 10, ShowPercentage = false };
        var padded = new Padding { Content = progress, Left = 2, Right = 3, Top = 1, Bottom = 1 };

        // When
        var result = padded.Measure(new Size(30, 10));

        // Then — inner available is 25x8, progress measures (25, 1),
        // so padded measures (30, 3)
        result.Width.ShouldBe(30);
        result.Height.ShouldBe(3);
    }

    [Fact]
    public void Should_Render_Content_At_Padded_Position()
    {
        // Given — 2 columns of left padding should shift content right
        var fixture = new TuiFixture(new Size(15, 5));
        var label = new Label("AB", Appearance.Plain);
        var padded = new Padding { Content = label, Left = 2, Top = 1 };

        // When
        var output = fixture.Render(padded);
        var lines = output.Split('\n');

        // Then — content should be on row 1 (after 1 row of top padding)
        // and shifted right by 2 columns (left padding)
        lines[1].ShouldContain("AB");
        // Row 0 should be empty (padding row)
        lines[0].ShouldNotContain("AB");
    }

    [Fact]
    public void Should_Handle_Zero_Padding()
    {
        // Given
        var fixture = new TuiFixture(new Size(10, 3));
        var label = new Label("XY", Appearance.Plain);
        var padded = new Padding { Content = label };

        // When
        var output = fixture.Render(padded);

        // Then — content should render at origin
        output.ShouldContain("XY");
    }

    [Fact]
    public void Should_Handle_Padding_Larger_Than_Viewport()
    {
        // Given — padding exceeds viewport, so inner dimensions are 0
        var fixture = new TuiFixture(new Size(5, 3));
        var label = new Label("X", Appearance.Plain);
        var padded = new Padding { Content = label, Left = 3, Right = 3 };

        // When / Then — should not throw
        var output = fixture.Render(padded);
        output.ShouldNotBeNull();
    }

    [Fact]
    public void Should_Work_In_Layout_Template()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var label = new Label("Hi", Appearance.Plain);
        var padded = new Padding { Content = label, Left = 1, Top = 1 };

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{padded}");
        });

        // Then
        output.ShouldContain("Hi");
    }

    [Fact]
    public void Should_Nest_Inside_Border()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 7));
        var label = new Label("Hi", Appearance.Plain);
        var padded = new Padding { Content = label, Left = 1, Top = 1 };
        var bordered = new Border { Content = padded };

        // When
        var output = fixture.Render(bordered);

        // Then — should have border characters and content
        output.ShouldContain("\u250c");
        output.ShouldContain("Hi");
    }

    // --- Convenience Factory Method Tests ---

    [Fact]
    public void All_Should_Set_Equal_Padding_On_All_Sides()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);

        // When
        var padded = Padding.All(3, label);

        // Then
        padded.Top.ShouldBe(3);
        padded.Right.ShouldBe(3);
        padded.Bottom.ShouldBe(3);
        padded.Left.ShouldBe(3);
        padded.Content.ShouldBe(label);
    }

    [Fact]
    public void Horizontal_Should_Set_Left_And_Right_Padding()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);

        // When
        var padded = Padding.Horizontal(5, label);

        // Then
        padded.Left.ShouldBe(5);
        padded.Right.ShouldBe(5);
        padded.Top.ShouldBe(0);
        padded.Bottom.ShouldBe(0);
        padded.Content.ShouldBe(label);
    }

    [Fact]
    public void Vertical_Should_Set_Top_And_Bottom_Padding()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);

        // When
        var padded = Padding.Vertical(2, label);

        // Then
        padded.Top.ShouldBe(2);
        padded.Bottom.ShouldBe(2);
        padded.Left.ShouldBe(0);
        padded.Right.ShouldBe(0);
        padded.Content.ShouldBe(label);
    }

    [Fact]
    public void Symmetric_Should_Set_Vertical_And_Horizontal_Padding()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);

        // When
        var padded = Padding.Symmetric(1, 4, label);

        // Then
        padded.Top.ShouldBe(1);
        padded.Bottom.ShouldBe(1);
        padded.Left.ShouldBe(4);
        padded.Right.ShouldBe(4);
        padded.Content.ShouldBe(label);
    }

    [Fact]
    public void All_Should_Render_Correctly()
    {
        // Given
        var fixture = new TuiFixture(new Size(15, 7));
        var label = new Label("AB", Appearance.Plain);
        var padded = Padding.All(2, label);

        // When
        var output = fixture.Render(padded);
        var lines = output.Split('\n');

        // Then — content should be on row 2 (after 2 rows of top padding)
        lines[2].ShouldContain("AB");
        // Rows 0 and 1 should not contain content
        lines[0].ShouldNotContain("AB");
        lines[1].ShouldNotContain("AB");
    }

    [Fact]
    public void Horizontal_MinWidth_Should_Include_Both_Sides()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var padded = Padding.Horizontal(3, label);

        // Then
        padded.MinWidth.ShouldBe(label.MinWidth + 6); // 3 left + 3 right
        padded.MinHeight.ShouldBe(label.MinHeight); // no vertical padding
    }

    [Fact]
    public void Vertical_MinHeight_Should_Include_Both_Sides()
    {
        // Given
        var label = new Label("Hi", Appearance.Plain);
        var padded = Padding.Vertical(2, label);

        // Then
        padded.MinHeight.ShouldBe(label.MinHeight + 4); // 2 top + 2 bottom
        padded.MinWidth.ShouldBe(label.MinWidth); // no horizontal padding
    }
}
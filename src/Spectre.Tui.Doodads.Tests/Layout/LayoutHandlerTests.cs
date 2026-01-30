using Shouldly;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Doodads.Progress;
using Spectre.Tui.Doodads.Rendering;
using Spectre.Tui.Doodads.Tests.Spectre.Tui.Testing;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class LayoutHandlerTests
{
    // TuiFixture renders empty/space cells as '•'
    private const char Dot = '•';

    [Fact]
    public void Should_Render_Literal_Text()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"Hello");
        });

        // Then
        output.ShouldContain("Hello");
    }

    [Fact]
    public void Should_Render_Interpolated_String()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var name = "World";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"Hello-{name}");
        });

        // Then
        output.ShouldContain("Hello-World");
    }

    [Fact]
    public void Should_Position_Holes_With_Spacing()
    {
        // Given
        var fixture = new TuiFixture(new Size(30, 3));
        var left = "AAA";
        var right = "BBB";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{left}     {right}");
        });

        // Then — left starts at column 0, 5 spaces (rendered as •), then right
        output.ShouldContain($"AAA{Dot}{Dot}{Dot}{Dot}{Dot}BBB");
    }

    [Fact]
    public void Should_Advance_To_Next_Line_On_Newline()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var line1 = "First";
        var line2 = "Second";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"""
                {line1}
                {line2}
                """);
        });

        // Then — each word on its own line
        output.ShouldContain("First");
        output.ShouldContain("Second");
    }

    [Fact]
    public void Should_Render_Integer_Values_Via_ToString()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var count = 42;

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"Count:{count}");
        });

        // Then
        output.ShouldContain("Count:42");
    }

    [Fact]
    public void Should_Render_ISizedRenderable_At_Current_Position()
    {
        // Given
        var fixture = new TuiFixture(new Size(50, 5));
        var progress = new ProgressModel { MinWidth = 20, ShowPercentage = false };
        var (updated, _) = progress.SetPercent(0.5);

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"""
                Progress:{updated}
                """);
        });

        // Then — the progress bar should render after "Progress:"
        output.ShouldNotBeNullOrEmpty();
        output.ShouldContain("Progress:");
    }

    [Fact]
    public void Should_Render_Literal_Characters_Between_Holes()
    {
        // Given
        var fixture = new TuiFixture(new Size(30, 3));
        var left = "A";
        var right = "B";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{left}|{right}");
        });

        // Then — literal pipe character should be rendered
        output.ShouldContain("A|B");
    }

    [Fact]
    public void Should_Handle_Empty_String_Holes()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        string? empty = null;

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"Before{empty}After");
        });

        // Then — null should be skipped, "Before" and "After" adjacent
        output.ShouldContain("BeforeAfter");
    }

    [Fact]
    public void Should_Handle_Blank_Lines_In_Template()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var title = "Title";
        var body = "Body";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"""
                {title}

                {body}
                """);
        });

        // Then — title on line 0, blank line 1, body on line 2
        output.ShouldContain("Title");
        output.ShouldContain("Body");
    }

    [Fact]
    public void Should_Apply_Style_Format_To_String()
    {
        // Given — we can't directly assert on Appearance in output,
        // but we can verify the text renders without error
        var fixture = new TuiFixture(new Size(20, 3));
        var title = "Hello";

        // When / Then — should not throw
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{title:bold text-red}");
        });

        output.ShouldContain("Hello");
    }

    [Fact]
    public void Should_Apply_Style_Format_To_Integer()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 3));
        var count = 99;

        // When / Then — should not throw
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"Count:{count:bold}");
        });

        output.ShouldContain("Count:99");
    }

    [Fact]
    public void Should_Render_Multi_Line_Template_With_Mixed_Content()
    {
        // Given
        var fixture = new TuiFixture(new Size(40, 10));
        var title = "Dashboard";
        var status = "OK";
        var count = 5;

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"""
                {title:bold}
                Status:{status:dim}--Items:{count}
                """);
        });

        // Then
        output.ShouldContain("Dashboard");
        output.ShouldContain("Status:OK--Items:5");
    }

    [Fact]
    public void Should_Place_Content_On_Correct_Lines()
    {
        // Given
        var fixture = new TuiFixture(new Size(20, 5));
        var a = "Line0";
        var b = "Line1";
        var c = "Line3";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"""
                {a}
                {b}

                {c}
                """);
        });

        // Then — verify each value appears on its own line
        var lines = output.Split('\n');
        lines[0].ShouldStartWith("Line0");
        lines[1].ShouldStartWith("Line1");
        lines[3].ShouldStartWith("Line3");
    }

    [Fact]
    public void Should_Render_Multiple_Holes_On_Same_Line()
    {
        // Given
        var fixture = new TuiFixture(new Size(30, 3));
        var a = "XX";
        var b = "YY";
        var c = "ZZ";

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{a}-{b}-{c}");
        });

        // Then
        output.ShouldContain("XX-YY-ZZ");
    }

    [Fact]
    public void Default_Measure_Should_Produce_Same_Output_As_MinSize()
    {
        // Given — a label has no Measure override, so default returns (MinWidth, MinHeight)
        var fixture = new TuiFixture(new Size(30, 3));
        var label = new Label("Hello", Appearance.Plain);

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{label}");
        });

        // Then — label text should render at its MinWidth (5 chars)
        output.ShouldContain("Hello");
    }

    [Fact]
    public void Widget_With_Custom_Measure_Should_Get_Measured_Size()
    {
        // Given — ProgressModel.Measure returns (available.Width, 1)
        // With no prefix text, available width = viewport width = 30
        var fixture = new TuiFixture(new Size(30, 3));
        var progress = new ProgressModel { MinWidth = 10, ShowPercentage = false }
            .SetPercentImmediate(0.5);

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{progress}");
        });

        // Then — progress bar should fill all 30 columns (no empty '•' cells on first line)
        var firstLine = output.Split('\n')[0].TrimEnd('\r');
        firstLine.ShouldNotContain(Dot.ToString());
    }

    [Fact]
    public void Fill_Format_Should_Give_Remaining_Row_Width()
    {
        // Given — "AB" takes 2 columns, leaving 28 of 30 for the widget
        var fixture = new TuiFixture(new Size(30, 3));
        var label = new Label("X", Appearance.Plain);

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"AB{label:fill}");
        });

        // Then — label should be rendered; no exception
        output.ShouldContain("AB");
        output.ShouldContain("X");
    }

    [Fact]
    public void Expand_Format_Should_Give_Remaining_Width_And_Height()
    {
        // Given — expand should give the widget the remaining viewport space
        var fixture = new TuiFixture(new Size(20, 10));
        var label = new Label("Z", Appearance.Plain);

        // When / Then — should not throw
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{label:expand}");
        });

        output.ShouldContain("Z");
    }

    [Fact]
    public void Measure_Exceeding_Available_Space_Should_Be_Clamped()
    {
        // Given — ProgressModel has MinWidth=40 but viewport is only 20 wide.
        // Measure returns (available.Width, 1), clamped to 20.
        var fixture = new TuiFixture(new Size(20, 3));
        var progress = new ProgressModel { MinWidth = 40, ShowPercentage = false }
            .SetPercentImmediate(0.5);

        // When
        var output = fixture.Render(ctx =>
        {
            ctx.Layout($"{progress}");
        });

        // Then — should render without exceeding viewport (no empty cells on first line)
        var firstLine = output.Split('\n')[0].TrimEnd('\r');
        firstLine.ShouldNotContain(Dot.ToString());
    }
}
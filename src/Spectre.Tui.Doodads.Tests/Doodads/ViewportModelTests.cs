using Shouldly;
using Spectre.Tui.Doodads.Doodads.Viewport;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class ViewportModelTests
{
    private static ViewportModel CreateViewport(string content, int height = 5)
    {
        return new ViewportModel { MinWidth = 40, MinHeight = height }
            .SetContent(content);
    }

    [Fact]
    public void SetContent_Should_Split_Lines()
    {
        var vp = CreateViewport("Line1\nLine2\nLine3");
        vp.TotalLines.ShouldBe(3);
    }

    [Fact]
    public void Initial_State_Should_Be_At_Top()
    {
        var vp = CreateViewport("A\nB\nC\nD\nE\nF\nG");
        vp.AtTop.ShouldBeTrue();
        vp.AtBottom.ShouldBeFalse();
    }

    [Fact]
    public void LineDown_Should_Scroll_Down()
    {
        var vp = CreateViewport("A\nB\nC\nD\nE\nF\nG");
        var (scrolled, _) = vp.LineDown();

        scrolled.YOffset.ShouldBe(1);
        scrolled.AtTop.ShouldBeFalse();
    }

    [Fact]
    public void LineUp_Should_Not_Go_Below_Zero()
    {
        var vp = CreateViewport("A\nB\nC");
        var (scrolled, _) = vp.LineUp();

        scrolled.YOffset.ShouldBe(0);
    }

    [Fact]
    public void GotoBottom_Should_Scroll_To_End()
    {
        var vp = CreateViewport("A\nB\nC\nD\nE\nF\nG\nH\nI\nJ", height: 3);
        var (scrolled, _) = vp.GotoBottom();

        scrolled.AtBottom.ShouldBeTrue();
    }

    [Fact]
    public void GotoTop_Should_Scroll_To_Start()
    {
        var vp = CreateViewport("A\nB\nC\nD\nE\nF\nG", height: 3)
            with
        { YOffset = 3 };
        var (scrolled, _) = vp.GotoTop();

        scrolled.YOffset.ShouldBe(0);
        scrolled.AtTop.ShouldBeTrue();
    }

    [Fact]
    public void PageDown_Should_Scroll_By_Height()
    {
        var vp = CreateViewport("A\nB\nC\nD\nE\nF\nG\nH\nI\nJ", height: 3);
        var (scrolled, _) = vp.PageDown();

        scrolled.YOffset.ShouldBe(3);
    }

    [Fact]
    public void ScrollPercent_Should_Be_Zero_At_Top()
    {
        var vp = CreateViewport("A\nB\nC\nD\nE");
        vp.ScrollPercent.ShouldBe(0.0);
    }

    [Fact]
    public void VisibleLines_Should_Be_Capped_By_Content()
    {
        var vp = CreateViewport("A\nB", height: 10);
        vp.VisibleLines.ShouldBe(2);
    }
}
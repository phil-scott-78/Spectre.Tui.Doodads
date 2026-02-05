using Shouldly;

namespace Spectre.Tui.Doodads.Tests;

public sealed class ScrollHelperTests
{
    [Fact]
    public void Position_Within_Viewport_Returns_Same_Offset()
    {
        // Given — position 3 is within viewport [2..12)
        var offset = 2;
        var viewportSize = 10;
        var position = 3;

        // When
        var result = ScrollHelper.EnsureVisible(position, offset, viewportSize);

        // Then
        result.ShouldBe(2);
    }

    [Fact]
    public void Position_Above_Viewport_Snaps_Down()
    {
        // Given — position 1 is before offset 5
        var offset = 5;
        var viewportSize = 10;
        var position = 1;

        // When
        var result = ScrollHelper.EnsureVisible(position, offset, viewportSize);

        // Then
        result.ShouldBe(1);
    }

    [Fact]
    public void Position_Below_Viewport_Snaps_Up()
    {
        // Given — position 15 is beyond viewport [0..10)
        var offset = 0;
        var viewportSize = 10;
        var position = 15;

        // When
        var result = ScrollHelper.EnsureVisible(position, offset, viewportSize);

        // Then — offset = 15 - 10 + 1 = 6
        result.ShouldBe(6);
    }

    [Fact]
    public void Position_At_Viewport_Start_Is_Visible()
    {
        // Given — position equals offset
        var offset = 5;
        var viewportSize = 10;
        var position = 5;

        // When
        var result = ScrollHelper.EnsureVisible(position, offset, viewportSize);

        // Then
        result.ShouldBe(5);
    }

    [Fact]
    public void Position_At_Viewport_End_Scrolls()
    {
        // Given — position == offset + viewportSize triggers scroll
        var offset = 0;
        var viewportSize = 10;
        var position = 10;

        // When
        var result = ScrollHelper.EnsureVisible(position, offset, viewportSize);

        // Then — offset = 10 - 10 + 1 = 1
        result.ShouldBe(1);
    }

    [Fact]
    public void Position_Just_Inside_Viewport_End_Does_Not_Scroll()
    {
        // Given — position = offset + viewportSize - 1 is the last visible position
        var offset = 0;
        var viewportSize = 10;
        var position = 9;

        // When
        var result = ScrollHelper.EnsureVisible(position, offset, viewportSize);

        // Then
        result.ShouldBe(0);
    }

    [Fact]
    public void Position_Zero_With_Zero_Offset()
    {
        // Given
        var offset = 0;
        var viewportSize = 5;
        var position = 0;

        // When
        var result = ScrollHelper.EnsureVisible(position, offset, viewportSize);

        // Then
        result.ShouldBe(0);
    }
}

using System.Collections.Immutable;
using Shouldly;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Rendering;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class FlexAlgorithmTests
{
    private sealed record TestWidget(int MinWidth) : ISizedRenderable
    {
        public int MinHeight => 1;

        public void Render(IRenderSurface surface) { }
    }

    private static FlexItem Item(FlexSize size, int minWidth = 0)
    {
        var widget = new TestWidget(minWidth);
        return new FlexItem(widget, size);
    }

    private static int[] MinSizes(ImmutableArray<FlexItem> items)
    {
        var result = new int[items.Length];
        for (var i = 0; i < items.Length; i++)
        {
            var min = items[i].Widget.MinWidth;
            if (items[i].Size is FlexSize.FixedSize fixedSize)
            {
                min = Math.Max(min, fixedSize.Characters);
            }

            result[i] = min;
        }

        return result;
    }

    [Fact]
    public void Empty_Items_Should_Return_Empty_Array()
    {
        // Given / When
        var result = FlexAlgorithm.Distribute(100, 0, [], []);

        // Then
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Single_Fill_Should_Get_All_Space()
    {
        // Given
        var items = ImmutableArray.Create(Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(80, 0, items, MinSizes(items));

        // Then
        result.ShouldBe([80]);
    }

    [Fact]
    public void Fixed_Items_Should_Get_Exact_Space()
    {
        // Given
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(10)),
            Item(FlexSize.Fixed(20)));

        // When
        var result = FlexAlgorithm.Distribute(80, 0, items, MinSizes(items));

        // Then
        result.ShouldBe([10, 20]);
    }

    [Fact]
    public void Remaining_Space_Should_Distribute_By_Weight()
    {
        // Given — 100 total, 20 fixed, 80 remaining split 3:1
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(20)),
            Item(FlexSize.Ratio(3)),
            Item(FlexSize.Ratio(1)));

        // When
        var result = FlexAlgorithm.Distribute(100, 0, items, MinSizes(items));

        // Then
        result.ShouldBe([20, 60, 20]);
    }

    [Fact]
    public void Gap_Should_Be_Subtracted_Before_Distribution()
    {
        // Given — 100 total, gap 5, 2 items → 95 usable, all fill → 47 + 48
        var items = ImmutableArray.Create(
            Item(FlexSize.Fill()),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(100, 5, items, MinSizes(items));

        // Then
        result[0].ShouldBe(47);
        result[1].ShouldBe(48);
        (result[0] + result[1]).ShouldBe(95);
    }

    [Fact]
    public void Rounding_Remainder_Should_Go_To_Last_Ratio_Item()
    {
        // Given — 10 total, 3 fill items → 3 + 3 + 4
        var items = ImmutableArray.Create(
            Item(FlexSize.Fill()),
            Item(FlexSize.Fill()),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(10, 0, items, MinSizes(items));

        // Then
        result.ShouldBe([3, 3, 4]);
    }

    [Fact]
    public void Fixed_Exceeding_Space_Should_Scale_Down()
    {
        // Given — 10 total, two fixed items totaling 20
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(15)),
            Item(FlexSize.Fixed(5)));

        // When
        var result = FlexAlgorithm.Distribute(10, 0, items, MinSizes(items));

        // Then — scaled proportionally: 15/20*10=7, 5/20*10=2, remainder 1 to last
        result.Sum().ShouldBe(10);
        result[0].ShouldBeGreaterThan(result[1]);
    }

    [Fact]
    public void Fixed_Exceeding_Space_Should_Give_Ratio_Items_Zero()
    {
        // Given — 10 total, fixed items exceed space, ratio item should get 0
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(15)),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(10, 0, items, MinSizes(items));

        // Then — fixed gets scaled to 10, fill gets 0
        result.ShouldBe([10, 0]);
    }

    [Fact]
    public void Zero_Total_Space_Should_Return_All_Zeros()
    {
        // Given
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(10)),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(0, 0, items, MinSizes(items));

        // Then
        result.ShouldBe([0, 0]);
    }

    [Fact]
    public void Fixed_Zero_Should_Allocate_Nothing()
    {
        // Given
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(0)),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(80, 0, items, MinSizes(items));

        // Then
        result.ShouldBe([0, 80]);
    }

    [Fact]
    public void Large_Gap_Consuming_All_Space_Should_Give_Zero_To_Items()
    {
        // Given — gap * (n-1) >= totalSpace
        var items = ImmutableArray.Create(
            Item(FlexSize.Fill()),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(10, 20, items, MinSizes(items));

        // Then
        result.ShouldBe([0, 0]);
    }

    [Fact]
    public void Mixed_Ratio_And_Fill_Should_Distribute_Correctly()
    {
        // Given — Fill is Ratio(1), so Ratio(2) + Fill = 2:1 split of 90 remaining
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(10)),
            Item(FlexSize.Ratio(2)),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(100, 0, items, MinSizes(items));

        // Then
        result.ShouldBe([10, 60, 30]);
    }

    [Fact]
    public void Total_Allocation_Should_Always_Equal_Available_Space()
    {
        // Given — an awkward distribution that tests rounding
        var items = ImmutableArray.Create(
            Item(FlexSize.Fixed(7)),
            Item(FlexSize.Ratio(3)),
            Item(FlexSize.Ratio(2)),
            Item(FlexSize.Fill()));

        // When
        var result = FlexAlgorithm.Distribute(100, 3, items, MinSizes(items));

        // Then — available = 100 - 3*3 = 91, fixed=7, remaining=84 split 3:2:1
        var available = 100 - (3 * 3);
        result.Sum().ShouldBe(available);
    }

    [Fact]
    public void Min_Sizes_Should_Be_Included_In_Distribution()
    {
        // Given — two fill items with minimum 2 each
        var items = ImmutableArray.Create(
            Item(FlexSize.Fill(), minWidth: 2),
            Item(FlexSize.Fill(), minWidth: 2));

        // When
        var result = FlexAlgorithm.Distribute(10, 0, items, MinSizes(items));

        // Then — min 2 each, remaining 6 split evenly
        result.ShouldBe([5, 5]);
    }
}
using Shouldly;
using Spectre.Tui.Doodads.Layout;

namespace Spectre.Tui.Doodads.Tests.Layout;

public sealed class FlexSizeTests
{
    [Fact]
    public void Fixed_Should_Create_FixedSize()
    {
        // Given / When
        var size = FlexSize.Fixed(20);

        // Then
        size.ShouldBeOfType<FlexSize.FixedSize>();
        ((FlexSize.FixedSize)size).Characters.ShouldBe(20);
    }

    [Fact]
    public void Fixed_Zero_Should_Create_FixedSize_With_Zero()
    {
        // Given / When
        var size = FlexSize.Fixed(0);

        // Then
        ((FlexSize.FixedSize)size).Characters.ShouldBe(0);
    }

    [Fact]
    public void Fixed_Negative_Should_Clamp_To_Zero()
    {
        // Given / When
        var size = FlexSize.Fixed(-5);

        // Then
        ((FlexSize.FixedSize)size).Characters.ShouldBe(0);
    }

    [Fact]
    public void Ratio_Should_Create_RatioSize()
    {
        // Given / When
        var size = FlexSize.Ratio(3);

        // Then
        size.ShouldBeOfType<FlexSize.RatioSize>();
        ((FlexSize.RatioSize)size).Weight.ShouldBe(3);
    }

    [Fact]
    public void Ratio_Zero_Should_Clamp_To_One()
    {
        // Given / When
        var size = FlexSize.Ratio(0);

        // Then
        ((FlexSize.RatioSize)size).Weight.ShouldBe(1);
    }

    [Fact]
    public void Ratio_Negative_Should_Clamp_To_One()
    {
        // Given / When
        var size = FlexSize.Ratio(-5);

        // Then
        ((FlexSize.RatioSize)size).Weight.ShouldBe(1);
    }

    [Fact]
    public void Fill_Should_Equal_Ratio_One()
    {
        // Given / When
        var fill = FlexSize.Fill();
        var ratio = FlexSize.Ratio(1);

        // Then
        fill.ShouldBe(ratio);
    }

    [Fact]
    public void Fill_Should_Be_RatioSize_With_Weight_One()
    {
        // Given / When
        var fill = FlexSize.Fill();

        // Then
        fill.ShouldBeOfType<FlexSize.RatioSize>();
        ((FlexSize.RatioSize)fill).Weight.ShouldBe(1);
    }
}
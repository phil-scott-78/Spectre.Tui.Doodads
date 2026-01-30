using Shouldly;
using Spectre.Tui.Doodads.Doodads.Progress;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class ProgressModelTests
{
    [Fact]
    public void Default_Percent_Should_Be_Zero()
    {
        var progress = new ProgressModel();
        progress.Percent.ShouldBe(0.0);
    }

    [Fact]
    public void SetPercent_Should_Set_Target_And_Return_Command()
    {
        var progress = new ProgressModel();
        var (updated, cmd) = progress.SetPercent(0.5);

        updated.Percent.ShouldBe(0.5);
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void IncrPercent_Should_Increment()
    {
        var progress = new ProgressModel { Percent = 0.3 };
        var (updated, _) = progress.IncrPercent(0.2);

        updated.Percent.ShouldBe(0.5, 0.001);
    }

    [Fact]
    public void Percent_Should_Clamp_To_One()
    {
        var progress = new ProgressModel { Percent = 0.9 };
        var (updated, _) = progress.IncrPercent(0.5);

        updated.Percent.ShouldBeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void Init_Should_Return_Null()
    {
        var progress = new ProgressModel();
        progress.Init().ShouldBeNull();
    }

    [Fact]
    public void Default_Width_Should_Be_40()
    {
        var progress = new ProgressModel();
        progress.MinWidth.ShouldBe(40);
    }
}
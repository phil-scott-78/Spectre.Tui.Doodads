using Shouldly;
using Spectre.Tui.Doodads.Doodads.Spinner;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class SpinnerModelTests
{
    [Fact]
    public void Init_Should_Return_Command()
    {
        var spinner = new SpinnerModel();
        spinner.Init().ShouldNotBeNull();
    }

    [Fact]
    public void Update_With_Matching_Tick_Should_Advance_Frame()
    {
        var spinner = new SpinnerModel();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = spinner.Ticks.Id, Tag = spinner.Ticks.Tag };

        var (updated, cmd) = spinner.Update(tick);

        updated.Frame.ShouldBe(1);
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Update_With_Stale_Tick_Should_Not_Advance()
    {
        var spinner = new SpinnerModel();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = spinner.Ticks.Id, Tag = spinner.Ticks.Tag + 99 };

        var (updated, cmd) = spinner.Update(tick);

        updated.Frame.ShouldBe(0);
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Update_With_Wrong_Id_Should_Not_Advance()
    {
        var spinner = new SpinnerModel();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = -999, Tag = spinner.Ticks.Tag };

        var (updated, cmd) = spinner.Update(tick);

        updated.Frame.ShouldBe(0);
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Frame_Should_Wrap_Around()
    {
        var spinnerType = new SpinnerType(["a", "b"], TimeSpan.FromMilliseconds(100));
        var spinner = new SpinnerModel { Spinner = spinnerType };

        // Advance past the end
        var tick1 = new TickMessage { Time = DateTimeOffset.UtcNow, Id = spinner.Ticks.Id, Tag = spinner.Ticks.Tag };
        var (s1, _) = spinner.Update(tick1); // frame 1
        var tick2 = new TickMessage { Time = DateTimeOffset.UtcNow, Id = s1.Ticks.Id, Tag = s1.Ticks.Tag };
        var (s2, _) = s1.Update(tick2); // frame 2 => wraps to 0

        s2.Frame.ShouldBe(0);
    }

    [Fact]
    public void Default_Spinner_Should_Be_Line_Type()
    {
        var spinner = new SpinnerModel();
        spinner.Spinner.ShouldBe(SpinnerType.Line);
    }
}

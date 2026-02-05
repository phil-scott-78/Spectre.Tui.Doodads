using Shouldly;
using Spectre.Tui.Doodads.Doodads.Timer;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class TimerModelTests
{
    [Fact]
    public void Init_When_Running_Should_Return_Command()
    {
        var timer = new TimerModel
        {
            Timeout = TimeSpan.FromSeconds(10),
            Running = true,
        };

        timer.Init().ShouldNotBeNull();
    }

    [Fact]
    public void Init_When_Not_Running_Should_Return_Null()
    {
        var timer = new TimerModel { Timeout = TimeSpan.FromSeconds(10) };
        timer.Init().ShouldBeNull();
    }

    [Fact]
    public void Start_Should_Set_Running_And_Return_Command()
    {
        var timer = new TimerModel { Timeout = TimeSpan.FromSeconds(10) };
        var (started, cmd) = timer.Start();

        started.Running.ShouldBeTrue();
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Stop_Should_Set_Not_Running()
    {
        var timer = new TimerModel { Timeout = TimeSpan.FromSeconds(10), Running = true };
        var (stopped, cmd) = timer.Stop();

        stopped.Running.ShouldBeFalse();
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Toggle_Should_Start_When_Stopped()
    {
        var timer = new TimerModel { Timeout = TimeSpan.FromSeconds(10) };
        var (toggled, cmd) = timer.Toggle();

        toggled.Running.ShouldBeTrue();
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Toggle_Should_Stop_When_Running()
    {
        var timer = new TimerModel { Timeout = TimeSpan.FromSeconds(10), Running = true };
        var (toggled, cmd) = timer.Toggle();

        toggled.Running.ShouldBeFalse();
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Update_Tick_Should_Decrement_Timeout()
    {
        var timer = new TimerModel
        {
            Timeout = TimeSpan.FromSeconds(10),
            Running = true,
        };

        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = timer.Ticks.Id, Tag = timer.Ticks.Tag };
        var (updated, cmd) = timer.Update(tick);

        updated.Timeout.ShouldBe(TimeSpan.FromSeconds(9));
        cmd.ShouldNotBeNull(); // next tick scheduled
    }

    [Fact]
    public void Update_Tick_Should_Emit_Timeout_When_Done()
    {
        var timer = new TimerModel
        {
            Timeout = TimeSpan.FromSeconds(1),
            Interval = TimeSpan.FromSeconds(1),
            Running = true,
        };

        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = timer.Ticks.Id, Tag = timer.Ticks.Tag };
        var (updated, cmd) = timer.Update(tick);

        updated.Timeout.ShouldBe(TimeSpan.Zero);
        updated.Running.ShouldBeFalse();
        cmd.ShouldNotBeNull(); // should produce TimerTimeoutMessage
    }
}

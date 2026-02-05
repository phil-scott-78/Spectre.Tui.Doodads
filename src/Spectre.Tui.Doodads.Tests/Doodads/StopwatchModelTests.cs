using Shouldly;
using Spectre.Tui.Doodads.Doodads.Stopwatch;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Doodads;

public sealed class StopwatchModelTests
{
    [Fact]
    public void Init_When_Running_Should_Return_Command()
    {
        var sw = new StopwatchModel { Running = true };
        sw.Init().ShouldNotBeNull();
    }

    [Fact]
    public void Init_When_Not_Running_Should_Return_Null()
    {
        var sw = new StopwatchModel();
        sw.Init().ShouldBeNull();
    }

    [Fact]
    public void Start_Should_Set_Running()
    {
        var sw = new StopwatchModel();
        var (started, cmd) = sw.Start();

        started.Running.ShouldBeTrue();
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Stop_Should_Set_Not_Running()
    {
        var sw = new StopwatchModel { Running = true };
        var (stopped, _) = sw.Stop();

        stopped.Running.ShouldBeFalse();
    }

    [Fact]
    public void Reset_Should_Zero_Elapsed()
    {
        var sw = new StopwatchModel
        {
            Elapsed = TimeSpan.FromSeconds(42),
            Running = true,
        };

        var (reset, _) = sw.Reset();

        reset.Elapsed.ShouldBe(TimeSpan.Zero);
        reset.Running.ShouldBeFalse();
    }

    [Fact]
    public void Update_Tick_Should_Increment_Elapsed()
    {
        var sw = new StopwatchModel { Running = true };
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = sw.Ticks.Id, Tag = sw.Ticks.Tag };

        var (updated, cmd) = sw.Update(tick);

        updated.Elapsed.ShouldBe(TimeSpan.FromSeconds(1));
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Update_Stale_Tick_Should_Be_Ignored()
    {
        var sw = new StopwatchModel { Running = true };
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = sw.Ticks.Id, Tag = sw.Ticks.Tag + 99 };

        var (updated, cmd) = sw.Update(tick);

        updated.Elapsed.ShouldBe(TimeSpan.Zero);
        cmd.ShouldBeNull();
    }
}

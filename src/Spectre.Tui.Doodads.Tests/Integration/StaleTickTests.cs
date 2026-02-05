using Shouldly;
using Spectre.Tui.Doodads.Doodads.Spinner;
using Spectre.Tui.Doodads.Doodads.Stopwatch;
using Spectre.Tui.Doodads.Doodads.Timer;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests.Integration;

public sealed class StaleTickTests
{
    [Fact]
    public void Spinner_Should_Reject_Stale_Tick()
    {
        // Given - a spinner with a known Id and Tag
        var spinner = new SpinnerModel();
        var fixture = new DoodadFixture<SpinnerModel>(spinner);

        // When - send a tick with the wrong Tag
        var staleTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = spinner.Ticks.Id, Tag = spinner.Ticks.Tag + 99 };
        fixture.Send(staleTick);

        // Then - frame should not advance
        fixture.Model.Frame.ShouldBe(0);
    }

    [Fact]
    public void Spinner_Should_Accept_Valid_Tick()
    {
        // Given
        var spinner = new SpinnerModel();
        var fixture = new DoodadFixture<SpinnerModel>(spinner);

        // When - send a tick with the correct Id and Tag.
        // The fixture processes the returned tick command which may produce
        // additional ticks, so the frame may advance more than once.
        var validTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = spinner.Ticks.Id, Tag = spinner.Ticks.Tag };
        fixture.Send(validTick);

        // Then - frame should have advanced from 0
        fixture.Model.Frame.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Spinner_Should_Reject_Wrong_Id()
    {
        // Given
        var spinner = new SpinnerModel();
        var fixture = new DoodadFixture<SpinnerModel>(spinner);

        // When - send a tick with the wrong Id
        var wrongIdTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = -999, Tag = spinner.Ticks.Tag };
        fixture.Send(wrongIdTick);

        // Then - frame should not advance
        fixture.Model.Frame.ShouldBe(0);
    }

    [Fact]
    public void Stopwatch_Should_Reject_Stale_Tick()
    {
        // Given - a running stopwatch
        var (started, _) = new StopwatchModel().Start();
        var fixture = new DoodadFixture<StopwatchModel>(started);

        // When - send a tick with the wrong Tag (stale generation)
        var staleTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = started.Ticks.Id, Tag = started.Ticks.Tag + 99 };
        fixture.Send(staleTick);

        // Then - elapsed should not change
        fixture.Model.Elapsed.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Stopwatch_Should_Accept_Valid_Tick()
    {
        // Given - a running stopwatch
        var (started, _) = new StopwatchModel().Start();
        var fixture = new DoodadFixture<StopwatchModel>(started);

        // When - send a tick with the correct Id and Tag.
        // The fixture processes the returned tick command which may produce
        // additional ticks, so elapsed may increment more than once.
        var validTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = started.Ticks.Id, Tag = started.Ticks.Tag };
        fixture.Send(validTick);

        // Then - elapsed should have increased from zero
        fixture.Model.Elapsed.ShouldBeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void Stopwatch_Should_Reject_Tick_When_Stopped()
    {
        // Given - a stopped stopwatch (with a known Id/Tag from a previous start)
        var (started, _) = new StopwatchModel().Start();
        var savedId = started.Ticks.Id;
        var savedTag = started.Ticks.Tag;
        var (stopped, _) = started.Stop();
        var fixture = new DoodadFixture<StopwatchModel>(stopped);

        // When - send a tick that matches the old running state
        var oldTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = savedId, Tag = savedTag };
        fixture.Send(oldTick);

        // Then - elapsed should not change because Running is false and Tag changed on Stop
        fixture.Model.Elapsed.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Timer_Should_Reject_Stale_Tick()
    {
        // Given - a running timer
        var (started, _) = new TimerModel { Timeout = TimeSpan.FromSeconds(10) }.Start();
        var fixture = new DoodadFixture<TimerModel>(started);

        // When - send a tick with the wrong Tag (stale generation)
        var staleTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = started.Ticks.Id, Tag = started.Ticks.Tag + 99 };
        fixture.Send(staleTick);

        // Then - timeout should not change
        fixture.Model.Timeout.ShouldBe(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Timer_Should_Accept_Valid_Tick()
    {
        // Given - a running timer with 10 seconds remaining
        var (started, _) = new TimerModel { Timeout = TimeSpan.FromSeconds(10) }.Start();
        var fixture = new DoodadFixture<TimerModel>(started);

        // When - send a valid tick.
        // The fixture processes the returned tick command which may produce
        // additional ticks, so timeout may decrement more than once.
        var validTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = started.Ticks.Id, Tag = started.Ticks.Tag };
        fixture.Send(validTick);

        // Then - timeout should have decreased from the original 10 seconds
        fixture.Model.Timeout.ShouldBeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Timer_Should_Reject_Tick_When_Stopped()
    {
        // Given - a timer that was started then stopped
        var (started, _) = new TimerModel { Timeout = TimeSpan.FromSeconds(10) }.Start();
        var savedId = started.Ticks.Id;
        var savedTag = started.Ticks.Tag;
        var (stopped, _) = started.Stop();
        var fixture = new DoodadFixture<TimerModel>(stopped);

        // When - send a tick that matches the old running state
        var oldTick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = savedId, Tag = savedTag };
        fixture.Send(oldTick);

        // Then - timeout should not change because Running is false and Tag changed on Stop
        fixture.Model.Timeout.ShouldBe(TimeSpan.FromSeconds(10));
    }
}

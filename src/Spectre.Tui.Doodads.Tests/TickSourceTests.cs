using Shouldly;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests;

public sealed class TickSourceTests
{
    [Fact]
    public void New_TickSources_Should_Have_Unique_Ids()
    {
        var a = new TickSource();
        var b = new TickSource();

        a.Id.ShouldNotBe(b.Id);
    }

    [Fact]
    public void New_TickSource_Should_Start_With_Tag_Zero()
    {
        var source = new TickSource();

        source.Tag.ShouldBe(0);
    }

    [Fact]
    public void Advance_Should_Increment_Tag()
    {
        var source = new TickSource();
        var advanced = source.Advance();

        advanced.Tag.ShouldBe(source.Tag + 1);
    }

    [Fact]
    public void Advance_Should_Preserve_Id()
    {
        var source = new TickSource();
        var advanced = source.Advance();

        advanced.Id.ShouldBe(source.Id);
    }

    [Fact]
    public void IsValid_Should_Accept_Matching_Tick()
    {
        var source = new TickSource();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = source.Id, Tag = source.Tag };

        source.IsValid(tick).ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Should_Reject_Wrong_Tag()
    {
        var source = new TickSource();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = source.Id, Tag = source.Tag + 99 };

        source.IsValid(tick).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_Should_Reject_Wrong_Id()
    {
        var source = new TickSource();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = -999, Tag = source.Tag };

        source.IsValid(tick).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_After_Advance_Should_Reject_Old_Tag()
    {
        var source = new TickSource();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = source.Id, Tag = source.Tag };
        var advanced = source.Advance();

        advanced.IsValid(tick).ShouldBeFalse();
    }

    [Fact]
    public void IsValid_With_Kind_Should_Accept_Matching()
    {
        var source = new TickSource();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = source.Id, Tag = source.Tag, Kind = "blink" };

        source.IsValid(tick, "blink").ShouldBeTrue();
    }

    [Fact]
    public void IsValid_With_Kind_Should_Reject_Wrong_Kind()
    {
        var source = new TickSource();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = source.Id, Tag = source.Tag, Kind = "idle" };

        source.IsValid(tick, "blink").ShouldBeFalse();
    }

    [Fact]
    public void IsValid_With_Kind_Should_Reject_Null_Kind()
    {
        var source = new TickSource();
        var tick = new TickMessage { Time = DateTimeOffset.UtcNow, Id = source.Id, Tag = source.Tag };

        source.IsValid(tick, "blink").ShouldBeFalse();
    }
}

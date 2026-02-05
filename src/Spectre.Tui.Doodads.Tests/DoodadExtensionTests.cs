using Shouldly;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Spectre.Tui.Doodads.Tests;

/// <summary>
/// A simple child doodad that increments a counter on any message.
/// Returns no command.
/// </summary>
internal record CounterChild : IDoodad<CounterChild>
{
    public int Count { get; init; }

    public Command? Init() => null;

    public (CounterChild Model, Command? Command) Update(Message message)
    {
        return (this with { Count = Count + 1 }, null);
    }

    public void View(IRenderSurface surface) { }
}

/// <summary>
/// A child doodad that returns a command from Update.
/// </summary>
internal record CommandChild : IDoodad<CommandChild>
{
    public int Value { get; init; }

    public Command? Init() => null;

    public (CommandChild Model, Command? Command) Update(Message message)
    {
        return (this with { Value = Value + 1 }, Commands.Message(new QuitMessage()));
    }

    public void View(IRenderSurface surface) { }
}

/// <summary>
/// A composite parent with multiple children of mixed types.
/// </summary>
internal record ParentDoodad : IDoodad<ParentDoodad>
{
    public CounterChild Counter { get; init; } = new();
    public CommandChild Commander { get; init; } = new();
    public CounterChild Counter2 { get; init; } = new();

    public Command? Init() => null;

    public (ParentDoodad Model, Command? Command) Update(Message message)
    {
        return (this, null);
    }

    public void View(IRenderSurface surface) { }
}

public sealed class DoodadExtensionTests
{
    private static readonly Message TestMessage = new QuitMessage();

    [Fact]
    public void Forward_Single_Child_Should_Update_State()
    {
        // Given
        var parent = new ParentDoodad();

        // When
        var (model, cmd) = parent.Forward(
            TestMessage,
            m => m.Counter,
            (m, v) => m with { Counter = v });

        // Then
        model.Counter.Count.ShouldBe(1);
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Forward_Single_Child_Should_Return_Command()
    {
        // Given
        var parent = new ParentDoodad();

        // When
        var (model, cmd) = parent.Forward(
            TestMessage,
            m => m.Commander,
            (m, v) => m with { Commander = v });

        // Then
        model.Commander.Value.ShouldBe(1);
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public void Forward_Chain_Should_Update_All_Children()
    {
        // Given
        var parent = new ParentDoodad();

        // When
        var (model, _) = parent
            .Forward(TestMessage, m => m.Counter, (m, v) => m with { Counter = v })
            .Forward(TestMessage, m => m.Commander, (m, v) => m with { Commander = v })
            .Forward(TestMessage, m => m.Counter2, (m, v) => m with { Counter2 = v });

        // Then
        model.Counter.Count.ShouldBe(1);
        model.Commander.Value.ShouldBe(1);
        model.Counter2.Count.ShouldBe(1);
    }

    [Fact]
    public void Forward_Chain_All_Null_Commands_Should_Return_Null()
    {
        // Given
        var parent = new ParentDoodad();

        // When — both CounterChild children return null commands
        var (_, cmd) = parent
            .Forward(TestMessage, m => m.Counter, (m, v) => m with { Counter = v })
            .Forward(TestMessage, m => m.Counter2, (m, v) => m with { Counter2 = v });

        // Then
        cmd.ShouldBeNull();
    }

    [Fact]
    public void Forward_Chain_Mixed_Commands_Should_Batch()
    {
        // Given
        var parent = new ParentDoodad();

        // When — Counter returns null, Commander returns a command
        var (_, cmd) = parent
            .Forward(TestMessage, m => m.Counter, (m, v) => m with { Counter = v })
            .Forward(TestMessage, m => m.Commander, (m, v) => m with { Commander = v });

        // Then — Commands.Batch with one non-null unwraps to the single command
        cmd.ShouldNotBeNull();
    }

    [Fact]
    public async Task Forward_Chain_Multiple_Commands_Should_Batch()
    {
        // Given
        var parent = new ParentDoodad
        {
            Commander = new CommandChild(),
        };

        // Build a second parent with two commanding children by using Commander for both slots
        var parent2 = new TwoCommandParent();

        // When
        var (_, cmd) = parent2
            .Forward(TestMessage, m => m.A, (m, v) => m with { A = v })
            .Forward(TestMessage, m => m.B, (m, v) => m with { B = v });

        // Then — both children produce commands, so we get a BatchMessage
        cmd.ShouldNotBeNull();
        var result = await cmd(CancellationToken.None);
        result.ShouldBeOfType<BatchMessage>();
    }

    [Fact]
    public void Forward_Chain_Should_Preserve_Independent_State()
    {
        // Given — Counter starts at 5, Counter2 starts at 10
        var parent = new ParentDoodad
        {
            Counter = new CounterChild { Count = 5 },
            Counter2 = new CounterChild { Count = 10 },
        };

        // When
        var (model, _) = parent
            .Forward(TestMessage, m => m.Counter, (m, v) => m with { Counter = v })
            .Forward(TestMessage, m => m.Counter2, (m, v) => m with { Counter2 = v });

        // Then — each child incremented independently
        model.Counter.Count.ShouldBe(6);
        model.Counter2.Count.ShouldBe(11);
    }
}

/// <summary>
/// A parent with two command-producing children for testing batch behavior.
/// </summary>
internal record TwoCommandParent : IDoodad<TwoCommandParent>
{
    public CommandChild A { get; init; } = new();
    public CommandChild B { get; init; } = new();

    public Command? Init() => null;

    public (TwoCommandParent Model, Command? Command) Update(Message message)
    {
        return (this, null);
    }

    public void View(IRenderSurface surface) { }
}

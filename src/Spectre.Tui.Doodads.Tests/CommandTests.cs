using Shouldly;
using Spectre.Tui.Doodads.Messages;

namespace Spectre.Tui.Doodads.Tests;

public sealed class CommandTests
{
    [Fact]
    public async Task Message_Should_Return_Provided_Message()
    {
        // Given
        var expected = new QuitMessage();
        var cmd = Commands.Message(expected);

        // When
        var result = await cmd(CancellationToken.None);

        // Then
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Quit_Should_Return_QuitMessage()
    {
        // Given
        var cmd = Commands.Quit();

        // When
        var result = await cmd(CancellationToken.None);

        // Then
        result.ShouldBeOfType<QuitMessage>();
    }

    [Fact]
    public void Batch_With_No_Commands_Should_Return_Null()
    {
        // When
        var result = Commands.Batch();

        // Then
        result.ShouldBeNull();
    }

    [Fact]
    public void Batch_With_Only_Null_Commands_Should_Return_Null()
    {
        // When
        var result = Commands.Batch(null, null);

        // Then
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Batch_With_Single_Command_Should_Return_Its_Message()
    {
        // Given
        var expected = new QuitMessage();
        var cmd = Commands.Batch(Commands.Message(expected));

        // When
        var result = await cmd!(CancellationToken.None);

        // Then
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Batch_With_Multiple_Commands_Should_Return_BatchMessage()
    {
        // Given
        var msg1 = new WindowSizeMessage { Width = 80, Height = 25 };
        var msg2 = new QuitMessage();
        var cmd = Commands.Batch(Commands.Message(msg1), Commands.Message(msg2));

        // When
        var result = await cmd!(CancellationToken.None);

        // Then
        var batch = result.ShouldBeOfType<BatchMessage>();
        batch.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Sequence_With_No_Commands_Should_Return_Null()
    {
        // When
        var result = Commands.Sequence();

        // Then
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Sequence_With_Multiple_Commands_Should_Return_SequenceMessage()
    {
        // Given
        var msg1 = new QuitMessage();
        var msg2 = new QuitMessage();
        var cmd = Commands.Sequence(Commands.Message(msg1), Commands.Message(msg2));

        // When
        var result = await cmd!(CancellationToken.None);

        // Then
        var seq = result.ShouldBeOfType<SequenceMessage>();
        seq.StepMessage.ShouldBe(msg1);
        seq.Remaining.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Tick_Should_Wait_And_Produce_Message()
    {
        // Given
        var cmd = Commands.Tick(
            TimeSpan.FromMilliseconds(10),
            time => new TickMessage { Time = time, Id = 1, Tag = 0 });

        // When
        var result = await cmd(CancellationToken.None);

        // Then
        result.ShouldBeOfType<TickMessage>();
    }

    [Fact]
    public async Task Batch_With_Failing_Command_Should_Produce_CommandErrorMessage()
    {
        // Given
        var expected = new QuitMessage();
        Command failing = _ => throw new InvalidOperationException("test error");
        var cmd = Commands.Batch(Commands.Message(expected), failing);

        // When
        var result = await cmd!(CancellationToken.None);

        // Then
        var batch = result.ShouldBeOfType<BatchMessage>();
        batch.Messages.Count.ShouldBe(2);
        batch.Messages[0].ShouldBe(expected);
        var error = batch.Messages[1].ShouldBeOfType<CommandErrorMessage>();
        error.Exception.ShouldBeOfType<InvalidOperationException>();
        error.Exception.Message.ShouldBe("test error");
    }

    [Fact]
    public async Task Sequence_With_Failing_First_Step_Should_Produce_SequenceMessage_With_Error()
    {
        // Given
        Command failing = _ => throw new InvalidOperationException("step error");
        var msg2 = new QuitMessage();
        var cmd = Commands.Sequence(failing, Commands.Message(msg2));

        // When
        var result = await cmd!(CancellationToken.None);

        // Then
        var seq = result.ShouldBeOfType<SequenceMessage>();
        var error = seq.StepMessage.ShouldBeOfType<CommandErrorMessage>();
        error.Exception.ShouldBeOfType<InvalidOperationException>();
        error.Exception.Message.ShouldBe("step error");
        seq.Remaining.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Batch_With_All_Failing_Commands_Should_Produce_Errors()
    {
        // Given
        Command failing1 = _ => throw new InvalidOperationException("error 1");
        Command failing2 = _ => throw new ArgumentException("error 2");
        var cmd = Commands.Batch(failing1, failing2);

        // When
        var result = await cmd!(CancellationToken.None);

        // Then
        var batch = result.ShouldBeOfType<BatchMessage>();
        batch.Messages.Count.ShouldBe(2);
        batch.Messages.ShouldAllBe(m => m is CommandErrorMessage);
    }
}
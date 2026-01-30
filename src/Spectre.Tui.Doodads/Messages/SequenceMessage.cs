namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// Internal message for sequenced command execution.
/// </summary>
internal record SequenceMessage : Message
{
    /// <summary>
    /// Gets the message from the current step.
    /// </summary>
    public required Message? StepMessage { get; init; }

    /// <summary>
    /// Gets the remaining commands to execute.
    /// </summary>
    public required IReadOnlyList<Command> Remaining { get; init; }
}
namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// Internal message containing results from batched commands.
/// </summary>
internal record BatchMessage : Message
{
    /// <summary>
    /// Gets the messages produced by the batch.
    /// </summary>
    public required IReadOnlyList<Message> Messages { get; init; }
}
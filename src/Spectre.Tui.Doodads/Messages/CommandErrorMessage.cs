namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// Produced when a command throws an unhandled exception.
/// </summary>
public record CommandErrorMessage : Message
{
    /// <summary>
    /// Gets the exception that caused the error.
    /// </summary>
    public required Exception Exception { get; init; }
}
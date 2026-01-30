namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// Internal message used to auto-dismiss status bar messages after a timeout.
/// </summary>
internal record ListStatusMessage : Message
{
    /// <summary>
    /// Gets the unique identifier of the status message to dismiss.
    /// </summary>
    public required int Id { get; init; }
}
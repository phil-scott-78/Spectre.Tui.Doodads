namespace Spectre.Tui.Doodads.Input;

/// <summary>
/// Reads terminal input and converts it to messages.
/// </summary>
public interface IInputReader : IDisposable
{
    /// <summary>
    /// Reads the next input message asynchronously.
    /// </summary>
    ValueTask<Message?> ReadAsync(CancellationToken cancellationToken);
}
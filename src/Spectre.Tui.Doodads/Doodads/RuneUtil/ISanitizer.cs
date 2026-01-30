namespace Spectre.Tui.Doodads.Doodads.RuneUtil;

/// <summary>
/// Interface for sanitizing rune sequences by removing or replacing unwanted characters.
/// </summary>
public interface ISanitizer
{
    /// <summary>
    /// Sanitizes the given runes, returning a cleaned sequence.
    /// </summary>
    /// <param name="runes">The runes to sanitize.</param>
    /// <returns>The sanitized runes.</returns>
    IReadOnlyList<Rune> Sanitize(IReadOnlyList<Rune> runes);
}
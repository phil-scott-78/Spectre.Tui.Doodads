namespace Spectre.Tui.Doodads.Doodads.RuneUtil;

/// <summary>
/// Default rune sanitizer that removes control characters and optionally replaces tabs and newlines.
/// </summary>
public sealed class RuneSanitizer : ISanitizer
{
    /// <summary>
    /// Gets or sets the replacement string for tab characters. Null means tabs are removed.
    /// </summary>
    public string? TabReplacement { get; init; } = "    ";

    /// <summary>
    /// Gets or sets the replacement string for newline characters. Null means newlines are removed.
    /// </summary>
    public string? NewlineReplacement { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<Rune> Sanitize(IReadOnlyList<Rune> runes)
    {
        var result = new List<Rune>(runes.Count);

        foreach (var rune in runes)
        {
            if (rune == new Rune('\t'))
            {
                if (TabReplacement is not null)
                {
                    foreach (var r in TabReplacement.EnumerateRunes())
                    {
                        result.Add(r);
                    }
                }

                continue;
            }

            if (rune == new Rune('\n') || rune == new Rune('\r'))
            {
                if (NewlineReplacement is not null)
                {
                    foreach (var r in NewlineReplacement.EnumerateRunes())
                    {
                        result.Add(r);
                    }
                }

                continue;
            }

            if (Rune.IsControl(rune))
            {
                continue;
            }

            result.Add(rune);
        }

        return result;
    }
}
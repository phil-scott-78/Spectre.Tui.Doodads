namespace Spectre.Tui.Doodads.Doodads.RuneUtil;

/// <summary>
/// Unicode-aware width calculation utilities for terminal display.
/// </summary>
public static class UnicodeWidth
{
    /// <summary>
    /// Gets the display width of a single rune using Wcwidth.
    /// Control characters and other -1 results are treated as zero width.
    /// </summary>
    public static int GetWidth(Rune rune)
    {
        return Wcwidth.UnicodeCalculator.GetWidth(rune) switch
        {
            -1 => 0,
            var w => w,
        };
    }

    /// <summary>
    /// Gets the total display width of a string by summing the width of each rune.
    /// </summary>
    public static int GetWidth(string text)
    {
        var width = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            width += GetWidth(rune);
        }

        return width;
    }

    /// <summary>
    /// Gets the display width of a single rune, treating zero-width runes as width 1.
    /// Useful when each character must occupy at least one terminal column.
    /// </summary>
    public static int GetDisplayWidth(Rune rune)
    {
        return Math.Max(1, GetWidth(rune));
    }

    /// <summary>
    /// Gets the display width of a string, treating each rune as at least width 1.
    /// </summary>
    public static int GetDisplayWidth(string text)
    {
        var width = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            width += GetDisplayWidth(rune);
        }

        return width;
    }

    /// <summary>
    /// Truncates a string to fit within the specified display width,
    /// treating each rune as at least width 1.
    /// </summary>
    public static string TruncateToWidth(string text, int maxWidth)
    {
        var sb = new StringBuilder();
        var width = 0;

        foreach (var rune in text.EnumerateRunes())
        {
            var runeWidth = GetDisplayWidth(rune);

            if (width + runeWidth > maxWidth)
            {
                break;
            }

            sb.Append(rune);
            width += runeWidth;
        }

        return sb.ToString();
    }
}

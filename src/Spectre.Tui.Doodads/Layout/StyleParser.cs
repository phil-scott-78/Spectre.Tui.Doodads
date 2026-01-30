namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// Parses Tailwind-like format strings into <see cref="Appearance"/> values.
/// </summary>
/// <remarks>
/// Supported tokens:
/// <list type="bullet">
///   <item><description>Decorations: <c>bold</c>, <c>dim</c>, <c>italic</c>, <c>underline</c>, <c>invert</c>, <c>strikethrough</c></description></item>
///   <item><description>Foreground: <c>text-red</c>, <c>text-#ff8800</c></description></item>
///   <item><description>Background: <c>bg-blue</c>, <c>bg-#001122</c></description></item>
/// </list>
/// Multiple tokens are space-separated: <c>"bold text-red bg-blue"</c>.
/// Unknown tokens are silently ignored.
/// </remarks>
internal static class StyleParser
{
    internal static Appearance Parse(ReadOnlySpan<char> format)
    {
        var decoration = Decoration.None;
        Color? foreground = null;
        Color? background = null;

        foreach (var range in format.Split(' '))
        {
            var token = format[range];
            if (token.IsEmpty)
            {
                continue;
            }

            if (token.StartsWith("text-"))
            {
                foreground = ParseColor(token[5..]) ?? foreground;
            }
            else if (token.StartsWith("bg-"))
            {
                background = ParseColor(token[3..]) ?? background;
            }
            else
            {
                decoration |= ParseDecoration(token);
            }
        }

        var result = Appearance.Plain;
        if (decoration != Decoration.None)
        {
            result = result with { Decoration = decoration };
        }

        if (foreground.HasValue)
        {
            result = result with { Foreground = foreground.Value };
        }

        if (background.HasValue)
        {
            result = result with { Background = background.Value };
        }

        return result;
    }

    private static Decoration ParseDecoration(ReadOnlySpan<char> token) => token switch
    {
        "bold" => Decoration.Bold,
        "dim" => Decoration.Dim,
        "italic" => Decoration.Italic,
        "underline" => Decoration.Underline,
        "invert" => Decoration.Invert,
        "strikethrough" => Decoration.Strikethrough,
        _ => Decoration.None,
    };

    private static Color? ParseColor(ReadOnlySpan<char> name)
    {
        if (name.Length == 7 && name[0] == '#')
        {
            if (byte.TryParse(name.Slice(1, 2), NumberStyles.HexNumber, null, out var r)
                && byte.TryParse(name.Slice(3, 2), NumberStyles.HexNumber, null, out var g)
                && byte.TryParse(name.Slice(5, 2), NumberStyles.HexNumber, null, out var b))
            {
                return new Color(r, g, b);
            }

            return null;
        }

        return name switch
        {
            "black" => Color.Black,
            "red" => Color.Red,
            "green" => Color.Green,
            "yellow" => Color.Yellow,
            "blue" => Color.Blue,
            "magenta" => Color.Magenta1,
            "cyan" => Color.Cyan1,
            "white" => Color.White,
            "grey" or "gray" => Color.Grey,
            _ => null,
        };
    }
}
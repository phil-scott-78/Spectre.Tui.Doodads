using Spectre.Console;
using Spectre.Tui;

namespace Sandbox.AspNet;

internal static class LineColorizers
{
    public static IReadOnlyList<TextSegment> ColorizeRouteLine(string line)
    {
        // Format: "{method,-8} {path}"
        // The method field is 8 chars wide (left-aligned, space-padded).
        if (line.Length < 9)
        {
            return [new TextSegment(line, Appearance.Plain)];
        }

        var methodField = line[..8];
        var method = methodField.TrimEnd();
        var rest = line[8..]; // starts with space + path

        var methodStyle = method.ToUpperInvariant() switch
        {
            "GET" => new Appearance { Foreground = Color.Green },
            "POST" => new Appearance { Foreground = Color.Yellow },
            "PUT" => new Appearance { Foreground = Color.CornflowerBlue },
            "DELETE" => new Appearance { Foreground = Color.Red },
            "PATCH" => new Appearance { Foreground = Color.Magenta1 },
            _ => new Appearance { Decoration = Decoration.Dim },
        };

        return [new TextSegment(methodField, methodStyle), new TextSegment(rest, Appearance.Plain)];
    }

    public static IReadOnlyList<TextSegment> ColorizeServiceLine(string line)
    {
        // Format: "{lifetime}  {serviceType} -> {implementation}"
        // Lifetime is one of: "Singleton", "Scoped   ", "Transient" (9 chars).
        var arrowIndex = line.IndexOf(" -> ", StringComparison.Ordinal);
        if (arrowIndex < 0 || line.Length < 11)
        {
            return [new TextSegment(line, Appearance.Plain)];
        }

        // Lifetime field is the first 9 characters.
        var lifetimeField = line[..9];
        var lifetime = lifetimeField.TrimEnd();

        var lifetimeStyle = lifetime switch
        {
            "Singleton" => new Appearance { Foreground = Color.Cyan1 },
            "Scoped" => new Appearance { Foreground = Color.Yellow },
            "Transient" => new Appearance { Foreground = Color.Green },
            _ => Appearance.Plain,
        };

        // Two spaces after lifetime field.
        var separator = line[9..11]; // "  "
        var serviceType = line[11..arrowIndex];
        var arrow = " -> ";
        var implementation = line[(arrowIndex + 4)..];

        var dimStyle = new Appearance { Decoration = Decoration.Dim };

        return
        [
            new TextSegment(lifetimeField, lifetimeStyle),
            new TextSegment(separator, Appearance.Plain),
            new TextSegment(serviceType, Appearance.Plain),
            new TextSegment(arrow, dimStyle),
            new TextSegment(implementation, dimStyle),
        ];
    }

    public static IReadOnlyList<TextSegment> ColorizeConfigLine(string line)
    {
        // Config debug view format:
        //   SectionName:          (section header — ends with ':' after trimming)
        //     Key=Value (Provider) (key-value line)

        var trimmed = line.TrimEnd();

        // Section header: line ends with ':' after trimming.
        if (trimmed.Length > 0 && trimmed[^1] == ':')
        {
            return [new TextSegment(line, new Appearance { Decoration = Decoration.Bold })];
        }

        // Key=Value line: look for '=' sign.
        var eqIndex = line.IndexOf('=');
        if (eqIndex < 0)
        {
            return [new TextSegment(line, Appearance.Plain)];
        }

        var key = line[..eqIndex];
        var dimStyle = new Appearance { Decoration = Decoration.Dim };

        // Check for provider suffix: " (ProviderName)" at end.
        var providerStart = line.LastIndexOf(" (", StringComparison.Ordinal);
        if (providerStart > eqIndex && trimmed[^1] == ')')
        {
            var value = line[(eqIndex + 1)..providerStart];
            var provider = line[providerStart..];
            return
            [
                new TextSegment(key, new Appearance { Foreground = Color.Cyan1 }),
                new TextSegment("=", dimStyle),
                new TextSegment(value, Appearance.Plain),
                new TextSegment(provider, dimStyle),
            ];
        }

        // No provider suffix: just key=value.
        var valueOnly = line[(eqIndex + 1)..];
        return
        [
            new TextSegment(key, new Appearance { Foreground = Color.Cyan1 }),
            new TextSegment("=", dimStyle),
            new TextSegment(valueOnly, Appearance.Plain),
        ];
    }
}
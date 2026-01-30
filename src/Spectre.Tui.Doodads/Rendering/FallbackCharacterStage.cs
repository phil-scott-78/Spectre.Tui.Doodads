using System.Collections.Frozen;
using Spectre.Tui.Doodads.Layout;

namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// A render pipeline stage that replaces box-drawing characters with ASCII equivalents.
/// Useful for terminals that don't support Unicode box-drawing glyphs.
/// </summary>
public sealed class FallbackCharacterStage : RenderPipelineStage
{
    private static readonly FrozenDictionary<char, char> _mappings = BuildMappings();

    /// <summary>
    /// Initializes a new instance of the <see cref="FallbackCharacterStage"/> class.
    /// </summary>
    /// <param name="inner">The inner render surface to delegate to.</param>
    public FallbackCharacterStage(IRenderSurface inner)
        : base(inner)
    {
    }

    /// <inheritdoc />
    public override Position SetString(int x, int y, string text, Appearance style)
    {
        return base.SetString(x, y, ReplaceBoxDrawing(text), style);
    }

    private static string ReplaceBoxDrawing(string text)
    {
        return string.Create(text.Length, text, static (span, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                span[i] = MapCharacter(source[i]);
            }
        });
    }

    private static char MapCharacter(char c)
    {
        return _mappings.TryGetValue(c, out var mapped) ? mapped : c;
    }

    private static FrozenDictionary<char, char> BuildMappings()
    {
        var ascii = BorderStyle.Ascii;
        var asciiTop = ascii.Top[0];
        var asciiBottom = ascii.Bottom[0];
        var asciiLeft = ascii.Left[0];
        var asciiRight = ascii.Right[0];
        var asciiTopLeft = ascii.TopLeft[0];
        var asciiTopRight = ascii.TopRight[0];
        var asciiBottomLeft = ascii.BottomLeft[0];
        var asciiBottomRight = ascii.BottomRight[0];

        var dict = new Dictionary<char, char>();

        foreach (var style in BorderStyle.AllStyles)
        {
            TryAdd(dict, style.Top[0], asciiTop);
            TryAdd(dict, style.Bottom[0], asciiBottom);
            TryAdd(dict, style.Left[0], asciiLeft);
            TryAdd(dict, style.Right[0], asciiRight);
            TryAdd(dict, style.TopLeft[0], asciiTopLeft);
            TryAdd(dict, style.TopRight[0], asciiTopRight);
            TryAdd(dict, style.BottomLeft[0], asciiBottomLeft);
            TryAdd(dict, style.BottomRight[0], asciiBottomRight);
        }

        // T-junctions and crosses used in table rendering (not in BorderStyle)
        dict.TryAdd('\u252C', '+'); // ┬
        dict.TryAdd('\u2534', '+'); // ┴
        dict.TryAdd('\u251C', '+'); // ├
        dict.TryAdd('\u2524', '+'); // ┤
        dict.TryAdd('\u253C', '+'); // ┼

        return dict.ToFrozenDictionary();
    }

    private static void TryAdd(Dictionary<char, char> dict, char source, char target)
    {
        // Skip characters that are already ASCII or spaces
        if (source is <= '\u007F' or ' ')
        {
            return;
        }

        dict.TryAdd(source, target);
    }
}

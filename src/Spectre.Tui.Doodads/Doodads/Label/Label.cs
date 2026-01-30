using Spectre.Tui.Doodads.Layout;

namespace Spectre.Tui.Doodads.Doodads.Label;

/// <summary>
/// A styled text widget that bundles text and appearance together.
/// Implements <see cref="ISizedRenderable"/> so it can be used inside
/// <see cref="RenderSurfaceExtensions.Layout"/> templates.
/// </summary>
public record Label : ISizedRenderable
{
    /// <summary>
    /// Gets the text content of the label.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Gets the visual style applied to the text.
    /// </summary>
    public Appearance Style { get; init; }

    /// <summary>
    /// Gets the minimum display width in terminal columns, computed from <see cref="Text"/>
    /// using Unicode character widths.
    /// </summary>
    public int MinWidth => UnicodeWidth(Text);

    /// <summary>
    /// Gets the minimum height of the label (always 1).
    /// </summary>
    public int MinHeight => 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> record
    /// with the specified text and <see cref="Appearance"/> style.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="style">The visual appearance.</param>
    public Label(string text, Appearance style)
    {
        Text = text;
        Style = style;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Label"/> record
    /// with the specified text and a Tailwind-like style format string.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="styleFormat">
    /// A space-separated style string (e.g. <c>"bold text-red"</c>, <c>"dim"</c>,
    /// <c>"text-#ff0000 bg-blue"</c>). Parsed by <see cref="Layout.StyleParser"/>.
    /// </param>
    public Label(string text, string styleFormat)
    {
        Text = text;
        Style = StyleParser.Parse(styleFormat);
    }

    /// <inheritdoc />
    public void Render(IRenderSurface surface)
    {
        surface.SetString(0, 0, Text, Style);
    }

    private static int UnicodeWidth(string text)
    {
        var width = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            width += Wcwidth.UnicodeCalculator.GetWidth(rune) switch
            {
                -1 => 0,
                var w => w,
            };
        }

        return width;
    }
}

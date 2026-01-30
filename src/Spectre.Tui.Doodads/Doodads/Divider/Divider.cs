namespace Spectre.Tui.Doodads.Doodads.Divider;

/// <summary>
/// A horizontal divider widget that fills its available width with a
/// repeated character. Implements <see cref="ISizedRenderable"/> so it can
/// be used inside flex layouts and layout templates.
/// </summary>
public record Divider : ISizedRenderable
{
    /// <summary>
    /// Gets the character used to draw the divider line.
    /// Defaults to U+2500 (box-drawing horizontal).
    /// </summary>
    public char Character { get; init; } = '\u2500';

    /// <summary>
    /// Gets the visual style applied to the divider line.
    /// </summary>
    public Appearance Style { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the minimum display width in terminal columns (always 1).
    /// </summary>
    public int MinWidth => 1;

    /// <summary>
    /// Gets the minimum height of the divider (always 1).
    /// </summary>
    public int MinHeight => 1;

    /// <summary>
    /// Given available space, returns the desired size: fills available
    /// width and uses exactly 1 row of height.
    /// </summary>
    /// <param name="availableSize">The available space offered by the layout system.</param>
    /// <returns>The desired size for this widget.</returns>
    public Size Measure(Size availableSize) => new(availableSize.Width, 1);

    /// <inheritdoc />
    public void Render(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        if (width > 0)
        {
            surface.SetString(0, 0, new string(Character, width), Style);
        }
    }
}

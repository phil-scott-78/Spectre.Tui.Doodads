namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// Abstraction over <see cref="RenderContext"/> that supports middleware pipelines.
/// All rendering goes through this interface so pipeline stages can intercept
/// and transform individual operations.
/// </summary>
public interface IRenderSurface
{
    /// <summary>
    /// Gets the viewport bounds for this surface.
    /// </summary>
    Rectangle Viewport { get; }

    /// <summary>
    /// Writes a string at the given position with the specified appearance.
    /// Returns the position immediately after the last character written.
    /// </summary>
    /// <param name="x">The column position.</param>
    /// <param name="y">The row position.</param>
    /// <param name="text">The text to write.</param>
    /// <param name="style">The visual appearance.</param>
    /// <returns>The position after the last character written.</returns>
    Position SetString(int x, int y, string text, Appearance style);

    /// <summary>
    /// Renders a child <see cref="IRenderable"/> within the specified bounds.
    /// The surface constrains the child's viewport and rebuilds the pipeline
    /// for the nested rendering context.
    /// </summary>
    /// <param name="renderable">The renderable to draw.</param>
    /// <param name="bounds">The region to render into.</param>
    void Render(IRenderable renderable, Rectangle bounds);
}

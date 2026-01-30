namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// A renderable component that can draw itself to an <see cref="IRenderSurface"/>.
/// </summary>
public interface IRenderable
{
    /// <summary>
    /// Renders this component to the given surface.
    /// </summary>
    /// <param name="surface">The render surface to draw into.</param>
    void Render(IRenderSurface surface);
}

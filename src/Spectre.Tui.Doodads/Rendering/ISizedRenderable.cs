namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// An <see cref="IRenderable"/> that declares its minimum intrinsic dimensions.
/// The layout system uses these values to allocate space in templates.
/// Flex containers and other adaptive layouts may provide more space
/// via <c>surface.Viewport</c>.
/// </summary>
public interface ISizedRenderable : IRenderable
{
    /// <summary>
    /// Gets the minimum width of the renderable in columns.
    /// </summary>
    int MinWidth { get; }

    /// <summary>
    /// Gets the minimum height of the renderable in rows.
    /// </summary>
    int MinHeight { get; }

    /// <summary>
    /// Given available space, returns the desired size for this renderable.
    /// The default returns (<see cref="MinWidth"/>, <see cref="MinHeight"/>).
    /// </summary>
    /// <param name="availableSize">The available space offered by the layout system.</param>
    /// <returns>The desired size for this renderable.</returns>
    Size Measure(Size availableSize) => new(MinWidth, MinHeight);
}

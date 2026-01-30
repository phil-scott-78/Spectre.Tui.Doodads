using Spectre.Tui.Doodads.Layout;

namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// Extension methods for <see cref="IRenderSurface"/>.
/// </summary>
public static class RenderSurfaceExtensions
{
    /// <summary>
    /// Renders an interpolated string layout template to the render surface.
    /// Literal text defines the visual structure, and interpolation holes place
    /// styled content or widgets at the corresponding positions.
    /// </summary>
    /// <remarks>
    /// <para>Usage example:</para>
    /// <code>
    /// surface.Layout($"""
    ///     {Title:bold text-cyan}
    ///
    ///     {ProgressBar}
    ///
    ///     Items: {Count}  Status: {Status:dim}
    ///     """);
    /// </code>
    /// </remarks>
    /// <param name="surface">The render surface to draw into.</param>
    /// <param name="handler">
    /// The interpolated string handler. Created automatically by the compiler
    /// from an interpolated string argument.
    /// </param>
    public static void Layout(
        this IRenderSurface surface,
        [InterpolatedStringHandlerArgument("surface")] LayoutHandler handler)
    {
        // The handler renders everything during construction.
        // This method exists only to provide the entry point and
        // wire the IRenderSurface to the handler via InterpolatedStringHandlerArgument.
    }
}

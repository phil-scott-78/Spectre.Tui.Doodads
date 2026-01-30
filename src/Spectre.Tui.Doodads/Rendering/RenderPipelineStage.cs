namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// Base class for render pipeline stages (decorators). Override individual
/// methods to intercept and transform rendering operations. Unoverridden
/// methods pass through to <see cref="Inner"/>.
/// </summary>
public abstract class RenderPipelineStage : IRenderSurface
{
    /// <summary>
    /// Gets the inner surface that this stage wraps.
    /// </summary>
    protected IRenderSurface Inner { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderPipelineStage"/> class.
    /// </summary>
    /// <param name="inner">The inner surface to delegate to.</param>
    protected RenderPipelineStage(IRenderSurface inner) => Inner = inner;

    /// <inheritdoc />
    public virtual Rectangle Viewport => Inner.Viewport;

    /// <inheritdoc />
    public virtual Position SetString(int x, int y, string text, Appearance style)
        => Inner.SetString(x, y, text, style);

    /// <inheritdoc />
    public virtual void Render(IRenderable renderable, Rectangle bounds)
        => Inner.Render(renderable, bounds);
}

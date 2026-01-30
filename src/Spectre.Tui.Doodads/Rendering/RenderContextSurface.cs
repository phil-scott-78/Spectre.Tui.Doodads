namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// Base <see cref="IRenderSurface"/> implementation that delegates to a real
/// <see cref="RenderContext"/>. Stores a pipeline factory so that nested
/// <see cref="Render"/> calls rebuild the full pipeline for constrained contexts.
/// </summary>
public sealed class RenderContextSurface : IRenderSurface
{
    private readonly RenderContext _context;
    private readonly Func<RenderContext, IRenderSurface> _pipelineFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderContextSurface"/> class.
    /// </summary>
    /// <param name="context">The underlying render context.</param>
    /// <param name="pipelineFactory">
    /// A factory that rebuilds the full pipeline from a constrained context.
    /// </param>
    public RenderContextSurface(RenderContext context, Func<RenderContext, IRenderSurface> pipelineFactory)
    {
        _context = context;
        _pipelineFactory = pipelineFactory;
    }

    /// <inheritdoc />
    public Rectangle Viewport => _context.Viewport;

    /// <inheritdoc />
    public Position SetString(int x, int y, string text, Appearance style)
    {
        return _context.SetString(x, y, text, style);
    }

    /// <inheritdoc />
    public void Render(IRenderable renderable, Rectangle bounds)
    {
        var adapter = new RenderableWidgetAdapter(renderable, _pipelineFactory);
        _context.Render(adapter, bounds);
    }
}

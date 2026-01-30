namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// Bridges <see cref="IRenderable"/> to <see cref="IWidget"/> so that
/// Spectre.Tui's <c>context.Render(IWidget, Rectangle)</c> can be used
/// for viewport constraining. Rebuilds the full pipeline from the
/// constrained context before delegating to the renderable.
/// </summary>
internal sealed class RenderableWidgetAdapter : IWidget
{
    private readonly IRenderable _renderable;
    private readonly Func<RenderContext, IRenderSurface> _pipelineFactory;

    public RenderableWidgetAdapter(IRenderable renderable, Func<RenderContext, IRenderSurface> pipelineFactory)
    {
        _renderable = renderable;
        _pipelineFactory = pipelineFactory;
    }

    public void Render(RenderContext context)
    {
        var surface = _pipelineFactory(context);
        _renderable.Render(surface);
    }
}

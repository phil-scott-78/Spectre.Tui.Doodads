namespace Spectre.Tui.Doodads.Rendering;

/// <summary>
/// A render pipeline stage that strips all foreground and background colors.
/// Respects the <c>NO_COLOR</c> convention (see https://no-color.org/).
/// </summary>
public sealed class NoColorStage : RenderPipelineStage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoColorStage"/> class.
    /// </summary>
    /// <param name="inner">The inner render surface to delegate to.</param>
    public NoColorStage(IRenderSurface inner)
        : base(inner)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the <c>NO_COLOR</c> environment variable is set.
    /// </summary>
    public static bool IsNoColorSet =>
        Environment.GetEnvironmentVariable("NO_COLOR") is not null;

    /// <inheritdoc />
    public override Position SetString(int x, int y, string text, Appearance style)
    {
        if (!IsNoColorSet)
        {
            return Inner.SetString(x, y, text, style);
        }

        var stripped = style with
        {
            Foreground = Color.Default,
            Background = Color.Default,
        };

        return base.SetString(x, y, text, stripped);
    }
}

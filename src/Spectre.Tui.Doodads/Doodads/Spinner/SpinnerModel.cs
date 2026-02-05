namespace Spectre.Tui.Doodads.Doodads.Spinner;

/// <summary>
/// An animated loading indicator with customizable frames.
/// </summary>
public record SpinnerModel : IDoodad<SpinnerModel>, ISizedRenderable
{
    /// <summary>
    /// Gets the spinner type that defines frames and interval.
    /// </summary>
    public SpinnerType Spinner { get; init; } = SpinnerType.Line;

    /// <summary>
    /// Gets the minimum display width of the spinner.
    /// </summary>
    public int MinWidth { get; init; } = 4;

    /// <summary>
    /// Gets the minimum display height of the spinner (always 1).
    /// </summary>
    public int MinHeight { get; init; } = 1;

    /// <summary>
    /// Gets the style applied to the spinner frame.
    /// </summary>
    public Appearance Style { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the current frame index.
    /// </summary>
    internal int Frame { get; init; }

    /// <summary>
    /// Gets the tick source for stale tick detection.
    /// </summary>
    public TickSource Ticks { get; init; } = new();

    /// <inheritdoc />
    public Command Init()
    {
        return Ticks.CreateTick(Spinner.Interval);
    }

    /// <inheritdoc />
    public (SpinnerModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case TickMessage tick when Ticks.IsValid(tick):
                var nextFrame = (Frame + 1) % Spinner.Frames.Count;
                var updated = this with { Frame = nextFrame };
                return (updated, updated.Ticks.CreateTick(Spinner.Interval));

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var frame = Spinner.Frames.Count > 0
            ? Spinner.Frames[Frame % Spinner.Frames.Count]
            : string.Empty;

        surface.SetString(0, 0, frame, Style);
    }

    /// <summary>
    /// Manually advances the spinner to the next frame.
    /// </summary>
    /// <returns>The updated model and a command to schedule the next tick.</returns>
    public (SpinnerModel Model, Command? Command) Tick()
    {
        var nextFrame = (Frame + 1) % Spinner.Frames.Count;
        var updated = this with { Frame = nextFrame };
        return (updated, updated.Ticks.CreateTick(Spinner.Interval));
    }
}

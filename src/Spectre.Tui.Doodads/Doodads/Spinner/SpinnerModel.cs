namespace Spectre.Tui.Doodads.Doodads.Spinner;

/// <summary>
/// An animated loading indicator with customizable frames.
/// </summary>
public record SpinnerModel : IDoodad<SpinnerModel>, ISizedRenderable
{
    private static int _nextId;

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
    /// Gets the unique identifier for this spinner instance.
    /// </summary>
    public int Id { get; init; } = Interlocked.Increment(ref _nextId);

    /// <summary>
    /// Gets the generation tag for stale tick detection.
    /// </summary>
    internal int Tag { get; init; }

    /// <inheritdoc />
    public Command Init()
    {
        return TickCommand();
    }

    /// <inheritdoc />
    public (SpinnerModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case SpinnerTickMessage tick when tick.Id == Id && tick.Tag == Tag:
                var nextFrame = (Frame + 1) % Spinner.Frames.Count;
                var updated = this with { Frame = nextFrame };
                return (updated, updated.TickCommand());

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
        return (updated, updated.TickCommand());
    }

    private Command TickCommand()
    {
        var id = Id;
        var tag = Tag;
        return Commands.Tick(Spinner.Interval, _ => new SpinnerTickMessage { Id = id, Tag = tag });
    }
}

namespace Spectre.Tui.Doodads.Doodads.Progress;

/// <summary>
/// An animated progress bar with percentage display.
/// </summary>
public record ProgressModel : IDoodad<ProgressModel>, ISizedRenderable
{
    private static int _nextId;

    private const double AnimationSpeed = 0.1;
    private const double AnimationThreshold = 0.001;
    private static readonly TimeSpan AnimationInterval = TimeSpan.FromMilliseconds(16);

    /// <summary>
    /// Gets the minimum width of the progress bar in characters.
    /// </summary>
    public int MinWidth { get; init; } = 40;

    /// <summary>
    /// Gets the minimum height of the progress bar (always 1).
    /// </summary>
    public int MinHeight { get; init; } = 1;

    /// <summary>
    /// Returns the desired size: fills available width, always 1 row tall.
    /// </summary>
    public Size Measure(Size availableSize) => new(availableSize.Width, 1);

    /// <summary>
    /// Gets a value indicating whether to show the percentage text.
    /// </summary>
    public bool ShowPercentage { get; init; } = true;

    /// <summary>
    /// Gets the character used for the filled portion of the bar.
    /// </summary>
    public char FullCharacter { get; init; } = '\u2588';

    /// <summary>
    /// Gets the character used for the empty portion of the bar.
    /// </summary>
    public char EmptyCharacter { get; init; } = '\u2591';

    /// <summary>
    /// Gets the style applied to the filled portion.
    /// </summary>
    public Appearance FullStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style applied to the empty portion.
    /// </summary>
    public Appearance EmptyStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets a value indicating whether to use gradient fill colors.
    /// </summary>
    public bool UseGradient { get; init; }

    /// <summary>
    /// Gets a value indicating whether to use scaled gradient fill (gradient scales with bar width).
    /// </summary>
    public bool UseScaledGradient { get; init; }

    /// <summary>
    /// Gets the first gradient color.
    /// </summary>
    public Color? GradientColorA { get; init; }

    /// <summary>
    /// Gets the second gradient color.
    /// </summary>
    public Color? GradientColorB { get; init; }

    /// <summary>
    /// Gets the spring frequency for animation (higher = faster oscillation).
    /// </summary>
    public double SpringFrequency { get; init; }

    /// <summary>
    /// Gets the spring damping for animation (higher = less oscillation).
    /// </summary>
    public double SpringDamping { get; init; }

    /// <summary>
    /// Gets the format string for the percentage display.
    /// </summary>
    public string PercentFormat { get; init; } = "F0";

    /// <summary>
    /// Gets the style applied to the percentage text.
    /// </summary>
    public Appearance PercentageStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the target percentage (0.0 to 1.0).
    /// </summary>
    public double Percent { get; init; }

    /// <summary>
    /// Gets the currently displayed percentage (animated toward <see cref="Percent"/>).
    /// </summary>
    internal double DisplayPercent { get; init; }

    /// <summary>
    /// Gets the unique identifier for this progress bar instance.
    /// </summary>
    internal int Id { get; init; } = Interlocked.Increment(ref _nextId);

    /// <summary>
    /// Gets a value indicating whether the progress bar is currently animating.
    /// </summary>
    public bool IsAnimating { get; init; }

    /// <summary>
    /// Gets the velocity for spring-based animation.
    /// </summary>
    internal double SpringVelocity { get; init; }

    /// <inheritdoc />
    public Command? Init()
    {
        return null;
    }

    /// <inheritdoc />
    public (ProgressModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case ProgressFrameMessage frame when frame.Id == Id && IsAnimating:
                var diff = Percent - DisplayPercent;
                if (Math.Abs(diff) < AnimationThreshold && Math.Abs(SpringVelocity) < AnimationThreshold)
                {
                    return (this with { DisplayPercent = Percent, IsAnimating = false, SpringVelocity = 0 }, null);
                }

                double newDisplay;
                double newVelocity;
                if (SpringFrequency > 0)
                {
                    var dt = AnimationInterval.TotalSeconds;
                    var angularFreq = SpringFrequency * 2.0 * Math.PI;
                    var force = -angularFreq * angularFreq * (DisplayPercent - Percent);
                    var dampingForce = -2.0 * SpringDamping * angularFreq * SpringVelocity;
                    newVelocity = SpringVelocity + ((force + dampingForce) * dt);
                    newDisplay = DisplayPercent + (newVelocity * dt);
                }
                else
                {
                    newDisplay = DisplayPercent + (diff * AnimationSpeed);
                    newVelocity = 0;
                }

                var updated = this with { DisplayPercent = newDisplay, SpringVelocity = newVelocity };
                return (updated, updated.FrameCommand());

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        RenderBar(surface, DisplayPercent);
    }

    /// <summary>
    /// Renders the progress bar at an arbitrary percentage without changing state.
    /// </summary>
    /// <param name="percent">The percentage to render (0.0 to 1.0).</param>
    /// <returns>A string representation of the bar at the given percent.</returns>
    public string ViewAs(double percent)
    {
        // ViewAs returns a textual representation; rendering is stateless
        var clamped = Math.Clamp(percent, 0.0, 1.0);
        var barWidth = ShowPercentage ? Math.Max(0, MinWidth - 5) : MinWidth;
        var filled = (int)Math.Round(barWidth * clamped);
        var empty = barWidth - filled;
        var bar = new string(FullCharacter, filled) + new string(EmptyCharacter, empty);
        if (ShowPercentage)
        {
            bar += $" {(clamped * 100).ToString(PercentFormat)}%";
        }

        return bar;
    }

    private void RenderBar(IRenderSurface surface, double displayPercent)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var barWidth = ShowPercentage ? Math.Max(0, width - 5) : width;
        var percent = Math.Clamp(displayPercent, 0.0, 1.0);
        var filled = (int)Math.Round(barWidth * percent);
        var empty = barWidth - filled;

        var x = 0;

        if (filled > 0)
        {
            if ((UseGradient || UseScaledGradient) && GradientColorA is not null && GradientColorB is not null)
            {
                var totalForGradient = UseScaledGradient ? barWidth : filled;
                for (var i = 0; i < filled; i++)
                {
                    var t = totalForGradient > 1 ? (double)i / (totalForGradient - 1) : 0;
                    var r = (byte)(GradientColorA.Value.R + ((GradientColorB.Value.R - GradientColorA.Value.R) * t));
                    var g = (byte)(GradientColorA.Value.G + ((GradientColorB.Value.G - GradientColorA.Value.G) * t));
                    var b = (byte)(GradientColorA.Value.B + ((GradientColorB.Value.B - GradientColorA.Value.B) * t));
                    var color = new Color(r, g, b);
                    var style = new Appearance { Foreground = color };
                    surface.SetString(x, 0, FullCharacter.ToString(), style);
                    x++;
                }
            }
            else
            {
                var fullStr = new string(FullCharacter, filled);
                var pos = surface.SetString(x, 0, fullStr, FullStyle);
                x = pos.X;
            }
        }

        if (empty > 0)
        {
            var emptyStr = new string(EmptyCharacter, empty);
            var pos = surface.SetString(x, 0, emptyStr, EmptyStyle);
            x = pos.X;
        }

        if (ShowPercentage)
        {
            var pctText = $" {(percent * 100).ToString(PercentFormat)}%";
            surface.SetString(x, 0, pctText, PercentageStyle);
        }
    }

    /// <summary>
    /// Sets the target percentage, starting animation toward the new value.
    /// </summary>
    /// <param name="percent">Target percentage (0.0 to 1.0).</param>
    /// <returns>The updated model and an optional animation command.</returns>
    public (ProgressModel Model, Command? Command) SetPercent(double percent)
    {
        var clamped = Math.Clamp(percent, 0.0, 1.0);
        var updated = this with { Percent = clamped, IsAnimating = true };
        return (updated, updated.FrameCommand());
    }

    /// <summary>
    /// Sets the percentage immediately without animation.
    /// </summary>
    /// <param name="percent">Target percentage (0.0 to 1.0).</param>
    /// <returns>The updated model with display snapped to the target.</returns>
    public ProgressModel SetPercentImmediate(double percent)
    {
        var clamped = Math.Clamp(percent, 0.0, 1.0);
        return this with { Percent = clamped, DisplayPercent = clamped, IsAnimating = false, SpringVelocity = 0 };
    }

    /// <summary>
    /// Increments the target percentage by the specified delta.
    /// </summary>
    /// <param name="delta">The amount to add (0.0 to 1.0 range).</param>
    /// <returns>The updated model and an optional animation command.</returns>
    public (ProgressModel Model, Command? Command) IncrPercent(double delta)
    {
        return SetPercent(Percent + delta);
    }

    /// <summary>
    /// Decrements the target percentage by the specified delta.
    /// </summary>
    /// <param name="delta">The amount to subtract (0.0 to 1.0 range).</param>
    /// <returns>The updated model and an optional animation command.</returns>
    public (ProgressModel Model, Command? Command) DecrPercent(double delta)
    {
        return SetPercent(Percent - delta);
    }

    /// <summary>
    /// Sets the spring animation parameters.
    /// </summary>
    /// <param name="frequency">The spring frequency (oscillation speed).</param>
    /// <param name="damping">The spring damping (oscillation decay).</param>
    /// <returns>The updated model.</returns>
    public ProgressModel SetSpringOptions(double frequency, double damping)
    {
        return this with { SpringFrequency = frequency, SpringDamping = damping };
    }

    /// <summary>
    /// Configures the progress bar to use gradient fill between two colors.
    /// </summary>
    /// <param name="colorA">The start color.</param>
    /// <param name="colorB">The end color.</param>
    /// <returns>The updated model.</returns>
    public ProgressModel WithGradient(Color colorA, Color colorB)
    {
        return this with { UseGradient = true, UseScaledGradient = false, GradientColorA = colorA, GradientColorB = colorB };
    }

    /// <summary>
    /// Configures the progress bar to use scaled gradient fill between two colors.
    /// </summary>
    /// <param name="colorA">The start color.</param>
    /// <param name="colorB">The end color.</param>
    /// <returns>The updated model.</returns>
    public ProgressModel WithScaledGradient(Color colorA, Color colorB)
    {
        return this with { UseGradient = false, UseScaledGradient = true, GradientColorA = colorA, GradientColorB = colorB };
    }

    /// <summary>
    /// Configures the progress bar to use default gradient colors (magenta to blue).
    /// </summary>
    /// <returns>The updated model.</returns>
    public ProgressModel WithDefaultGradient()
    {
        return WithGradient(new Color(255, 0, 255), new Color(0, 0, 255));
    }

    private Command FrameCommand()
    {
        var id = Id;
        return Commands.Tick(AnimationInterval, _ => new ProgressFrameMessage { Id = id });
    }
}

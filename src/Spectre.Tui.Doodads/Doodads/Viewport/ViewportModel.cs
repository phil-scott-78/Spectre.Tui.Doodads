namespace Spectre.Tui.Doodads.Doodads.Viewport;

/// <summary>
/// A scrollable content area that renders a window into larger content.
/// </summary>
public record ViewportModel : IDoodad<ViewportModel>, ISizedRenderable
{
    /// <summary>
    /// Gets the minimum viewport width in columns.
    /// </summary>
    public int MinWidth { get; init; }

    /// <summary>
    /// Gets the minimum viewport height in rows.
    /// </summary>
    public int MinHeight { get; init; }

    /// <summary>
    /// Gets the actual rendered width (updated via <see cref="WindowSizeMessage"/>).
    /// Falls back to <see cref="MinWidth"/> when zero.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets the actual rendered height (updated via <see cref="WindowSizeMessage"/>).
    /// Falls back to <see cref="MinHeight"/> when zero.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Gets a value indicating whether mouse wheel scrolling is enabled.
    /// </summary>
    public bool MouseWheelEnabled { get; init; }

    /// <summary>
    /// Gets the number of lines to scroll per mouse wheel tick.
    /// </summary>
    public int MouseWheelDelta { get; init; } = 3;

    /// <summary>
    /// Gets the vertical scroll offset.
    /// </summary>
    public int YOffset { get; init; }

    /// <summary>
    /// Gets the horizontal scroll offset.
    /// </summary>
    public int XOffset { get; init; }

    /// <summary>
    /// Gets the key map for viewport navigation.
    /// </summary>
    public ViewportKeyMap KeyMap { get; init; } = new();

    /// <summary>
    /// Gets the content lines.
    /// </summary>
    internal ImmutableArray<string> Lines { get; init; } = [];

    /// <summary>
    /// Gets the style applied to the viewport frame.
    /// </summary>
    public Appearance Style { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the Y position of the viewport in the terminal.
    /// </summary>
    public int YPosition { get; init; }

    /// <summary>
    /// Gets the horizontal step size for left/right scrolling.
    /// </summary>
    public int HorizontalStep { get; init; } = 1;

    /// <summary>
    /// Gets the scroll position as a percentage (0.0 to 1.0).
    /// </summary>
    public double ScrollPercent
    {
        get
        {
            var maxOffset = MaxYOffset;
            if (maxOffset <= 0)
            {
                return 0.0;
            }

            return (double)YOffset / maxOffset;
        }
    }

    /// <summary>
    /// Gets the horizontal scroll position as a percentage (0.0 to 1.0).
    /// </summary>
    public double HorizontalScrollPercent
    {
        get
        {
            var maxOffset = MaxXOffset;
            if (maxOffset <= 0)
            {
                return 0.0;
            }

            return (double)XOffset / maxOffset;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the viewport is scrolled to the top.
    /// </summary>
    public bool AtTop => YOffset <= 0;

    /// <summary>
    /// Gets a value indicating whether the viewport is scrolled to the bottom.
    /// </summary>
    public bool AtBottom => YOffset >= MaxYOffset;

    /// <summary>
    /// Gets a value indicating whether the viewport is scrolled past the bottom of the content.
    /// </summary>
    public bool PastBottom => YOffset > MaxYOffset;

    /// <summary>
    /// Gets the total number of content lines.
    /// </summary>
    public int TotalLines => Lines.Length;

    /// <summary>
    /// Gets the number of visible lines in the viewport.
    /// </summary>
    public int VisibleLines => Math.Min(EffectiveHeight, Lines.Length);

    private int EffectiveWidth => Width > 0 ? Width : MinWidth;

    private int EffectiveHeight => Height > 0 ? Height : MinHeight;

    /// <summary>
    /// Sets the content to display in the viewport.
    /// </summary>
    /// <param name="content">The text content, split by newlines.</param>
    public ViewportModel SetContent(string content)
    {
        var lines = content.Split('\n');
        return this with
        {
            Lines = [.. lines],
            YOffset = 0,
            XOffset = 0,
        };
    }

    /// <inheritdoc />
    public Command? Init() => null;

    /// <inheritdoc />
    public (ViewportModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case KeyMessage km:
                return HandleKey(km);

            case MouseMessage mm when MouseWheelEnabled:
                return HandleMouse(mm);

            case WindowSizeMessage ws:
                return (this with { Width = ws.Width, Height = ws.Height }, null);

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);

        for (var y = 0; y < height; y++)
        {
            var lineIndex = YOffset + y;
            if (lineIndex >= Lines.Length)
            {
                break;
            }

            var line = Lines[lineIndex];
            if (XOffset < line.Length)
            {
                var visible = line.Substring(XOffset);
                if (visible.Length > width)
                {
                    visible = visible.Substring(0, width);
                }

                surface.SetString(0, y, visible, Appearance.Plain);
            }
        }
    }

    /// <summary>
    /// Scrolls down by the specified number of lines.
    /// </summary>
    public (ViewportModel Model, Command? Command) LineDown(int n = 1)
    {
        return (ClampYOffset(this with { YOffset = YOffset + n }), null);
    }

    /// <summary>
    /// Scrolls up by the specified number of lines.
    /// </summary>
    public (ViewportModel Model, Command? Command) LineUp(int n = 1)
    {
        return (ClampYOffset(this with { YOffset = YOffset - n }), null);
    }

    /// <summary>
    /// Scrolls down one full page.
    /// </summary>
    public (ViewportModel Model, Command? Command) PageDown()
    {
        return LineDown(EffectiveHeight);
    }

    /// <summary>
    /// Scrolls up one full page.
    /// </summary>
    public (ViewportModel Model, Command? Command) PageUp()
    {
        return LineUp(EffectiveHeight);
    }

    /// <summary>
    /// Scrolls down half a page.
    /// </summary>
    public (ViewportModel Model, Command? Command) HalfPageDown()
    {
        return LineDown(EffectiveHeight / 2);
    }

    /// <summary>
    /// Scrolls up half a page.
    /// </summary>
    public (ViewportModel Model, Command? Command) HalfPageUp()
    {
        return LineUp(EffectiveHeight / 2);
    }

    /// <summary>
    /// Scrolls to the top of the content.
    /// </summary>
    public (ViewportModel Model, Command? Command) GotoTop()
    {
        return (this with { YOffset = 0 }, null);
    }

    /// <summary>
    /// Scrolls to the bottom of the content.
    /// </summary>
    public (ViewportModel Model, Command? Command) GotoBottom()
    {
        return (ClampYOffset(this with { YOffset = MaxYOffset }), null);
    }

    /// <summary>
    /// Sets the vertical scroll offset.
    /// </summary>
    /// <param name="offset">The new Y offset.</param>
    public ViewportModel SetYOffset(int offset)
    {
        return ClampYOffset(this with { YOffset = offset });
    }

    /// <summary>
    /// Sets the horizontal scroll offset.
    /// </summary>
    /// <param name="offset">The new X offset.</param>
    public ViewportModel SetXOffset(int offset)
    {
        return ClampXOffset(this with { XOffset = offset });
    }

    /// <summary>
    /// Sets the horizontal step size for left/right scrolling.
    /// </summary>
    /// <param name="step">The number of columns to scroll per step.</param>
    public ViewportModel SetHorizontalStep(int step)
    {
        return this with { HorizontalStep = Math.Max(1, step) };
    }

    /// <summary>
    /// Scrolls left by the specified number of columns.
    /// </summary>
    /// <param name="n">The number of columns to scroll (defaults to HorizontalStep).</param>
    public (ViewportModel Model, Command? Command) ScrollLeft(int? n = null)
    {
        var step = n ?? HorizontalStep;
        return (ClampXOffset(this with { XOffset = XOffset - step }), null);
    }

    /// <summary>
    /// Scrolls right by the specified number of columns.
    /// </summary>
    /// <param name="n">The number of columns to scroll (defaults to HorizontalStep).</param>
    public (ViewportModel Model, Command? Command) ScrollRight(int? n = null)
    {
        var step = n ?? HorizontalStep;
        return (ClampXOffset(this with { XOffset = XOffset + step }), null);
    }

    private int MaxYOffset => Math.Max(0, Lines.Length - EffectiveHeight);

    private int MaxXOffset
    {
        get
        {
            var maxLineLength = 0;
            foreach (var line in Lines)
            {
                if (line.Length > maxLineLength)
                {
                    maxLineLength = line.Length;
                }
            }

            return Math.Max(0, maxLineLength - EffectiveWidth);
        }
    }

    private (ViewportModel Model, Command? Command) HandleKey(KeyMessage km)
    {
        if (KeyMap.PageDown.Matches(km))
        {
            return PageDown();
        }

        if (KeyMap.PageUp.Matches(km))
        {
            return PageUp();
        }

        if (KeyMap.HalfPageDown.Matches(km))
        {
            return HalfPageDown();
        }

        if (KeyMap.HalfPageUp.Matches(km))
        {
            return HalfPageUp();
        }

        if (KeyMap.LineDown.Matches(km))
        {
            return LineDown();
        }

        if (KeyMap.LineUp.Matches(km))
        {
            return LineUp();
        }

        if (KeyMap.Left.Matches(km))
        {
            return (ClampXOffset(this with { XOffset = XOffset - 1 }), null);
        }

        if (KeyMap.Right.Matches(km))
        {
            return (ClampXOffset(this with { XOffset = XOffset + 1 }), null);
        }

        if (KeyMap.Home.Matches(km))
        {
            return GotoTop();
        }

        if (KeyMap.End.Matches(km))
        {
            return GotoBottom();
        }

        return (this, null);
    }

    private (ViewportModel Model, Command? Command) HandleMouse(MouseMessage mm)
    {
        return mm.Action switch
        {
            MouseAction.WheelUp => LineUp(MouseWheelDelta),
            MouseAction.WheelDown => LineDown(MouseWheelDelta),
            _ => (this, null),
        };
    }

    private static ViewportModel ClampYOffset(ViewportModel model)
    {
        var clamped = Math.Clamp(model.YOffset, 0, model.MaxYOffset);
        return model with { YOffset = clamped };
    }

    private static ViewportModel ClampXOffset(ViewportModel model)
    {
        var clamped = Math.Max(0, model.XOffset);
        return model with { XOffset = clamped };
    }
}

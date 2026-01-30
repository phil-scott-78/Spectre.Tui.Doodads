namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// A wrapper renderable that draws box-drawing characters around a child
/// <see cref="ISizedRenderable"/>. Supports configurable border styles,
/// independent side visibility, and an optional title in the top border.
/// </summary>
public record Border : ISizedRenderable
{
    /// <summary>
    /// Gets the child renderable to wrap.
    /// </summary>
    public required ISizedRenderable Content { get; init; }

    /// <summary>
    /// Gets the border style defining which characters to use.
    /// Defaults to <see cref="BorderStyle.Normal"/>.
    /// </summary>
    public BorderStyle BorderStyle { get; init; } = BorderStyle.Normal;

    /// <summary>
    /// Gets the style applied to the border characters.
    /// </summary>
    public Appearance Style { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets whether the top border line is visible.
    /// </summary>
    public bool ShowTop { get; init; } = true;

    /// <summary>
    /// Gets whether the bottom border line is visible.
    /// </summary>
    public bool ShowBottom { get; init; } = true;

    /// <summary>
    /// Gets whether the left border line is visible.
    /// </summary>
    public bool ShowLeft { get; init; } = true;

    /// <summary>
    /// Gets whether the right border line is visible.
    /// </summary>
    public bool ShowRight { get; init; } = true;

    /// <summary>
    /// Gets the optional title text displayed in the top border line.
    /// Requires <see cref="ShowTop"/> to be <c>true</c>.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the style applied to the title text.
    /// </summary>
    public Appearance TitleStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the alignment of the title within the top border line.
    /// </summary>
    public TitleAlignment TitleAlignment { get; init; } = TitleAlignment.Left;

    private int LeftChrome => ShowLeft ? 1 : 0;
    private int RightChrome => ShowRight ? 1 : 0;
    private int TopChrome => ShowTop ? 1 : 0;
    private int BottomChrome => ShowBottom ? 1 : 0;

    /// <inheritdoc />
    public int MinWidth => Content.MinWidth + LeftChrome + RightChrome;

    /// <inheritdoc />
    public int MinHeight => Content.MinHeight + TopChrome + BottomChrome;

    /// <inheritdoc />
    public Size Measure(Size availableSize)
    {
        var innerAvailable = new Size(
            Math.Max(0, availableSize.Width - LeftChrome - RightChrome),
            Math.Max(0, availableSize.Height - TopChrome - BottomChrome));
        var inner = Content.Measure(innerAvailable);
        return new Size(inner.Width + LeftChrome + RightChrome, inner.Height + TopChrome + BottomChrome);
    }

    /// <inheritdoc />
    public void Render(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);

        var minRequiredWidth = LeftChrome + RightChrome;
        var minRequiredHeight = TopChrome + BottomChrome;

        if (width < minRequiredWidth || height < minRequiredHeight)
        {
            return;
        }

        var innerWidth = width - LeftChrome - RightChrome;
        var innerHeight = height - TopChrome - BottomChrome;
        var lastCol = width - 1;
        var lastRow = height - 1;

        // Top border
        if (ShowTop)
        {
            // Top-left corner or extended edge
            if (ShowLeft)
            {
                surface.SetString(0, 0, BorderStyle.TopLeft, Style);
            }

            // Top edge characters
            if (innerWidth > 0)
            {
                var topEdge = new string(BorderStyle.Top[0], innerWidth);
                surface.SetString(LeftChrome, 0, topEdge, Style);
            }

            // Top-right corner or extended edge
            if (ShowRight)
            {
                surface.SetString(lastCol, 0, BorderStyle.TopRight, Style);
            }

            // Render title if present and there's space
            RenderTitle(surface, width);
        }

        // Side borders (for rows between top and bottom)
        for (var y = TopChrome; y < height - BottomChrome; y++)
        {
            if (ShowLeft)
            {
                surface.SetString(0, y, BorderStyle.Left, Style);
            }

            if (ShowRight)
            {
                surface.SetString(lastCol, y, BorderStyle.Right, Style);
            }
        }

        // Bottom border
        if (ShowBottom)
        {
            // Bottom-left corner or extended edge
            if (ShowLeft)
            {
                surface.SetString(0, lastRow, BorderStyle.BottomLeft, Style);
            }

            // Bottom edge characters
            if (innerWidth > 0)
            {
                var bottomEdge = new string(BorderStyle.Bottom[0], innerWidth);
                surface.SetString(LeftChrome, lastRow, bottomEdge, Style);
            }

            // Bottom-right corner or extended edge
            if (ShowRight)
            {
                surface.SetString(lastCol, lastRow, BorderStyle.BottomRight, Style);
            }
        }

        // Render child content inside the border
        if (innerWidth > 0 && innerHeight > 0)
        {
            surface.Render(Content, new Rectangle(LeftChrome, TopChrome, innerWidth, innerHeight));
        }
    }

    private void RenderTitle(IRenderSurface surface, int width)
    {
        if (string.IsNullOrEmpty(Title) || !ShowTop)
        {
            return;
        }

        // Available space for title content (including the padding spaces around it)
        // We need at least: left_corner(if shown) + space + 1_char + space + right_corner(if shown)
        // The title area is the inner width between corners
        var titleAreaStart = LeftChrome;
        var titleAreaEnd = width - RightChrome;
        var titleAreaWidth = titleAreaEnd - titleAreaStart;

        if (titleAreaWidth < 3)
        {
            // Not enough space for even " X " (space + char + space)
            return;
        }

        // Format title with padding spaces: " Title "
        var maxTitleLength = titleAreaWidth - 2; // reserve 1 space on each side
        var displayTitle = Title.Length <= maxTitleLength
            ? Title
            : Title[..(maxTitleLength - 1)] + "\u2026"; // ellipsis truncation

        var paddedTitle = " " + displayTitle + " ";

        // Calculate position based on alignment
        var titleX = TitleAlignment switch
        {
            TitleAlignment.Left => titleAreaStart,
            TitleAlignment.Right => titleAreaEnd - paddedTitle.Length,
            TitleAlignment.Center => titleAreaStart + ((titleAreaWidth - paddedTitle.Length) / 2),
            _ => titleAreaStart,
        };

        // Clamp to valid range
        titleX = Math.Max(titleAreaStart, Math.Min(titleX, titleAreaEnd - paddedTitle.Length));

        surface.SetString(titleX, 0, paddedTitle, TitleStyle);
    }
}

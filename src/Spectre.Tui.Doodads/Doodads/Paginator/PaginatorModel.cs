namespace Spectre.Tui.Doodads.Doodads.Paginator;

/// <summary>
/// Pagination logic and UI rendering for paged content.
/// </summary>
public record PaginatorModel : IDoodad<PaginatorModel>
{
    /// <summary>
    /// Gets the paginator display type.
    /// </summary>
    public PaginatorType Type { get; init; } = PaginatorType.Dots;

    /// <summary>
    /// Gets the current page (0-indexed).
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PerPage { get; init; } = 10;

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; init; } = 1;

    /// <summary>
    /// Gets the string used for the active page dot.
    /// </summary>
    public string ActiveDot { get; init; } = "\u25cf";

    /// <summary>
    /// Gets the string used for inactive page dots.
    /// </summary>
    public string InactiveDot { get; init; } = "\u25cb";

    /// <summary>
    /// Gets the format string for numeric display mode.
    /// </summary>
    public string NumericFormat { get; init; } = "{0}/{1}";

    /// <summary>
    /// Gets the style for the active page indicator.
    /// </summary>
    public Appearance ActiveStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for inactive page indicators.
    /// </summary>
    public Appearance InactiveStyle { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the key map for paginator navigation.
    /// </summary>
    public PaginatorKeyMap KeyMap { get; init; } = new();

    /// <summary>
    /// Gets a value indicating whether the current page is the first page.
    /// </summary>
    public bool OnFirstPage => Page <= 0;

    /// <summary>
    /// Gets a value indicating whether the current page is the last page.
    /// </summary>
    public bool OnLastPage => Page >= TotalPages - 1;

    /// <summary>
    /// Sets the total number of pages based on item count and <see cref="PerPage"/>.
    /// </summary>
    /// <param name="totalItems">The total number of items.</param>
    public PaginatorModel SetTotalPages(int totalItems)
    {
        if (PerPage <= 0)
        {
            return this with { TotalPages = 1, Page = 0 };
        }

        var pages = (int)Math.Ceiling((double)totalItems / PerPage);
        pages = Math.Max(1, pages);
        var page = Math.Clamp(Page, 0, pages - 1);
        return this with { TotalPages = pages, Page = page };
    }

    /// <summary>
    /// Gets the start and end indices for the current page.
    /// </summary>
    /// <param name="totalItems">The total number of items.</param>
    /// <returns>A tuple of (Start, End) indices where End is exclusive.</returns>
    public (int Start, int End) GetSliceBounds(int totalItems)
    {
        var start = Page * PerPage;
        var end = Math.Min(start + PerPage, totalItems);
        return (start, end);
    }

    /// <summary>
    /// Gets the number of items on the current page.
    /// </summary>
    /// <param name="totalItems">The total number of items.</param>
    public int ItemsOnPage(int totalItems)
    {
        var (start, end) = GetSliceBounds(totalItems);
        return Math.Max(0, end - start);
    }

    /// <summary>
    /// Navigates to the next page.
    /// </summary>
    public (PaginatorModel Model, Command? Command) NextPage()
    {
        if (OnLastPage)
        {
            return (this, null);
        }

        return (this with { Page = Page + 1 }, null);
    }

    /// <summary>
    /// Navigates to the previous page.
    /// </summary>
    public (PaginatorModel Model, Command? Command) PrevPage()
    {
        if (OnFirstPage)
        {
            return (this, null);
        }

        return (this with { Page = Page - 1 }, null);
    }

    /// <inheritdoc />
    public Command? Init() => null;

    /// <inheritdoc />
    public (PaginatorModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case KeyMessage km when KeyMap.NextPage.Matches(km):
                return NextPage();

            case KeyMessage km when KeyMap.PrevPage.Matches(km):
                return PrevPage();

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        if (Type == PaginatorType.Dots)
        {
            RenderDots(surface);
        }
        else
        {
            RenderNumeric(surface);
        }
    }

    private void RenderDots(IRenderSurface surface)
    {
        var x = 0;
        for (var i = 0; i < TotalPages; i++)
        {
            if (i > 0)
            {
                surface.SetString(x, 0, " ", Appearance.Plain);
                x++;
            }

            if (i == Page)
            {
                var pos = surface.SetString(x, 0, ActiveDot, ActiveStyle);
                x = pos.X;
            }
            else
            {
                var pos = surface.SetString(x, 0, InactiveDot, InactiveStyle);
                x = pos.X;
            }
        }
    }

    private void RenderNumeric(IRenderSurface surface)
    {
        var text = string.Format(
            CultureInfo.InvariantCulture,
            NumericFormat,
            Page + 1,
            TotalPages);
        surface.SetString(0, 0, text, ActiveStyle);
    }
}

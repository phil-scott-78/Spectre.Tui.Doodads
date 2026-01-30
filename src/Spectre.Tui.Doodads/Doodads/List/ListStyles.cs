namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// Unified style configuration for all list chrome elements.
/// </summary>
public record ListStyles
{
    /// <summary>
    /// Gets the style for the title bar.
    /// </summary>
    public Appearance TitleBar { get; init; } = new() { Decoration = Decoration.Bold };

    /// <summary>
    /// Gets the style for the title text.
    /// </summary>
    public Appearance Title { get; init; } = new() { Decoration = Decoration.Bold };

    /// <summary>
    /// Gets the style for the spinner.
    /// </summary>
    public Appearance Spinner { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for the filter prompt.
    /// </summary>
    public Appearance FilterPrompt { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for the filter cursor.
    /// </summary>
    public Appearance FilterCursor { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the default style for filter character matches.
    /// </summary>
    public Appearance DefaultFilterCharacterMatch { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for the status bar.
    /// </summary>
    public Appearance StatusBar { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for the status bar when empty.
    /// </summary>
    public Appearance StatusEmpty { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for the status bar when a filter is active.
    /// </summary>
    public Appearance StatusBarActiveFilter { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for the filter count in the status bar.
    /// </summary>
    public Appearance StatusBarFilterCount { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for the "no items" message.
    /// </summary>
    public Appearance NoItems { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for pagination.
    /// </summary>
    public Appearance PaginationStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for help text.
    /// </summary>
    public Appearance HelpStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for active pagination dots.
    /// </summary>
    public Appearance ActivePaginationDot { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for inactive pagination dots.
    /// </summary>
    public Appearance InactivePaginationDot { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for arabic (numeric) pagination.
    /// </summary>
    public Appearance ArabicPagination { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets the style for the divider dot.
    /// </summary>
    public Appearance DividerDot { get; init; } = new() { Decoration = Decoration.Dim };
}
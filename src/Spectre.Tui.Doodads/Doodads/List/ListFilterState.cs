namespace Spectre.Tui.Doodads.Doodads.List;

/// <summary>
/// Represents the current state of list filtering.
/// </summary>
public enum ListFilterState
{
    /// <summary>
    /// No filter is active; all items are shown.
    /// </summary>
    Unfiltered,

    /// <summary>
    /// The user is actively typing a filter string.
    /// </summary>
    Filtering,

    /// <summary>
    /// A filter has been applied and results are shown.
    /// </summary>
    FilterApplied,
}
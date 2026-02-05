namespace Spectre.Tui.Doodads;

/// <summary>
/// Shared scroll-into-view calculations for components with cursor tracking.
/// </summary>
public static class ScrollHelper
{
    /// <summary>
    /// Adjusts the scroll <paramref name="offset"/> so that <paramref name="position"/>
    /// remains visible within a viewport of the given <paramref name="viewportSize"/>.
    /// </summary>
    /// <param name="position">The cursor or selection position to keep visible.</param>
    /// <param name="offset">The current scroll offset.</param>
    /// <param name="viewportSize">The number of visible rows or columns.</param>
    /// <returns>The adjusted scroll offset.</returns>
    public static int EnsureVisible(int position, int offset, int viewportSize)
    {
        if (position < offset)
        {
            offset = position;
        }

        if (position >= offset + viewportSize)
        {
            offset = position - viewportSize + 1;
        }

        return offset;
    }
}

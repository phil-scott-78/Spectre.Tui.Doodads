namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// A wrapper renderable that adds configurable padding around a child
/// <see cref="ISizedRenderable"/>.
/// </summary>
public record Padding : ISizedRenderable
{
    /// <summary>
    /// Gets the child renderable to wrap.
    /// </summary>
    public required ISizedRenderable Content { get; init; }

    /// <summary>
    /// Gets the top padding in rows.
    /// </summary>
    public int Top { get; init; }

    /// <summary>
    /// Gets the right padding in columns.
    /// </summary>
    public int Right { get; init; }

    /// <summary>
    /// Gets the bottom padding in rows.
    /// </summary>
    public int Bottom { get; init; }

    /// <summary>
    /// Gets the left padding in columns.
    /// </summary>
    public int Left { get; init; }

    /// <inheritdoc />
    public int MinWidth => Content.MinWidth + Left + Right;

    /// <inheritdoc />
    public int MinHeight => Content.MinHeight + Top + Bottom;

    /// <inheritdoc />
    public Size Measure(Size availableSize)
    {
        var innerAvailable = new Size(
            Math.Max(0, availableSize.Width - Left - Right),
            Math.Max(0, availableSize.Height - Top - Bottom));
        var inner = Content.Measure(innerAvailable);
        return new Size(inner.Width + Left + Right, inner.Height + Top + Bottom);
    }

    /// <inheritdoc />
    public void Render(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);

        var innerWidth = Math.Max(0, width - Left - Right);
        var innerHeight = Math.Max(0, height - Top - Bottom);

        if (innerWidth > 0 && innerHeight > 0)
        {
            surface.Render(Content, new Rectangle(Left, Top, innerWidth, innerHeight));
        }
    }

    /// <summary>
    /// Creates a <see cref="Padding"/> with equal padding on all sides.
    /// </summary>
    /// <param name="padding">The padding amount for all sides.</param>
    /// <param name="content">The child renderable to wrap.</param>
    /// <returns>A new <see cref="Padding"/> instance.</returns>
    public static Padding All(int padding, ISizedRenderable content) =>
        new() { Content = content, Top = padding, Right = padding, Bottom = padding, Left = padding };

    /// <summary>
    /// Creates a <see cref="Padding"/> with equal left and right padding.
    /// </summary>
    /// <param name="padding">The horizontal padding amount.</param>
    /// <param name="content">The child renderable to wrap.</param>
    /// <returns>A new <see cref="Padding"/> instance.</returns>
    public static Padding Horizontal(int padding, ISizedRenderable content) =>
        new() { Content = content, Left = padding, Right = padding };

    /// <summary>
    /// Creates a <see cref="Padding"/> with equal top and bottom padding.
    /// </summary>
    /// <param name="padding">The vertical padding amount.</param>
    /// <param name="content">The child renderable to wrap.</param>
    /// <returns>A new <see cref="Padding"/> instance.</returns>
    public static Padding Vertical(int padding, ISizedRenderable content) =>
        new() { Content = content, Top = padding, Bottom = padding };

    /// <summary>
    /// Creates a <see cref="Padding"/> with independent vertical and horizontal padding.
    /// </summary>
    /// <param name="vertical">The top and bottom padding amount.</param>
    /// <param name="horizontal">The left and right padding amount.</param>
    /// <param name="content">The child renderable to wrap.</param>
    /// <returns>A new <see cref="Padding"/> instance.</returns>
    public static Padding Symmetric(int vertical, int horizontal, ISizedRenderable content) =>
        new() { Content = content, Top = vertical, Bottom = vertical, Left = horizontal, Right = horizontal };
}

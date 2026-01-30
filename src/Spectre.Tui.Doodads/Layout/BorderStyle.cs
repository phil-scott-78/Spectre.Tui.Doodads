namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// Immutable record defining the characters for each border position.
/// Provides predefined static styles for common border types.
/// </summary>
public record BorderStyle
{
    /// <summary>
    /// Gets the character used for the top edge.
    /// </summary>
    public string Top { get; init; } = "\u2500";

    /// <summary>
    /// Gets the character used for the bottom edge.
    /// </summary>
    public string Bottom { get; init; } = "\u2500";

    /// <summary>
    /// Gets the character used for the left edge.
    /// </summary>
    public string Left { get; init; } = "\u2502";

    /// <summary>
    /// Gets the character used for the right edge.
    /// </summary>
    public string Right { get; init; } = "\u2502";

    /// <summary>
    /// Gets the character used for the top-left corner.
    /// </summary>
    public string TopLeft { get; init; } = "\u250c";

    /// <summary>
    /// Gets the character used for the top-right corner.
    /// </summary>
    public string TopRight { get; init; } = "\u2510";

    /// <summary>
    /// Gets the character used for the bottom-left corner.
    /// </summary>
    public string BottomLeft { get; init; } = "\u2514";

    /// <summary>
    /// Gets the character used for the bottom-right corner.
    /// </summary>
    public string BottomRight { get; init; } = "\u2518";

    /// <summary>
    /// Normal box-drawing style: ─│┌┐└┘
    /// </summary>
    public static BorderStyle Normal { get; } = new();

    /// <summary>
    /// Rounded box-drawing style: ─│╭╮╰╯
    /// </summary>
    public static BorderStyle Rounded { get; } = new()
    {
        TopLeft = "\u256d",
        TopRight = "\u256e",
        BottomLeft = "\u2570",
        BottomRight = "\u256f",
    };

    /// <summary>
    /// Thick box-drawing style: ━┃┏┓┗┛
    /// </summary>
    public static BorderStyle Thick { get; } = new()
    {
        Top = "\u2501",
        Bottom = "\u2501",
        Left = "\u2503",
        Right = "\u2503",
        TopLeft = "\u250f",
        TopRight = "\u2513",
        BottomLeft = "\u2517",
        BottomRight = "\u251b",
    };

    /// <summary>
    /// Double-line box-drawing style: ═║╔╗╚╝
    /// </summary>
    public static BorderStyle Double { get; } = new()
    {
        Top = "\u2550",
        Bottom = "\u2550",
        Left = "\u2551",
        Right = "\u2551",
        TopLeft = "\u2554",
        TopRight = "\u2557",
        BottomLeft = "\u255a",
        BottomRight = "\u255d",
    };

    /// <summary>
    /// Hidden style using spaces. Preserves layout dimensions but renders invisibly.
    /// </summary>
    public static BorderStyle Hidden { get; } = new()
    {
        Top = " ",
        Bottom = " ",
        Left = " ",
        Right = " ",
        TopLeft = " ",
        TopRight = " ",
        BottomLeft = " ",
        BottomRight = " ",
    };

    /// <summary>
    /// ASCII-only style: -|++++
    /// </summary>
    public static BorderStyle Ascii { get; } = new()
    {
        Top = "-",
        Bottom = "-",
        Left = "|",
        Right = "|",
        TopLeft = "+",
        TopRight = "+",
        BottomLeft = "+",
        BottomRight = "+",
    };

    /// <summary>
    /// Block style using full block characters: █ for all positions.
    /// </summary>
    public static BorderStyle Block { get; } = new()
    {
        Top = "\u2588",
        Bottom = "\u2588",
        Left = "\u2588",
        Right = "\u2588",
        TopLeft = "\u2588",
        TopRight = "\u2588",
        BottomLeft = "\u2588",
        BottomRight = "\u2588",
    };

    /// <summary>
    /// Outer half-block style: ▀▄▌▐▛▜▙▟
    /// </summary>
    public static BorderStyle OuterHalfBlock { get; } = new()
    {
        Top = "\u2580",
        Bottom = "\u2584",
        Left = "\u258c",
        Right = "\u2590",
        TopLeft = "\u259b",
        TopRight = "\u259c",
        BottomLeft = "\u2599",
        BottomRight = "\u259f",
    };

    /// <summary>
    /// Inner half-block style: ▄▀▐▌▗▖▝▘
    /// </summary>
    public static BorderStyle InnerHalfBlock { get; } = new()
    {
        Top = "\u2584",
        Bottom = "\u2580",
        Left = "\u2590",
        Right = "\u258c",
        TopLeft = "\u2597",
        TopRight = "\u2596",
        BottomLeft = "\u259d",
        BottomRight = "\u2598",
    };

    internal static IReadOnlyList<BorderStyle> AllStyles { get; } =
        [Normal, Rounded, Thick, Double, Hidden, Ascii, Block, OuterHalfBlock, InnerHalfBlock];
}

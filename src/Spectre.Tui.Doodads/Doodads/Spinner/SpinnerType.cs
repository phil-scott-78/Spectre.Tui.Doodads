namespace Spectre.Tui.Doodads.Doodads.Spinner;

/// <summary>
/// Defines a spinner animation as a sequence of frames and a display interval.
/// </summary>
public record SpinnerType(IReadOnlyList<string> Frames, TimeSpan Interval)
{
    /// <summary>
    /// Line spinner: |, /, -, \.
    /// </summary>
    public static SpinnerType Line { get; } = new(
        ["|", "/", "-", "\\"],
        TimeSpan.FromMilliseconds(130));

    /// <summary>
    /// Dot spinner using Braille patterns.
    /// </summary>
    public static SpinnerType Dot { get; } = new(
        ["\u2800", "\u2804", "\u2806", "\u2807", "\u2803", "\u2801"],
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Mini dot spinner using smaller Braille patterns.
    /// </summary>
    public static SpinnerType MiniDot { get; } = new(
        ["\u2810", "\u2820", "\u2804", "\u2802", "\u2801"],
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Jump spinner with bouncing characters.
    /// </summary>
    public static SpinnerType Jump { get; } = new(
        ["\u28FE", "\u28FD", "\u28FB", "\u28BF", "\u287F", "\u28DF", "\u28EF", "\u28F7"],
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Pulse spinner with fading blocks.
    /// </summary>
    public static SpinnerType Pulse { get; } = new(
        ["\u2588", "\u2593", "\u2592", "\u2591", " ", "\u2591", "\u2592", "\u2593"],
        TimeSpan.FromMilliseconds(120));

    /// <summary>
    /// Points spinner with growing dots.
    /// </summary>
    public static SpinnerType Points { get; } = new(
        ["\u2219", "\u2219\u2219", "\u2219\u2219\u2219", " "],
        TimeSpan.FromMilliseconds(300));

    /// <summary>
    /// Globe spinner with rotating globe characters.
    /// </summary>
    public static SpinnerType Globe { get; } = new(
        ["\uD83C\uDF0D", "\uD83C\uDF0E", "\uD83C\uDF0F"],
        TimeSpan.FromMilliseconds(180));

    /// <summary>
    /// Moon spinner with moon phase characters.
    /// </summary>
    public static SpinnerType Moon { get; } = new(
        ["\uD83C\uDF11", "\uD83C\uDF12", "\uD83C\uDF13", "\uD83C\uDF14", "\uD83C\uDF15", "\uD83C\uDF16", "\uD83C\uDF17", "\uD83C\uDF18"],
        TimeSpan.FromMilliseconds(180));

    /// <summary>
    /// Hamburger spinner with stacking layers.
    /// </summary>
    public static SpinnerType Hamburger { get; } = new(
        ["\u2631", "\u2632", "\u2634"],
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Ellipsis spinner with growing dots.
    /// </summary>
    public static SpinnerType Ellipsis { get; } = new(
        ["", ".", "..", "..."],
        TimeSpan.FromMilliseconds(300));

    /// <summary>
    /// Monkey spinner with see/hear/speak no evil monkey faces.
    /// </summary>
    public static SpinnerType Monkey { get; } = new(
        ["\uD83D\uDE48", "\uD83D\uDE49", "\uD83D\uDE4A"],
        TimeSpan.FromMilliseconds(300));

    /// <summary>
    /// Meter spinner with progress-like block characters.
    /// </summary>
    public static SpinnerType Meter { get; } = new(
        ["\u2581", "\u2582", "\u2583", "\u2584", "\u2585", "\u2586", "\u2587", "\u2588", "\u2587", "\u2586", "\u2585", "\u2584", "\u2583", "\u2582"],
        TimeSpan.FromMilliseconds(100));
}
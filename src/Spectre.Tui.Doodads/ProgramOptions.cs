namespace Spectre.Tui.Doodads;

/// <summary>
/// Configuration for the TEA program runner.
/// </summary>
public sealed class ProgramOptions
{
    /// <summary>
    /// Gets or sets the terminal mode. Default is <see cref="FullscreenMode"/>.
    /// </summary>
    public ITerminalMode? TerminalMode { get; set; }

    /// <summary>
    /// Gets or sets the target frames per second. Default is 60.
    /// </summary>
    public int TargetFps { get; set; } = 60;

    /// <summary>
    /// Gets or sets an externally-managed terminal instance.
    /// When set, the program will use this terminal instead of creating its own.
    /// The caller is responsible for disposing the terminal.
    /// </summary>
    public ITerminal? Terminal { get; set; }

    /// <summary>
    /// Gets or sets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets the render pipeline stages applied to all rendering.
    /// Each stage wraps the previous surface, enabling operation-level middleware
    /// (e.g., color stripping, character fallback).
    /// </summary>
    public List<Func<IRenderSurface, IRenderSurface>> RenderPipeline { get; } = [];
}
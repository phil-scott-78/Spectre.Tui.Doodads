namespace Spectre.Tui.Doodads;

/// <summary>
/// The core TEA component contract. Doodads are interactive, stateful TUI components
/// that follow the Elm Architecture (Model-Update-View) pattern.
/// </summary>
/// <typeparam name="TSelf">The concrete doodad type (CRTP pattern).</typeparam>
public interface IDoodad<TSelf> : IRenderable
    where TSelf : IDoodad<TSelf>
{
    /// <summary>
    /// Initializes the doodad and returns an optional startup command.
    /// </summary>
    Command? Init();

    /// <summary>
    /// Processes a message and returns the updated model and an optional command.
    /// </summary>
    (TSelf Model, Command? Command) Update(Message message);

    /// <summary>
    /// Renders the doodad to the given surface.
    /// </summary>
    /// <param name="surface">The render surface to draw into.</param>
    void View(IRenderSurface surface);

    /// <inheritdoc />
    void IRenderable.Render(IRenderSurface surface) => View(surface);
}

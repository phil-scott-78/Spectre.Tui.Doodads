namespace Spectre.Tui.Doodads.Input;

/// <summary>
/// Provides keybinding information for help display.
/// </summary>
public interface IKeyMap
{
    /// <summary>
    /// Returns a concise set of bindings for short help display.
    /// </summary>
    IEnumerable<KeyBinding> ShortHelp();

    /// <summary>
    /// Returns grouped bindings for full help display.
    /// </summary>
    IEnumerable<IEnumerable<KeyBinding>> FullHelp();
}
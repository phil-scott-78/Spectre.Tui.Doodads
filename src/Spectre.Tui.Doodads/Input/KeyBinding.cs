namespace Spectre.Tui.Doodads.Input;

/// <summary>
/// A structured keybinding with help text.
/// </summary>
public record KeyBinding
{
    /// <summary>
    /// Gets the keys that activate this binding.
    /// </summary>
    public required IReadOnlyList<Key> Keys { get; init; }

    /// <summary>
    /// Gets the runes that further qualify a <see cref="Key.Char"/> binding.
    /// When non-empty and the key is <see cref="Key.Char"/>, the message rune must match one of these.
    /// </summary>
    public ImmutableArray<Rune> Runes { get; init; } = [];

    /// <summary>
    /// Gets the display text for the key (e.g., "↑/k").
    /// </summary>
    public string HelpKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets the help description (e.g., "move up").
    /// </summary>
    public string HelpDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this binding is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Returns true if the key message matches any of this binding's keys.
    /// </summary>
    public bool Matches(KeyMessage message)
    {
        if (!Enabled)
        {
            return false;
        }

        if (!Keys.Contains(message.Key))
        {
            return false;
        }

        if (message.Key == Key.Char && !Runes.IsDefaultOrEmpty)
        {
            return message.Runes.Length > 0 && Runes.Contains(message.Runes[0]);
        }

        return true;
    }

    /// <summary>
    /// Creates a binding for the given keys.
    /// </summary>
    public static KeyBinding For(params Key[] keys)
    {
        return new KeyBinding { Keys = keys };
    }

    /// <summary>
    /// Creates a binding for <see cref="Key.Char"/> with specific rune(s), optionally combined with other keys.
    /// </summary>
    public static KeyBinding ForRune(params Rune[] runes)
    {
        return new KeyBinding { Keys = [Key.Char], Runes = [.. runes] };
    }
}

/// <summary>
/// Extension methods for fluent KeyBinding configuration.
/// </summary>
public static class KeyBindingExtensions
{
    /// <summary>
    /// Sets the help text for this binding.
    /// </summary>
    public static KeyBinding WithHelp(this KeyBinding binding, string key, string description)
    {
        return binding with { HelpKey = key, HelpDescription = description };
    }

    /// <summary>
    /// Adds rune qualifiers for <see cref="Key.Char"/> matching.
    /// </summary>
    public static KeyBinding WithRunes(this KeyBinding binding, params Rune[] runes)
    {
        return binding with { Runes = [.. runes] };
    }

    /// <summary>
    /// Disables this binding.
    /// </summary>
    public static KeyBinding Disabled(this KeyBinding binding)
    {
        return binding with { Enabled = false };
    }
}
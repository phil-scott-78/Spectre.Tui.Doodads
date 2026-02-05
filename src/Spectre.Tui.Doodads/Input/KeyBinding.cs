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
    /// Gets a value indicating whether the Alt modifier must match.
    /// <c>null</c> means don't care; <c>true</c> means Alt must be held; <c>false</c> means Alt must not be held.
    /// </summary>
    public bool? Alt { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Ctrl modifier must match.
    /// <c>null</c> means don't care; <c>true</c> means Ctrl must be held; <c>false</c> means Ctrl must not be held.
    /// </summary>
    public bool? Ctrl { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Shift modifier must match.
    /// <c>null</c> means don't care; <c>true</c> means Shift must be held; <c>false</c> means Shift must not be held.
    /// </summary>
    public bool? Shift { get; init; }

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

        if (Alt is { } alt && message.Alt != alt)
        {
            return false;
        }

        if (Ctrl is { } ctrl && message.Ctrl != ctrl)
        {
            return false;
        }

        if (Shift is { } shift && message.Shift != shift)
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

    /// <summary>
    /// Creates a binding for <see cref="Key.Char"/> with specific character(s).
    /// </summary>
    public static KeyBinding ForChar(params char[] chars)
    {
        return new KeyBinding { Keys = [Key.Char], Runes = [.. chars.Select(c => new Rune(c))] };
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
    /// Requires or excludes the Alt modifier.
    /// </summary>
    public static KeyBinding WithAlt(this KeyBinding binding, bool value = true)
    {
        return binding with { Alt = value };
    }

    /// <summary>
    /// Requires or excludes the Ctrl modifier.
    /// </summary>
    public static KeyBinding WithCtrl(this KeyBinding binding, bool value = true)
    {
        return binding with { Ctrl = value };
    }

    /// <summary>
    /// Requires or excludes the Shift modifier.
    /// </summary>
    public static KeyBinding WithShift(this KeyBinding binding, bool value = true)
    {
        return binding with { Shift = value };
    }

    /// <summary>
    /// Requires that no modifiers (Alt, Ctrl, Shift) are held.
    /// </summary>
    public static KeyBinding WithoutModifiers(this KeyBinding binding)
    {
        return binding with { Alt = false, Ctrl = false, Shift = false };
    }

    /// <summary>
    /// Disables this binding.
    /// </summary>
    public static KeyBinding Disabled(this KeyBinding binding)
    {
        return binding with { Enabled = false };
    }
}
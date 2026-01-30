namespace Spectre.Tui.Doodads.Doodads.TextInput;

/// <summary>
/// Controls how text input characters are displayed.
/// </summary>
public enum EchoMode
{
    /// <summary>
    /// Characters are displayed as typed.
    /// </summary>
    Normal,

    /// <summary>
    /// Characters are masked with the echo character.
    /// </summary>
    Password,

    /// <summary>
    /// Characters are not displayed at all.
    /// </summary>
    None,
}
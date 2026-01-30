namespace Spectre.Tui.Doodads.Messages;

/// <summary>
/// Message indicating the terminal gained focus.
/// </summary>
public record FocusMessage : Message;

/// <summary>
/// Message indicating the terminal lost focus.
/// </summary>
public record BlurMessage : Message;
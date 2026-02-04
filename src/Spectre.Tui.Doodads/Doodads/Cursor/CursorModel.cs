namespace Spectre.Tui.Doodads.Doodads.Cursor;

/// <summary>
/// Manages cursor rendering and blink state for input components.
/// </summary>
public record CursorModel : IDoodad<CursorModel>
{
    private static int _nextId;

    /// <summary>
    /// Gets the cursor display mode.
    /// </summary>
    public CursorMode Mode { get; init; } = CursorMode.Blink;

    /// <summary>
    /// Gets the blink speed.
    /// </summary>
    public TimeSpan BlinkSpeed { get; init; } = TimeSpan.FromMilliseconds(530);

    /// <summary>
    /// Gets the cursor style (reverse video by default).
    /// </summary>
    public Appearance Style { get; init; } = new()
    {
        Decoration = Decoration.Invert,
    };

    /// <summary>
    /// Gets the style when blink is off.
    /// </summary>
    public Appearance TextStyle { get; init; } = Appearance.Plain;

    /// <summary>
    /// Gets a value indicating whether the cursor is focused.
    /// </summary>
    public bool Focused { get; init; }

    /// <summary>
    /// Gets a value indicating the current blink visibility state.
    /// </summary>
    public bool Visible { get; init; } = true;

    /// <summary>
    /// Gets the character under the cursor.
    /// </summary>
    internal string Character { get; init; } = " ";

    /// <summary>
    /// Gets the unique identifier for this cursor instance.
    /// </summary>
    internal int Id { get; init; } = Interlocked.Increment(ref _nextId);

    /// <summary>
    /// Gets the blink generation tag for stale tick detection.
    /// </summary>
    internal int Tag { get; init; }

    /// <summary>
    /// Gets a value indicating whether the cursor is idle (not recently active).
    /// When idle, the cursor blinks. When active (typing), it stays solid.
    /// </summary>
    internal bool IsIdle { get; init; } = true;

    /// <inheritdoc />
    public Command? Init()
    {
        if (Mode == CursorMode.Blink && Focused)
        {
            return BlinkCommand();
        }

        return null;
    }

    /// <inheritdoc />
    public (CursorModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case FocusMessage:
                return Focus();

            case BlurMessage:
                return Blur();

            case CursorBlinkMessage blink when blink.Id == Id && blink.Tag == Tag:
                var toggled = this with { Visible = !Visible };
                return (toggled, toggled.BlinkCommand());

            case CursorIdleMessage idle when idle.Id == Id && idle.Tag == Tag:
                var idleModel = this with { IsIdle = true };
                return (idleModel, idleModel.BlinkCommand());

            default:
                return (this, null);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        if (Mode == CursorMode.Hidden || !Focused)
        {
            surface.SetString(0, 0, Character, TextStyle);
            return;
        }

        if (Visible || Mode == CursorMode.Static)
        {
            surface.SetString(0, 0, Character, Style);
        }
        else
        {
            surface.SetString(0, 0, Character, TextStyle);
        }
    }

    /// <summary>
    /// Sets the cursor to focused state and starts blinking if applicable.
    /// </summary>
    public (CursorModel Model, Command? Command) Focus()
    {
        var focused = this with { Focused = true, Visible = true, Tag = Tag + 1 };
        var cmd = focused.Mode == CursorMode.Blink ? focused.BlinkCommand() : null;
        return (focused, cmd);
    }

    /// <summary>
    /// Sets the cursor to unfocused state.
    /// </summary>
    public (CursorModel Model, Command? Command) Blur()
    {
        return (this with { Focused = false, Tag = Tag + 1 }, null);
    }

    /// <summary>
    /// Sets the character under the cursor.
    /// </summary>
    public CursorModel SetChar(string character)
    {
        return this with { Character = character };
    }

    /// <summary>
    /// Sets the character under the cursor and the blink-off text style.
    /// </summary>
    public CursorModel SetChar(string character, Appearance textStyle)
    {
        return this with { Character = character, TextStyle = textStyle };
    }

    /// <summary>
    /// Resets the idle timer, keeping the cursor visible while typing.
    /// Called by input components when a key is pressed.
    /// </summary>
    public (CursorModel Model, Command? Command) ResetIdle()
    {
        if (Mode != CursorMode.Blink || !Focused)
        {
            return (this, null);
        }

        // Increment Tag to cancel pending blink/idle messages, set visible and not idle
        var model = this with { Visible = true, IsIdle = false, Tag = Tag + 1 };

        // Schedule transition to idle after BlinkSpeed timeout
        var id = model.Id;
        var tag = model.Tag;
        var cmd = Commands.Tick(BlinkSpeed, _ => new CursorIdleMessage { Id = id, Tag = tag });
        return (model, cmd);
    }

    private Command? BlinkCommand()
    {
        // Don't blink while actively typing (not idle)
        if (Mode == CursorMode.Blink && !IsIdle)
        {
            return null;
        }

        if (Mode == CursorMode.Blink && Focused)
        {
            var id = Id;
            var tag = Tag;
            return Commands.Tick(BlinkSpeed, _ => new CursorBlinkMessage { Id = id, Tag = tag });
        }

        return null;
    }
}

using Spectre.Tui.Doodads.Input;

namespace Spectre.Tui.Doodads.Doodads.Help;

/// <summary>
/// Auto-generates help text from an <see cref="IKeyMap"/>.
/// </summary>
public record HelpModel : IDoodad<HelpModel>
{
    /// <summary>
    /// Gets the minimum width for rendering.
    /// </summary>
    public int MinWidth { get; init; } = 80;

    /// <summary>
    /// Gets a value indicating whether to show full help.
    /// </summary>
    public bool ShowAll { get; init; }

    /// <summary>
    /// Gets the separator for short help mode.
    /// </summary>
    public string ShortSeparator { get; init; } = " • ";

    /// <summary>
    /// Gets the separator for full help mode.
    /// </summary>
    public string FullSeparator { get; init; } = "    ";

    /// <summary>
    /// Gets the ellipsis for truncated help.
    /// </summary>
    public string Ellipsis { get; init; } = "…";

    /// <summary>
    /// Gets the style for key text (used for both short and full unless overridden).
    /// </summary>
    public Appearance KeyStyle { get; init; } = new() { Decoration = Decoration.Bold };

    /// <summary>
    /// Gets the style for description text (used for both short and full unless overridden).
    /// </summary>
    public Appearance DescriptionStyle { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for separator text (used for both short and full unless overridden).
    /// </summary>
    public Appearance SeparatorStyle { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the style for key text in short help mode. Falls back to <see cref="KeyStyle"/> if null.
    /// </summary>
    public Appearance? ShortKeyStyle { get; init; }

    /// <summary>
    /// Gets the style for key text in full help mode. Falls back to <see cref="KeyStyle"/> if null.
    /// </summary>
    public Appearance? FullKeyStyle { get; init; }

    /// <summary>
    /// Gets the style for description text in short help mode. Falls back to <see cref="DescriptionStyle"/> if null.
    /// </summary>
    public Appearance? ShortDescriptionStyle { get; init; }

    /// <summary>
    /// Gets the style for description text in full help mode. Falls back to <see cref="DescriptionStyle"/> if null.
    /// </summary>
    public Appearance? FullDescriptionStyle { get; init; }

    /// <summary>
    /// Gets the style for separator text in short help mode. Falls back to <see cref="SeparatorStyle"/> if null.
    /// </summary>
    public Appearance? ShortSeparatorStyle { get; init; }

    /// <summary>
    /// Gets the style for separator text in full help mode. Falls back to <see cref="SeparatorStyle"/> if null.
    /// </summary>
    public Appearance? FullSeparatorStyle { get; init; }

    /// <summary>
    /// Gets the style for the ellipsis indicator.
    /// </summary>
    public Appearance EllipsisStyle { get; init; } = new() { Decoration = Decoration.Dim };

    /// <summary>
    /// Gets the key map to render help for.
    /// </summary>
    public IKeyMap? KeyMap { get; init; }

    /// <inheritdoc />
    public Command? Init() => null;

    /// <inheritdoc />
    public (HelpModel Model, Command? Command) Update(Message message) => message switch
    {
        KeyMessage { Runes.Length: > 0 } km when km.Runes[0] == new Rune('?')
            => (this with { ShowAll = !ShowAll }, null),
        WindowSizeMessage ws => (this with { MinWidth = ws.Width }, null),
        _ => (this, null),
    };

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        if (KeyMap is null)
        {
            return;
        }

        if (ShowAll)
        {
            RenderFull(surface);
        }
        else
        {
            RenderShort(surface);
        }
    }

    /// <summary>
    /// Sets the key map.
    /// </summary>
    public HelpModel SetKeyMap(IKeyMap keyMap)
    {
        return this with { KeyMap = keyMap };
    }

    /// <summary>
    /// Renders only the short help view to a surface.
    /// </summary>
    public void ShortHelpView(IRenderSurface surface)
    {
        if (KeyMap is null)
        {
            return;
        }

        RenderShort(surface);
    }

    /// <summary>
    /// Renders only the full help view to a surface.
    /// </summary>
    public void FullHelpView(IRenderSurface surface)
    {
        if (KeyMap is null)
        {
            return;
        }

        RenderFull(surface);
    }

    private void RenderShort(IRenderSurface surface)
    {
        var bindings = KeyMap!.ShortHelp().Where(b => b.Enabled).ToList();
        var x = 0;
        var width = Math.Max(0, surface.Viewport.Width);
        if (width == 0)
        {
            width = MinWidth;
        }

        var keyStyle = ShortKeyStyle ?? KeyStyle;
        var descStyle = ShortDescriptionStyle ?? DescriptionStyle;
        var sepStyle = ShortSeparatorStyle ?? SeparatorStyle;

        for (var i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            var entry = $"{binding.HelpKey} {binding.HelpDescription}";

            if (i > 0)
            {
                var sepWidth = ShortSeparator.Length;
                if (x + sepWidth + entry.Length > width)
                {
                    surface.SetString(x, 0, Ellipsis, EllipsisStyle);
                    return;
                }

                surface.SetString(x, 0, ShortSeparator, sepStyle);
                x += sepWidth;
            }
            else if (entry.Length > width)
            {
                surface.SetString(0, 0, Ellipsis, EllipsisStyle);
                return;
            }

            var pos = surface.SetString(x, 0, binding.HelpKey, keyStyle);
            x = pos.X;
            surface.SetString(x, 0, " ", Appearance.Plain);
            x++;
            pos = surface.SetString(x, 0, binding.HelpDescription, descStyle);
            x = pos.X;
        }
    }

    private void RenderFull(IRenderSurface surface)
    {
        var groups = KeyMap!.FullHelp().ToList();
        var y = 0;
        var keyStyle = FullKeyStyle ?? KeyStyle;
        var descStyle = FullDescriptionStyle ?? DescriptionStyle;
        var sepStyle = FullSeparatorStyle ?? SeparatorStyle;

        foreach (var group in groups)
        {
            var bindings = group.Where(b => b.Enabled).ToList();
            foreach (var binding in bindings)
            {
                if (y >= surface.Viewport.Height)
                {
                    return;
                }

                var pos = surface.SetString(0, y, binding.HelpKey, keyStyle);
                var x = pos.X;
                surface.SetString(x, y, FullSeparator, sepStyle);
                x += FullSeparator.Length;
                surface.SetString(x, y, binding.HelpDescription, descStyle);
                y++;
            }

            y++; // blank line between groups
        }
    }
}

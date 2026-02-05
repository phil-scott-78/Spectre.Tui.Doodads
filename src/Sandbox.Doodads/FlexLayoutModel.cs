using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// A flex layout demo that renders bordered boxes showing their dimensions.
/// </summary>
public record FlexLayoutModel : IDoodad<FlexLayoutModel>, ISizedRenderable
{
    public int MinWidth => 40;
    public int MinHeight => 10;

    /// <inheritdoc />
    public Command? Init() => null;

    /// <inheritdoc />
    public (FlexLayoutModel Model, Command? Command) Update(Message message) => message switch
    {
        KeyMessage { Key: Key.Escape } => (this, Commands.Quit()),
        KeyMessage { Key: Key.CtrlC } => (this, Commands.Quit()),
        KeyMessage { Char: 'q' } => (this, Commands.Quit()),
        _ => (this, null),
    };

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);

        var titleStyle = new Appearance { Decoration = Decoration.Bold };
        surface.SetString(0, 0, "Flex Layout Demo", titleStyle);

        var layoutHeight = Math.Max(0, height - 3); // reserve: title row, blank row, help row

        var layout = Flex.Column(gap: 0)
            .Add(new BoxWidget(Color.Blue), FlexSize.Fixed(3))
            .Add(Flex.Row(gap: 2)
                .Add(new BoxWidget(Color.Green), FlexSize.Fixed(20))
                .Add(new BoxWidget(Color.Yellow), FlexSize.Ratio(3))
                .Add(new BoxWidget(Color.Cyan), FlexSize.Ratio(1)),
              FlexSize.Fill())
            .Add(new BoxWidget(Color.Magenta), FlexSize.Fixed(1));

        if (layoutHeight > 0 && width > 0)
        {
            surface.Render(layout, new Rectangle(0, 2, width, layoutHeight));
        }

        var helpStyle = new Appearance { Decoration = Decoration.Dim };
        if (height > 0)
        {
            surface.SetString(0, height - 1, "q/Esc: quit", helpStyle);
        }
    }

    /// <summary>
    /// A bordered box widget that reads its allocated space from the viewport.
    /// </summary>
    private record BoxWidget(Color BorderColor) : ISizedRenderable
    {
        public int MinWidth => 3;
        public int MinHeight => 1;

        public void Render(IRenderSurface surface)
        {
            var width = Math.Max(0, surface.Viewport.Width);
            var height = Math.Max(0, surface.Viewport.Height);

            if (width < 2 || height < 1)
            {
                return;
            }

            var borderStyle = new Appearance { Foreground = BorderColor };
            var innerWidth = width - 2;
            var label = $"{width}x{height}";

            if (height == 1)
            {
                // Single row: draw as ─ label ─
                surface.SetString(0, 0, "\u2500", borderStyle);
                var pad = innerWidth - label.Length;
                var leftPad = pad / 2;
                if (leftPad > 0)
                {
                    surface.SetString(1, 0, new string('\u2500', leftPad), borderStyle);
                }

                surface.SetString(1 + leftPad, 0, label, borderStyle);
                var rightStart = 1 + leftPad + label.Length;
                var rightPad = width - 1 - rightStart;
                if (rightPad > 0)
                {
                    surface.SetString(rightStart, 0, new string('\u2500', rightPad), borderStyle);
                }

                surface.SetString(width - 1, 0, "\u2500", borderStyle);
                return;
            }

            // Top border: ┌────────┐
            var topInner = innerWidth > 0 ? new string('\u2500', innerWidth) : string.Empty;
            surface.SetString(0, 0, "\u250C" + topInner + "\u2510", borderStyle);

            // Middle rows
            for (var y = 1; y < height - 1; y++)
            {
                surface.SetString(0, y, "\u2502", borderStyle);

                if (y == height / 2 && innerWidth >= label.Length)
                {
                    // Center the label on the middle row
                    var leftSpace = (innerWidth - label.Length) / 2;
                    var rightSpace = innerWidth - label.Length - leftSpace;
                    if (leftSpace > 0)
                    {
                        surface.SetString(1, y, new string(' ', leftSpace), Appearance.Plain);
                    }

                    surface.SetString(1 + leftSpace, y, label, Appearance.Plain);
                    if (rightSpace > 0)
                    {
                        surface.SetString(1 + leftSpace + label.Length, y, new string(' ', rightSpace), Appearance.Plain);
                    }
                }

                surface.SetString(width - 1, y, "\u2502", borderStyle);
            }

            // Bottom border: └────────┘
            var bottomInner = innerWidth > 0 ? new string('\u2500', innerWidth) : string.Empty;
            surface.SetString(0, height - 1, "\u2514" + bottomInner + "\u2518", borderStyle);
        }
    }
}
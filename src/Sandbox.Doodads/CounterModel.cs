using System.Text;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// A minimal counter doodad to validate the TEA framework.
/// </summary>
public record CounterModel(int Count = 0) : IDoodad<CounterModel>, ISizedRenderable
{
    public int MinWidth => 30;
    public int MinHeight => 3;

    public Command? Init() => null;

    public (CounterModel Model, Command? Command) Update(Message message) => message switch
    {
        KeyMessage { Key: Key.Up } => (this with { Count = Count + 1 }, null),
        KeyMessage { Key: Key.Down } => (this with { Count = Count - 1 }, null),
        KeyMessage { Key: Key.Escape } => (this, Commands.Quit()),
        KeyMessage { Runes.Length: > 0 } k when k.Runes[0] == new Rune('q') => (this, Commands.Quit()),
        _ => (this, null),
    };

    public void View(IRenderSurface surface)
    {
        surface.Layout($"""
            Count: {Count}

            ↑/↓ to change, q/Esc to quit
            """);
    }
}
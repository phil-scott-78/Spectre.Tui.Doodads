using Spectre.Tui;
using Spectre.Tui.Doodads.Doodads.TextInput;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.AspNet;

public partial record AppMonitorModel
{
    private record FilterBar(TextInputModel Input) : ISizedRenderable
    {
        public int MinWidth => Input.MinWidth;
        public int MinHeight => 1;

        public void Render(IRenderSurface surface)
        {
            surface.Render(Input, new Rectangle(0, 0, surface.Viewport.Width, 1));
        }
    }
}
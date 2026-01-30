using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.AspNet;

public partial record AppMonitorModel
{
    private record HeaderBar(string Url, ViewMode ActiveView) : ISizedRenderable
    {
        public int MinWidth => 1;
        public int MinHeight => 1;

        public void Render(IRenderSurface surface)
        {
            var width = Math.Max(0, surface.Viewport.Width);
            if (width == 0)
            {
                return;
            }

            var activeLabel = ActiveView switch
            {
                ViewMode.Routes => "ROUTES",
                ViewMode.Config => "CONFIG",
                ViewMode.Services => "SERVICES",
                _ => "LOG",
            };

            const string Commands = "  /log  /routes  /config  /services";
            var leftContent = activeLabel.Length + 4 + Url.Length;
            var gap = width - leftContent - Commands.Length;

            if (gap > 0)
            {
                surface.Layout($"{activeLabel:bold}    {Url}{new string(' ', gap)}{Commands:dim}");
            }
            else
            {
                surface.Layout($"{activeLabel:bold}    {Url}");
            }
        }
    }
}
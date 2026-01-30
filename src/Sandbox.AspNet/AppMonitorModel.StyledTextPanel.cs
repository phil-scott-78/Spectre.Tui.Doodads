using Spectre.Tui;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.AspNet;

public partial record AppMonitorModel
{
    private record StyledTextPanel(
        IReadOnlyList<string> Lines,
        int ScrollOffset,
        Func<string, IReadOnlyList<TextSegment>> Colorize) : ISizedRenderable
    {
        public int MinWidth => 1;
        public int MinHeight => 1;

        public void Render(IRenderSurface surface)
        {
            var width = Math.Max(0, surface.Viewport.Width);
            var height = Math.Max(0, surface.Viewport.Height);

            for (var row = 0; row < height; row++)
            {
                var lineIndex = ScrollOffset + row;
                if (lineIndex < 0 || lineIndex >= Lines.Count)
                {
                    continue;
                }

                var line = Lines[lineIndex];
                var segments = Colorize(line);

                if (segments.Count == 0)
                {
                    var text = line.Length > width ? line[..width] : line;
                    surface.SetString(0, row, text, Appearance.Plain);
                    continue;
                }

                var x = 0;
                foreach (var segment in segments)
                {
                    if (x >= width)
                    {
                        break;
                    }

                    var text = segment.Text;
                    var remaining = width - x;
                    if (text.Length > remaining)
                    {
                        text = text[..remaining];
                    }

                    surface.SetString(x, row, text, segment.Style);
                    x += text.Length;
                }
            }
        }
    }
}
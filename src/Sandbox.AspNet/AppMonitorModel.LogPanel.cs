using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.AspNet;

public partial record AppMonitorModel
{
    private record LogPanel(
        IReadOnlyList<LogEntry> Entries,
        IReadOnlyList<VisualLine> VisualLines,
        int ScrollOffset,
        int MessageColumnX,
        int MaxCategoryWidth) : ISizedRenderable
    {
        public int MinWidth => 1;
        public int MinHeight => 1;

        public void Render(IRenderSurface surface)
        {
            var width = Math.Max(0, surface.Viewport.Width);
            var height = Math.Max(0, surface.Viewport.Height);
            var msgWidth = Math.Max(1, width - MessageColumnX);

            for (var row = 0; row < height; row++)
            {
                var lineIndex = ScrollOffset + row;
                if (lineIndex < 0 || lineIndex >= VisualLines.Count)
                {
                    continue;
                }

                var vline = VisualLines[lineIndex];
                var entry = Entries[vline.EntryIndex];

                switch (vline.Kind)
                {
                    case VisualLineKind.EntryFirstLine:
                        RenderEntryFirstLine(surface, entry, row, msgWidth);
                        break;
                    case VisualLineKind.MessageContinuation:
                        RenderMessageContinuation(surface, entry, vline.LineWithinEntry, row, msgWidth);
                        break;
                    case VisualLineKind.ExceptionLine:
                        RenderExceptionLine(surface, entry, vline.LineWithinEntry, row, msgWidth);
                        break;
                }
            }
        }

        private void RenderEntryFirstLine(IRenderSurface surface, LogEntry entry, int row, int msgWidth)
        {
            var x = 0;
            var dimStyle = new Appearance { Decoration = Decoration.Dim };

            // Timestamp: HH:mm:ss.fff (12 chars)
            var timestamp = entry.Timestamp.ToString("HH:mm:ss.fff");
            surface.SetString(x, row, timestamp, dimStyle);
            x += TimestampWidth + ColumnGap;

            // Level: color-coded, 5 chars padded
            var (levelText, levelStyle) = FormatLevel(entry.Level);
            surface.SetString(x, row, levelText, levelStyle);
            x += LevelWidth + ColumnGap;

            // Category: dim, padded/truncated to MaxCategoryWidth
            var category = entry.Category.Length > MaxCategoryWidth
                ? entry.Category[..MaxCategoryWidth]
                : entry.Category.PadRight(MaxCategoryWidth);
            surface.SetString(x, row, category, dimStyle);

            // Message: first chunk at MessageColumnX
            if (msgWidth > 0 && entry.Message.Length > 0)
            {
                var chunk = entry.Message.Length > msgWidth
                    ? entry.Message[..msgWidth]
                    : entry.Message;
                surface.SetString(MessageColumnX, row, chunk, Appearance.Plain);
            }
        }

        private void RenderMessageContinuation(IRenderSurface surface, LogEntry entry, int lineWithinEntry, int row, int msgWidth)
        {
            var start = lineWithinEntry * msgWidth;
            if (start >= entry.Message.Length)
            {
                return;
            }

            var end = Math.Min(start + msgWidth, entry.Message.Length);
            var chunk = entry.Message[start..end];
            surface.SetString(MessageColumnX, row, chunk, Appearance.Plain);
        }

        private void RenderExceptionLine(IRenderSurface surface, LogEntry entry, int lineWithinEntry, int row, int msgWidth)
        {
            if (entry.Exception is null)
            {
                return;
            }

            var exLines = entry.Exception.Split('\n');
            if (lineWithinEntry >= exLines.Length)
            {
                return;
            }

            var line = exLines[lineWithinEntry].TrimEnd('\r');
            var dimRedStyle = new Appearance { Foreground = Color.Red, Decoration = Decoration.Dim };

            if (line.Length > msgWidth)
            {
                line = line[..msgWidth];
            }

            surface.SetString(MessageColumnX, row, line, dimRedStyle);
        }

        private static (string Text, Appearance Style) FormatLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => ("TRACE", new Appearance { Decoration = Decoration.Dim }),
                LogLevel.Debug => ("DEBUG", new Appearance { Decoration = Decoration.Dim }),
                LogLevel.Information => ("INFO ", new Appearance { Foreground = Color.Green }),
                LogLevel.Warning => ("WARN ", new Appearance { Foreground = Color.Yellow }),
                LogLevel.Error => ("ERROR", new Appearance { Foreground = Color.Red }),
                LogLevel.Critical => ("CRIT ", new Appearance { Foreground = Color.Red, Decoration = Decoration.Bold }),
                _ => ("?????", Appearance.Plain),
            };
        }
    }
}
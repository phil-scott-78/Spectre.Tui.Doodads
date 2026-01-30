using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.TextInput;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.AspNet;

/// <summary>
/// A fullscreen TUI log viewer doodad with scrolling, filtering, and auto-tail.
/// </summary>
public partial record AppMonitorModel : IDoodad<AppMonitorModel>
{
    private const int TimestampWidth = 12; // "HH:mm:ss.fff"
    private const int LevelWidth = 5; // "TRACE", "DEBUG", "INFO ", "WARN ", "ERROR", "CRIT "
    private const int ColumnGap = 1;

    private static readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Gets the base URL of the ASP.NET application.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the log store to poll for entries.
    /// </summary>
    public required LogStore Store { get; init; }

    /// <summary>
    /// Gets all captured log entries.
    /// </summary>
    public IReadOnlyList<LogEntry> AllEntries { get; init; } = [];

    /// <summary>
    /// Gets the filtered log entries currently displayed.
    /// </summary>
    public IReadOnlyList<LogEntry> FilteredEntries { get; init; } = [];

    /// <summary>
    /// Gets the last observed store version.
    /// </summary>
    public int LastStoreVersion { get; init; }

    /// <summary>
    /// Gets the scroll offset (index of the first visible visual line).
    /// </summary>
    public int ScrollOffset { get; init; }

    /// <summary>
    /// Gets a value indicating whether auto-scroll (tail mode) is active.
    /// </summary>
    public bool AutoScroll { get; init; } = true;

    /// <summary>
    /// Gets the text input model used for the filter bar.
    /// </summary>
    public TextInputModel FilterInput { get; init; } = new()
    {
        Prompt = "> ",
        Placeholder = "filter or /log /routes /config",
        Focused = true,
        MinWidth = 80,
        PromptStyle = Appearance.Plain with
        {
            Foreground = Color.Purple
        },
    };

    /// <summary>
    /// Gets the current terminal width.
    /// </summary>
    public int TerminalWidth { get; init; } = 120;

    /// <summary>
    /// Gets the current terminal height.
    /// </summary>
    public int TerminalHeight { get; init; } = 24;

    /// <summary>
    /// Gets the currently active view.
    /// </summary>
    public ViewMode ActiveView { get; init; } = ViewMode.Log;

    /// <summary>
    /// Gets all route lines fetched from the endpoint data sources.
    /// </summary>
    public IReadOnlyList<string> AllRouteLines { get; init; } = [];

    /// <summary>
    /// Gets the filtered route lines currently displayed.
    /// </summary>
    public IReadOnlyList<string> FilteredRouteLines { get; init; } = [];

    /// <summary>
    /// Gets the scroll offset for the routes view.
    /// </summary>
    public int RoutesScrollOffset { get; init; }

    /// <summary>
    /// Gets all configuration lines fetched from the config debug view.
    /// </summary>
    public IReadOnlyList<string> AllConfigLines { get; init; } = [];

    /// <summary>
    /// Gets the filtered configuration lines currently displayed.
    /// </summary>
    public IReadOnlyList<string> FilteredConfigLines { get; init; } = [];

    /// <summary>
    /// Gets the scroll offset for the config view.
    /// </summary>
    public int ConfigScrollOffset { get; init; }

    /// <summary>
    /// Gets the delegate that provides route endpoint data.
    /// </summary>
    public Func<IReadOnlyList<string>>? RoutesProvider { get; init; }

    /// <summary>
    /// Gets all service registration lines.
    /// </summary>
    public IReadOnlyList<string> AllServiceLines { get; init; } = [];

    /// <summary>
    /// Gets the filtered service registration lines currently displayed.
    /// </summary>
    public IReadOnlyList<string> FilteredServiceLines { get; init; } = [];

    /// <summary>
    /// Gets the scroll offset for the services view.
    /// </summary>
    public int ServicesScrollOffset { get; init; }

    /// <summary>
    /// Gets the delegate that provides service registration data.
    /// </summary>
    public Func<IReadOnlyList<string>>? ServicesProvider { get; init; }

    /// <summary>
    /// Gets the delegate that provides configuration debug output.
    /// </summary>
    public Func<string>? ConfigProvider { get; init; }

    /// <summary>
    /// Gets the flat list of visual lines computed from filtered entries.
    /// </summary>
    internal IReadOnlyList<VisualLine> VisualLines { get; init; } = [];

    /// <summary>
    /// Gets the X position where the message column starts.
    /// </summary>
    public int MessageColumnX { get; init; }

    /// <summary>
    /// Gets the max category width used in the current layout.
    /// </summary>
    public int MaxCategoryWidth { get; init; }

    /// <summary>
    /// Gets the terminal width used for the last visual line computation.
    /// </summary>
    public int LayoutWidth { get; init; }

    /// <inheritdoc />
    public Command? Init()
    {
        return Commands.Batch(
            FilterInput.Init(),
            TickCommand());
    }

    /// <inheritdoc />
    public (AppMonitorModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case KeyMessage { Key: Key.Escape }:
            case KeyMessage { Key: Key.CtrlC }:
                return (this, Commands.Quit());

            case KeyMessage { Key: Key.Enter }:
                return HandleEnter();

            case KeyMessage { Key: Key.Up }:
                return ActiveScrollUp(1);

            case KeyMessage { Key: Key.Down }:
                return ActiveScrollDown(1);

            case KeyMessage { Key: Key.PageUp }:
                return ActiveScrollUp(ViewportHeight);

            case KeyMessage { Key: Key.PageDown }:
                return ActiveScrollDown(ViewportHeight);

            case KeyMessage { Key: Key.Home, Ctrl: true }:
                return ActiveScrollToTop();

            case KeyMessage { Key: Key.End, Ctrl: true }:
                return ActiveScrollToBottom();

            case WindowSizeMessage ws:
                var resized = this with
                {
                    TerminalWidth = ws.Width,
                    TerminalHeight = ws.Height
                };
                return (resized.RecomputeLayout(), null);

            case AppMonitorTick:
                return HandleTick();

            case KeyMessage km:
                return HandleFilterInput(km);

            default:
                var (updatedInput, inputCmd) = FilterInput.Update(message);
                return (this with
                {
                    FilterInput = updatedInput
                }, inputCmd);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var height = Math.Max(0, surface.Viewport.Height);
        if (width == 0 || height == 0)
        {
            return;
        }

        ISizedRenderable middlePanel = ActiveView switch
        {
            ViewMode.Routes => new StyledTextPanel(FilteredRouteLines, RoutesScrollOffset,
                LineColorizers.ColorizeRouteLine),
            ViewMode.Config => new StyledTextPanel(FilteredConfigLines, ConfigScrollOffset,
                LineColorizers.ColorizeConfigLine),
            ViewMode.Services => new StyledTextPanel(FilteredServiceLines, ServicesScrollOffset,
                LineColorizers.ColorizeServiceLine),
            _ => new LogPanel(FilteredEntries, VisualLines, ScrollOffset, MessageColumnX, MaxCategoryWidth),
        };

        var layout = Flex.Column()
            .Add(new Border()
                {
                    Content = new HeaderBar(BaseUrl, ActiveView),
                    ShowLeft = false,
                    ShowRight = false,
                    ShowTop = false,
                    Style = Appearance.Plain with
                    {
                        Decoration = Decoration.Dim,
                    },
                }, FlexSize.Fixed(2))
            .Add(middlePanel, FlexSize.Fill())
            .Add(new Border
                {
                    Content = new FilterBar(FilterInput),
                    ShowLeft = false,
                    ShowRight = false,
                    Style = Appearance.Plain with
                    {
                        Decoration = Decoration.Dim,
                    },
                }, FlexSize.Fixed(3));

        surface.Render(layout, new Rectangle(0, 0, width, height));
    }

    private int ViewportHeight => Math.Max(1, TerminalHeight - 5);

    private int MaxScrollOffset => Math.Max(0, VisualLines.Count - ViewportHeight);

    private int ActiveLineCount => ActiveView switch
    {
        ViewMode.Routes => FilteredRouteLines.Count,
        ViewMode.Config => FilteredConfigLines.Count,
        ViewMode.Services => FilteredServiceLines.Count,
        _ => VisualLines.Count,
    };

    private int ActiveScrollOffset => ActiveView switch
    {
        ViewMode.Routes => RoutesScrollOffset,
        ViewMode.Config => ConfigScrollOffset,
        ViewMode.Services => ServicesScrollOffset,
        _ => ScrollOffset,
    };

    private int ActiveMaxScrollOffset => Math.Max(0, ActiveLineCount - ViewportHeight);

    private AppMonitorModel WithActiveScrollOffset(int offset, bool? logAutoScroll = null)
    {
        return ActiveView switch
        {
            ViewMode.Routes => this with
            {
                RoutesScrollOffset = offset
            },
            ViewMode.Config => this with
            {
                ConfigScrollOffset = offset
            },
            ViewMode.Services => this with
            {
                ServicesScrollOffset = offset
            },
            _ => this with
            {
                ScrollOffset = offset,
                AutoScroll = logAutoScroll ?? AutoScroll
            },
        };
    }

    private (AppMonitorModel Model, Command? Command) ActiveScrollUp(int lines)
    {
        var newOffset = Math.Max(0, ActiveScrollOffset - lines);
        return (WithActiveScrollOffset(newOffset, logAutoScroll: false), null);
    }

    private (AppMonitorModel Model, Command? Command) ActiveScrollDown(int lines)
    {
        var max = ActiveMaxScrollOffset;
        var newOffset = Math.Min(max, ActiveScrollOffset + lines);
        return (WithActiveScrollOffset(newOffset, logAutoScroll: newOffset >= max), null);
    }

    private (AppMonitorModel Model, Command? Command) ActiveScrollToTop()
    {
        return (WithActiveScrollOffset(0, logAutoScroll: false), null);
    }

    private (AppMonitorModel Model, Command? Command) ActiveScrollToBottom()
    {
        return (WithActiveScrollOffset(ActiveMaxScrollOffset, logAutoScroll: true), null);
    }

    private (AppMonitorModel Model, Command? Command) HandleEnter()
    {
        var text = FilterInput.GetValue().Trim();
        return text.ToLowerInvariant() switch
        {
            "/log" => SwitchToView(ViewMode.Log),
            "/routes" => SwitchToView(ViewMode.Routes),
            "/config" => SwitchToView(ViewMode.Config),
            "/services" => SwitchToView(ViewMode.Services),
            _ => (this, null),
        };
    }

    private (AppMonitorModel Model, Command? Command) SwitchToView(ViewMode view)
    {
        var model = this with
        {
            ActiveView = view,
            FilterInput = FilterInput.Reset() with
            {
                Placeholder = "filter or /log /routes /config /services",
            },
        };

        switch (view)
        {
            case ViewMode.Routes:
                var routeLines = model.RoutesProvider?.Invoke() ?? [];
                model = model with
                {
                    AllRouteLines = routeLines,
                    FilteredRouteLines = routeLines,
                    RoutesScrollOffset = 0,
                };
                break;
            case ViewMode.Config:
                var configText = model.ConfigProvider?.Invoke() ?? string.Empty;
                IReadOnlyList<string> configLines = configText.Length > 0
                    ? configText.Split('\n')
                        .Select(l => l.TrimEnd('\r'))
                        .ToList()
                    : [];
                model = model with
                {
                    AllConfigLines = configLines,
                    FilteredConfigLines = configLines,
                    ConfigScrollOffset = 0,
                };
                break;
            case ViewMode.Services:
                var serviceLines = model.ServicesProvider?.Invoke() ?? [];
                model = model with
                {
                    AllServiceLines = serviceLines,
                    FilteredServiceLines = serviceLines,
                    ServicesScrollOffset = 0,
                };
                break;
            default:
                var filterText = model.FilterInput.GetValue();
                model = model with
                {
                    FilteredEntries = ApplyFilter(model.AllEntries, filterText),
                    AutoScroll = true,
                };
                model = model.RecomputeLayout();
                model = model with
                {
                    ScrollOffset = model.MaxScrollOffset
                };
                break;
        }

        return (model, null);
    }

    private (AppMonitorModel Model, Command? Command) HandleTick()
    {
        var currentVersion = Store.Version;
        if (currentVersion == LastStoreVersion)
        {
            return (this, TickCommand());
        }

        var snapshot = Store.GetSnapshot();

        var model = this with
        {
            AllEntries = snapshot,
            LastStoreVersion = currentVersion,
        };

        if (ActiveView == ViewMode.Log)
        {
            var filterText = FilterInput.GetValue();
            var filtered = ApplyFilter(snapshot, filterText);
            model = model with
            {
                FilteredEntries = filtered
            };
            model = model.RecomputeLayout();

            if (model.AutoScroll)
            {
                model = model with
                {
                    ScrollOffset = model.MaxScrollOffset
                };
            }
        }

        return (model, TickCommand());
    }

    private (AppMonitorModel Model, Command? Command) HandleFilterInput(KeyMessage km)
    {
        var oldValue = FilterInput.GetValue();
        var (updatedInput, inputCmd) = FilterInput.Update(km);
        var newValue = updatedInput.GetValue();

        var model = this with
        {
            FilterInput = updatedInput
        };

        if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            switch (ActiveView)
            {
                case ViewMode.Log:
                    var filtered = ApplyFilter(AllEntries, newValue);
                    model = model with
                    {
                        FilteredEntries = filtered,
                        AutoScroll = true,
                    };
                    model = model.RecomputeLayout();
                    model = model with
                    {
                        ScrollOffset = model.MaxScrollOffset
                    };
                    break;

                case ViewMode.Routes:
                    model = model with
                    {
                        FilteredRouteLines = ApplyTextFilter(AllRouteLines, newValue),
                        RoutesScrollOffset = 0,
                    };
                    break;

                case ViewMode.Config:
                    model = model with
                    {
                        FilteredConfigLines = ApplyTextFilter(AllConfigLines, newValue),
                        ConfigScrollOffset = 0,
                    };
                    break;

                case ViewMode.Services:
                    model = model with
                    {
                        FilteredServiceLines = ApplyTextFilter(AllServiceLines, newValue),
                        ServicesScrollOffset = 0,
                    };
                    break;
            }
        }

        return (model, inputCmd);
    }

    private AppMonitorModel RecomputeLayout()
    {
        var (visualLines, msgColX, maxCatWidth) = ComputeVisualLines(FilteredEntries, TerminalWidth);
        return this with
        {
            VisualLines = visualLines,
            MessageColumnX = msgColX,
            MaxCategoryWidth = maxCatWidth,
            LayoutWidth = TerminalWidth,
        };
    }

    private static (IReadOnlyList<VisualLine> Lines, int MessageColumnX, int MaxCategoryWidth) ComputeVisualLines(
        IReadOnlyList<LogEntry> entries, int terminalWidth)
    {
        if (entries.Count == 0 || terminalWidth <= 0)
        {
            return ([], 0, 0);
        }

        // Find max category width, capped at terminalWidth / 3
        var maxCatCap = Math.Max(1, terminalWidth / 3);
        var maxCatWidth = 0;
        foreach (var entry in entries)
        {
            var catLen = entry.Category.Length;
            if (catLen > maxCatWidth)
            {
                maxCatWidth = catLen;
            }
        }

        maxCatWidth = Math.Min(maxCatWidth, maxCatCap);

        var msgColX = TimestampWidth + ColumnGap + LevelWidth + ColumnGap + maxCatWidth + ColumnGap;
        var msgWidth = Math.Max(1, terminalWidth - msgColX);

        var lines = new List<VisualLine>();
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            // First line of the entry
            lines.Add(new VisualLine(i, VisualLineKind.EntryFirstLine, 0));

            // Message continuation lines (if message wraps beyond first line)
            var msgLen = entry.Message.Length;
            if (msgLen > msgWidth)
            {
                var continuationCount = (int)Math.Ceiling((double)msgLen / msgWidth) - 1;
                for (var c = 0; c < continuationCount; c++)
                {
                    lines.Add(new VisualLine(i, VisualLineKind.MessageContinuation, c + 1));
                }
            }

            // Exception lines
            if (!string.IsNullOrEmpty(entry.Exception))
            {
                var exLines = entry.Exception.Split('\n');
                for (var e = 0; e < exLines.Length; e++)
                {
                    lines.Add(new VisualLine(i, VisualLineKind.ExceptionLine, e));
                }
            }
        }

        return (lines, msgColX, maxCatWidth);
    }

    private static IReadOnlyList<LogEntry> ApplyFilter(IReadOnlyList<LogEntry> entries, string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return entries;
        }

        var result = new List<LogEntry>();
        foreach (var entry in entries)
        {
            if (entry.Level.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
                || entry.Category.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || entry.Message.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || (entry.Exception?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true))
            {
                result.Add(entry);
            }
        }

        return result;
    }

    private static IReadOnlyList<string> ApplyTextFilter(IReadOnlyList<string> lines, string filter)
    {
        return string.IsNullOrEmpty(filter)
            ? lines
            : lines.Where(line => line.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static Command TickCommand()
    {
        return Commands.Tick(_pollInterval, _ => new AppMonitorTick());
    }
}
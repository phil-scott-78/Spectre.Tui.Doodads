using System.Text;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Doodads.Progress;
using Spectre.Tui.Doodads.Doodads.Spinner;
using Spectre.Tui.Doodads.Doodads.Stopwatch;
using Spectre.Tui.Doodads.Doodads.Timer;
using Spectre.Tui.Doodads.Layout;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// A dashboard example composing spinner, progress, stopwatch, and timer doodads.
/// </summary>
public record DashboardModel : IDoodad<DashboardModel>, ISizedRenderable
{
    public int MinWidth => 64;
    public int MinHeight => 14;

    private const int PanelWidth = 30;

    /// <summary>
    /// Gets the spinner doodad.
    /// </summary>
    public SpinnerModel Spinner { get; init; } = new()
    {
        Spinner = SpinnerType.Dot,
        MinWidth = PanelWidth - 4,
    };

    /// <summary>
    /// Gets the progress bar doodad.
    /// </summary>
    public ProgressModel Progress { get; init; } = new()
    {
        MinWidth = 25,
        ShowPercentage = true,
    };

    /// <summary>
    /// Gets the stopwatch doodad.
    /// </summary>
    public StopwatchModel Stopwatch { get; init; } = new()
    {
        MinWidth = PanelWidth - 4,
    };

    /// <summary>
    /// Gets the countdown timer doodad (60 seconds).
    /// </summary>
    public TimerModel Timer { get; init; } = new()
    {
        Timeout = TimeSpan.FromSeconds(60),
        MinWidth = PanelWidth - 4,
    };

    /// <summary>
    /// Gets a value indicating whether auto-progress is enabled.
    /// </summary>
    public bool AutoProgress { get; init; } = true;

    /// <inheritdoc />
    public Command? Init()
    {
        var spinnerCmd = Spinner.Init();
        var (startedTimer, timerCmd) = Timer.Start();
        var progressTickCmd = AutoProgressCommand();

        return Commands.Batch(
            spinnerCmd,
            timerCmd,
            progressTickCmd,
            Commands.Message(new DashboardTimerStarted { Timer = startedTimer }));
    }

    /// <inheritdoc />
    public (DashboardModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case KeyMessage { Key: Key.Escape }:
            case KeyMessage { Key: Key.CtrlC }:
                return (this, Commands.Quit());

            case KeyMessage { Key: Key.Char, Runes.Length: > 0 } km when km.Runes[0] == new Rune('q'):
                return (this, Commands.Quit());

            case KeyMessage { Key: Key.Space }:
                var (toggledSw, swCmd) = Stopwatch.Toggle();
                return (this with { Stopwatch = toggledSw }, swCmd);

            case KeyMessage { Key: Key.Char, Runes.Length: > 0 } km when km.Runes[0] == new Rune('+'):
                var (incrProgress, incrCmd) = Progress.IncrPercent(0.05);
                return (this with { Progress = incrProgress }, incrCmd);

            case KeyMessage { Key: Key.Char, Runes.Length: > 0 } km when km.Runes[0] == new Rune('-'):
                var (decrProgress, decrCmd) = Progress.IncrPercent(-0.05);
                return (this with { Progress = decrProgress }, decrCmd);

            case DashboardTimerStarted started:
                return (this with { Timer = started.Timer }, null);

            case DashboardAutoProgressTick when AutoProgress:
                var currentPercent = Progress.Percent;
                if (currentPercent >= 1.0)
                {
                    return (this with { AutoProgress = false }, null);
                }

                var (newProgress, progressCmd) = Progress.IncrPercent(0.01);
                return (this with { Progress = newProgress },
                    Commands.Batch(progressCmd, AutoProgressCommand()));

            default:
                return ForwardToChildren(message);
        }
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var swStatus = Stopwatch.Running ? "(running)" : "(stopped)";
        var tmStatus = Timer.Running ? "(running)" : "(stopped)";

        var topRow = Flex.Row(minWidth: width, minHeight: 4, gap: 2)
            .Add(Spinner, FlexSize.Fill())
            .Add(Progress, FlexSize.Fill());

        var bottomRow = Flex.Row(minWidth: width, minHeight: 4, gap: 2)
            .Add(Stopwatch, FlexSize.Fill())
            .Add(Timer, FlexSize.Fill());

        var separator = new string('-', width);
        var spinnerLabel = new Label("Spinner", "text-blue");
        var progressLabel = new Label("Progress", "text-green");
        var swLabel = new Label($"Stopwatch {swStatus}", "text-blue");
        var timerLabel = new Label($"Timer (60s) {tmStatus}", "text-green");

        surface.Layout($"""
            {"Dashboard":text-blue}
            {separator}

            {spinnerLabel}                       {progressLabel}
            {topRow}

            {swLabel}                  {timerLabel}
            {bottomRow}

            {"Space:toggle stopwatch  +/-:adjust progress  q/Esc:quit":dim}
            """);
    }

    private (DashboardModel Model, Command? Command) ForwardToChildren(Message message)
    {
        return this
            .Forward(message, m => m.Spinner, (m, v) => m with { Spinner = v })
            .Forward(message, m => m.Progress, (m, v) => m with { Progress = v })
            .Forward(message, m => m.Stopwatch, (m, v) => m with { Stopwatch = v })
            .Forward(message, m => m.Timer, (m, v) => m with { Timer = v });
    }

    private static Command AutoProgressCommand()
    {
        return Commands.Tick(
            TimeSpan.FromMilliseconds(500),
            _ => new DashboardAutoProgressTick());
    }
}

/// <summary>
/// Internal message to carry the started timer back after Init.
/// </summary>
internal record DashboardTimerStarted : Message
{
    public required TimerModel Timer { get; init; }
}

/// <summary>
/// Internal message for auto-incrementing progress.
/// </summary>
internal record DashboardAutoProgressTick : Message;
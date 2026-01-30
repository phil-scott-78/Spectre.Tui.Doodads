using System.Text;
using NetPace.Core;
using NetPace.Core.Clients.Ookla;
using Spectre.Console;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Doodads.Progress;
using Spectre.Tui.Doodads.Doodads.Spinner;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Doodads;

/// <summary>
/// Phase state machine for the speed test.
/// </summary>
public enum SpeedTestPhase
{
    Init,
    FindingServers,
    TestingLatency,
    Downloading,
    Uploading,
    Complete,
    Error,
}

/// <summary>
/// Thread-safe bridge between IProgress callbacks and TEA polling.
/// </summary>
internal sealed class SpeedTestBridge : IProgress<SpeedTestProgress>
{
    private readonly Lock _lock = new();
    private SpeedTestProgress? _latest;
    private SpeedTestResult? _result;
    private Exception? _error;

    public SpeedTestProgress? Latest { get { lock (_lock) { return _latest; } } }
    public SpeedTestResult? Result { get { lock (_lock) { return _result; } } }
    public Exception? Error { get { lock (_lock) { return _error; } } }
    public bool IsComplete { get { lock (_lock) { return _result is not null || _error is not null; } } }

    public void Report(SpeedTestProgress value)
    {
        lock (_lock) { _latest = value; }
    }

    public void SetResult(SpeedTestResult result)
    {
        lock (_lock) { _result = result; }
    }

    public void SetError(Exception error)
    {
        lock (_lock) { _error = error; }
    }
}

/// <summary>
/// Message carrying discovered servers.
/// </summary>
internal record ServersFoundMessage : Message
{
    public required ISpeedTestService Service { get; init; }
    public required IServer[] Servers { get; init; }
}

/// <summary>
/// Message carrying the fastest server latency result.
/// </summary>
internal record FastestServerFoundMessage : Message
{
    public required ISpeedTestService Service { get; init; }
    public required LatencyTestResult LatencyResult { get; init; }
}

/// <summary>
/// Periodic poll tick for download/upload progress.
/// </summary>
internal record SpeedTestPollMessage : Message;

/// <summary>
/// Error message from any async speed test operation.
/// </summary>
internal record SpeedTestErrorMessage : Message
{
    public required string ErrorText { get; init; }
}

/// <summary>
/// A network speed test doodad with colorful TUI interface.
/// </summary>
public record SpeedTestModel : IDoodad<SpeedTestModel>, ISizedRenderable
{
    public int MinWidth => 50;
    public int MinHeight => 16;

    /// <summary>
    /// Gets the current phase.
    /// </summary>
    public SpeedTestPhase Phase { get; init; } = SpeedTestPhase.Init;

    /// <summary>
    /// Gets the animated spinner for loading phases.
    /// </summary>
    public SpinnerModel Spinner { get; init; } = new()
    {
        Spinner = SpinnerType.Dot,
        MinWidth = 2,
    };

    /// <summary>
    /// Gets the download progress bar.
    /// </summary>
    public ProgressModel DownloadProgress { get; init; } = new ProgressModel
    {
        MinWidth = 40,
        ShowPercentage = true,
    }.WithScaledGradient(new Color(0, 255, 100), new Color(0, 255, 255));

    /// <summary>
    /// Gets the upload progress bar.
    /// </summary>
    public ProgressModel UploadProgress { get; init; } = new ProgressModel
    {
        MinWidth = 40,
        ShowPercentage = true,
    }.WithScaledGradient(new Color(80, 120, 255), new Color(200, 80, 255));

    /// <summary>
    /// Gets the speed test service instance.
    /// </summary>
    internal ISpeedTestService? Service { get; init; }

    /// <summary>
    /// Gets the selected server.
    /// </summary>
    internal IServer? Server { get; init; }

    /// <summary>
    /// Gets the server sponsor name.
    /// </summary>
    public string? ServerName { get; init; }

    /// <summary>
    /// Gets the server location.
    /// </summary>
    public string? ServerLocation { get; init; }

    /// <summary>
    /// Gets the measured latency in milliseconds.
    /// </summary>
    public long Latency { get; init; }

    /// <summary>
    /// Gets the download progress bridge.
    /// </summary>
    internal SpeedTestBridge? DownloadBridge { get; init; }

    /// <summary>
    /// Gets the upload progress bridge.
    /// </summary>
    internal SpeedTestBridge? UploadBridge { get; init; }

    /// <summary>
    /// Gets the download result.
    /// </summary>
    internal SpeedTestResult? DownloadResult { get; init; }

    /// <summary>
    /// Gets the upload result.
    /// </summary>
    internal SpeedTestResult? UploadResult { get; init; }

    /// <summary>
    /// Gets the current live speed string during download.
    /// </summary>
    public string DownloadSpeedText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current live speed string during upload.
    /// </summary>
    public string UploadSpeedText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the error message, if any.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <inheritdoc />
    public Command? Init()
    {
        return Commands.Sequence(
            Spinner.Init(),
            Commands.Message(new SpeedTestPhaseStart()));
    }

    /// <inheritdoc />
    public (SpeedTestModel Model, Command? Command) Update(Message message)
    {
        return message switch
        {
            KeyMessage { Key: Key.Escape } or KeyMessage { Key: Key.CtrlC } => (this, Commands.Quit()),
            KeyMessage { Key: Key.Char, Runes.Length: > 0 } km when km.Runes[0] == new Rune('q') => (this,
                Commands.Quit()),
            SpeedTestPhaseStart => StartFindingServers(),
            ServersFoundMessage found => StartTestingLatency(found),
            FastestServerFoundMessage fastest => StartDownloading(fastest),
            SpeedTestPollMessage when Phase == SpeedTestPhase.Downloading => PollDownload(),
            SpeedTestPollMessage when Phase == SpeedTestPhase.Uploading => PollUpload(),
            SpeedTestErrorMessage error => (this with
            {
                Phase = SpeedTestPhase.Error,
                ErrorMessage = error.ErrorText,
            }, null),
            CommandErrorMessage error => (this with
            {
                Phase = SpeedTestPhase.Error,
                ErrorMessage = error.Exception.Message,
            }, null),
            _ => ForwardToChildren(message)
        };
    }

    /// <inheritdoc />
    public void View(IRenderSurface surface)
    {
        var width = Math.Max(0, surface.Viewport.Width);
        var separator = new string('\u2500', Math.Min(width, 40));

        switch (Phase)
        {
            case SpeedTestPhase.Init:
            case SpeedTestPhase.FindingServers:
                surface.Layout($"""
                    {"⚡ SPEED TEST":bold text-cyan}
                    {separator:dim}

                    {Spinner} {"Discovering servers...":dim}

                    {"Press Esc to quit":dim}
                    """);
                break;

            case SpeedTestPhase.TestingLatency:
                surface.Layout($"""
                    {"⚡ SPEED TEST":bold text-cyan}
                    {separator:dim}

                    {Spinner} {"Testing latency...":dim}

                    {"Press Esc to quit":dim}
                    """);
                break;

            case SpeedTestPhase.Downloading:
            case SpeedTestPhase.Uploading:
            case SpeedTestPhase.Complete:
                RenderTestView(surface, separator);
                break;

            case SpeedTestPhase.Error:
                surface.Layout($"""
                    {"⚡ SPEED TEST":bold text-cyan}
                    {separator:dim}

                    {"Error:":bold text-red} {ErrorMessage ?? "Unknown error":text-red}

                    {"Press Esc to quit":dim}
                    """);
                break;
        }
    }

    private void RenderTestView(IRenderSurface surface, string separator)
    {
        var location = $"({ServerLocation})";
        var latencyColor = Latency < 50 ? "text-green" : Latency < 100 ? "text-yellow" : "text-red";
        var latencyLabel = new Label($"{Latency} ms", latencyColor);

        var dlSpeed = DownloadResult is not null
            ? FormatSpeed(DownloadResult)
            : DownloadSpeedText.Length > 0 ? DownloadSpeedText : "...";

        var showUpload = Phase is SpeedTestPhase.Uploading or SpeedTestPhase.Complete;

        if (showUpload)
        {
            var ulSpeed = UploadResult is not null
                ? FormatSpeed(UploadResult)
                : UploadSpeedText.Length > 0 ? UploadSpeedText : "...";

            if (Phase == SpeedTestPhase.Complete)
            {
                surface.Layout($"""
                    {"⚡ SPEED TEST":bold text-cyan}
                    {separator:dim}

                    Server: {ServerName:text-cyan} {location:dim}
                    Latency: {latencyLabel}

                    {"↓ Download":text-green}
                    {DownloadProgress}
                    {dlSpeed:text-green}

                    {"↑ Upload":text-blue}
                    {UploadProgress}
                    {ulSpeed:text-blue}

                    {"Test complete!":bold text-cyan}  {"Press Esc to quit":dim}
                    """);
            }
            else
            {
                surface.Layout($"""
                    {"⚡ SPEED TEST":bold text-cyan}
                    {separator:dim}

                    Server: {ServerName:text-cyan} {location:dim}
                    Latency: {latencyLabel}

                    {"↓ Download":text-green}
                    {DownloadProgress}
                    {dlSpeed:text-green}

                    {"↑ Upload":text-blue}
                    {UploadProgress}
                    {ulSpeed:text-blue}

                    {"Press Esc to quit":dim}
                    """);
            }
        }
        else
        {
            surface.Layout($"""
                {"⚡ SPEED TEST":bold text-cyan}
                {separator:dim}

                Server: {ServerName:text-cyan}{location:dim}
                Latency: {latencyLabel}

                {"↓ Download":text-green}
                {DownloadProgress}
                {dlSpeed:text-green}

                {"Press Esc to quit":dim}
                """);
        }
    }

    private (SpeedTestModel Model, Command? Command) StartFindingServers()
    {
        var model = this with { Phase = SpeedTestPhase.FindingServers };

        var cmd = new Command(async ct =>
        {
            try
            {
                var service = new OoklaSpeedtest();
                var servers = await service.GetServersAsync(ct).ConfigureAwait(false);
                return new ServersFoundMessage { Service = service, Servers = servers };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new SpeedTestErrorMessage { ErrorText = ex.Message };
            }
        });

        return (model, cmd);
    }

    private (SpeedTestModel Model, Command? Command) StartTestingLatency(ServersFoundMessage found)
    {
        var model = this with
        {
            Phase = SpeedTestPhase.TestingLatency,
            Service = found.Service,
        };

        var service = found.Service;
        var servers = found.Servers;

        var cmd = new Command(async ct =>
        {
            try
            {
                var result = await service.GetFastestServerByLatencyAsync(servers, ct).ConfigureAwait(false);
                return new FastestServerFoundMessage { Service = service, LatencyResult = result };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new SpeedTestErrorMessage { ErrorText = ex.Message };
            }
        });

        return (model, cmd);
    }

    private (SpeedTestModel Model, Command? Command) StartDownloading(FastestServerFoundMessage fastest)
    {
        var server = fastest.LatencyResult.Server;
        var bridge = new SpeedTestBridge();

        // Stop the spinner by creating a fresh one (new Id invalidates old ticks)
        var model = this with
        {
            Phase = SpeedTestPhase.Downloading,
            Server = server,
            ServerName = server.Sponsor,
            ServerLocation = server.Location,
            Latency = fastest.LatencyResult.LatencyMilliseconds,
            DownloadBridge = bridge,
            Spinner = new SpinnerModel(),
        };

        var service = fastest.Service;

        // Fire-and-forget: start download in background
        var fireAndForget = new Command(ct =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await service.GetDownloadSpeedAsync(server, bridge, ct).ConfigureAwait(false);
                    bridge.SetResult(result);
                }
                catch (OperationCanceledException)
                {
                    // Program is exiting, ignore
                }
                catch (Exception ex)
                {
                    bridge.SetError(ex);
                }
            }, ct);

            return Task.FromResult<Message?>(null);
        });

        var pollCmd = PollCommand();

        return (model, Commands.Batch(fireAndForget, pollCmd));
    }

    private (SpeedTestModel Model, Command? Command) PollDownload()
    {
        var bridge = DownloadBridge;
        if (bridge is null)
        {
            return (this, null);
        }

        if (bridge.Error is not null)
        {
            return (this with
            {
                Phase = SpeedTestPhase.Error,
                ErrorMessage = bridge.Error.Message,
            }, null);
        }

        if (bridge is { IsComplete: true, Result: not null })
        {
            // Download complete, snap progress to 100% and start upload
            var completedProgress = DownloadProgress.SetPercentImmediate(1.0);
            var model = this with
            {
                DownloadResult = bridge.Result,
                DownloadProgress = completedProgress,
                DownloadSpeedText = FormatSpeed(bridge.Result),
            };

            return StartUploading(model, null);
        }

        // Update progress from bridge
        var latest = bridge.Latest;
        var updatedModel = this;
        Command? animCmd = null;

        if (latest is not null)
        {
            var pct = Math.Clamp(latest.PercentageComplete / 100.0, 0.0, 1.0);
            var (newProgress, pCmd) = DownloadProgress.SetPercent(pct);
            updatedModel = updatedModel with
            {
                DownloadProgress = newProgress,
                DownloadSpeedText = "Testing...",
            };
            animCmd = pCmd;
        }

        return (updatedModel, Commands.Batch(animCmd, PollCommand()));
    }

    private (SpeedTestModel Model, Command? Command) StartUploading(SpeedTestModel model, Command? progressCmd)
    {
        var bridge = new SpeedTestBridge();
        var server = model.Server!;
        var service = model.Service!;

        model = model with
        {
            Phase = SpeedTestPhase.Uploading,
            UploadBridge = bridge,
        };

        var fireAndForget = new Command(ct =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await service.GetUploadSpeedAsync(server, bridge, ct).ConfigureAwait(false);
                    bridge.SetResult(result);
                }
                catch (OperationCanceledException)
                {
                    // Program is exiting, ignore
                }
                catch (Exception ex)
                {
                    bridge.SetError(ex);
                }
            }, ct);

            return Task.FromResult<Message?>(null);
        });

        var pollCmd = PollCommand();
        return (model, Commands.Batch(progressCmd, fireAndForget, pollCmd));
    }

    private (SpeedTestModel Model, Command? Command) PollUpload()
    {
        var bridge = UploadBridge;
        if (bridge is null)
        {
            return (this, null);
        }

        if (bridge.Error is not null)
        {
            return (this with
            {
                Phase = SpeedTestPhase.Error,
                ErrorMessage = bridge.Error.Message,
            }, null);
        }

        if (bridge.IsComplete && bridge.Result is not null)
        {
            // Upload complete, snap progress to 100%
            var completedProgress = UploadProgress.SetPercentImmediate(1.0);
            var model = this with
            {
                Phase = SpeedTestPhase.Complete,
                UploadResult = bridge.Result,
                UploadProgress = completedProgress,
                UploadSpeedText = FormatSpeed(bridge.Result),
            };

            return (model, null);
        }

        // Update progress from bridge
        var latest = bridge.Latest;
        var updatedModel = this;
        Command? animCmd = null;

        if (latest is not null)
        {
            var pct = Math.Clamp(latest.PercentageComplete / 100.0, 0.0, 1.0);
            var (newProgress, pCmd) = UploadProgress.SetPercent(pct);
            updatedModel = updatedModel with
            {
                UploadProgress = newProgress,
                UploadSpeedText = "Testing...",
            };
            animCmd = pCmd;
        }

        return (updatedModel, Commands.Batch(animCmd, PollCommand()));
    }

    private (SpeedTestModel Model, Command? Command) ForwardToChildren(Message message)
    {
        var (updatedSpinner, spinnerCmd) = Spinner.Update(message);
        var (updatedDlProgress, dlCmd) = DownloadProgress.Update(message);
        var (updatedUlProgress, ulCmd) = UploadProgress.Update(message);

        var model = this with
        {
            Spinner = updatedSpinner,
            DownloadProgress = updatedDlProgress,
            UploadProgress = updatedUlProgress,
        };

        return (model, Commands.Batch(spinnerCmd, dlCmd, ulCmd));
    }

    private static Command PollCommand()
    {
        return Commands.Tick(
            TimeSpan.FromMilliseconds(100),
            _ => new SpeedTestPollMessage());
    }

    private static string FormatSpeed(SpeedTestResult result)
    {
        return result.GetSpeedString(SpeedUnit.BitsPerSecond, SpeedUnitSystem.SI);
    }

}

/// <summary>
/// Internal message to trigger the first phase transition after init.
/// </summary>
internal record SpeedTestPhaseStart : Message;

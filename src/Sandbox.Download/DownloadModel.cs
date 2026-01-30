using Spectre.Console;
using Spectre.Tui;
using Spectre.Tui.Doodads;
using Spectre.Tui.Doodads.Doodads.Label;
using Spectre.Tui.Doodads.Doodads.Progress;
using Spectre.Tui.Doodads.Messages;
using Spectre.Tui.Doodads.Rendering;

namespace Sandbox.Download;

public record DownloadModel : IDoodad<DownloadModel>, ISizedRenderable
{
    private const int ChunkSize = 1024 * 1024;

    public int MinWidth => 60;
    public int MinHeight => 6;

    public string Url { get; init; } = string.Empty;
    public string OutputPath { get; init; } = string.Empty;
    public long BytesReceived { get; init; }
    public long TotalBytes { get; init; }
    public bool IsComplete { get; init; }
    public string? Error { get; init; }

    public ProgressModel Progress { get; init; } = new ProgressModel()
    {
        MinWidth = 50,
        ShowPercentage = true,
    }.WithScaledGradient(
        new Color(255, 105, 180), // hot pink
        new Color(0, 255, 255));  // cyan

    public Command? Init()
    {
        return StartDownloadCommand;
    }

    public (DownloadModel Model, Command? Command) Update(Message message)
    {
        switch (message)
        {
            case DownloadProgressMessage progress:
                {
                    var updated = this with
                    {
                        BytesReceived = progress.BytesReceived,
                        TotalBytes = progress.TotalBytes,
                    };

                    var percent = progress.TotalBytes > 0
                        ? (double)progress.BytesReceived / progress.TotalBytes
                        : 0;

                    var (newProgress, progressCmd) = updated.Progress.SetPercent(percent);
                    updated = updated with { Progress = newProgress };

                    var nextChunkCmd = ReadChunkCommand(progress.Context, progress.BytesReceived);
                    return (updated, Commands.Batch(progressCmd, nextChunkCmd));
                }

            case DownloadCompleteMessage complete:
                {
                    var (newProgress, progressCmd) = Progress.SetPercent(1.0);
                    var updated = this with
                    {
                        BytesReceived = complete.TotalBytes,
                        TotalBytes = complete.TotalBytes,
                        IsComplete = true,
                        Progress = newProgress,
                    };

                    return (updated, progressCmd);
                }

            case DownloadErrorMessage error:
                return (this with { Error = error.Error }, null);

            case KeyMessage { Key: Key.Escape }:
                return (this, Commands.Quit());

            default:
                {
                    var (newProgress, progressCmd) = Progress.Update(message);
                    var updated = this with { Progress = newProgress };

                    if (IsComplete && !newProgress.IsAnimating)
                    {
                        return (updated, Commands.Quit());
                    }

                    return (updated, progressCmd);
                }
        }
    }

    public void View(IRenderSurface surface)
    {
        var urlText = Url.Length > 80
            ? new Uri(Url).GetComponents(UriComponents.Path, UriFormat.Unescaped)
            : Url;


        var bytesInfo = TotalBytes > 0
            ? $"{FormatBytes(BytesReceived)} / {FormatBytes(TotalBytes)}  ({FormatBytes(TotalBytes - BytesReceived)} remaining)"
            : $"{FormatBytes(BytesReceived)} downloaded";

        var status = StatusLabel();

        surface.Layout($"""
            Downloading: {urlText:text-blue}

            {bytesInfo:dim}
            {Progress}

            {status}
            """);
    }

    private Label StatusLabel()
    {
        if (Error is not null)
            return new Label($"Error: {Error}", "text-red");

        return IsComplete
            ? new Label("Download complete!", "text-green")
            : new Label("Press Esc to cancel", "dim");
    }

    private async Task<Message?> StartDownloadCommand(CancellationToken cancellationToken)
    {
        HttpClient? client = null;
        HttpResponseMessage? response = null;
        Stream? responseStream = null;
        FileStream? outputFile = null;

        try
        {
            client = new HttpClient();
            response = await client.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            outputFile = new FileStream(OutputPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[ChunkSize];
            var bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, ChunkSize), cancellationToken);

            if (bytesRead == 0)
            {
                await responseStream.DisposeAsync();
                await outputFile.DisposeAsync();
                response.Dispose();
                client.Dispose();
                return new DownloadCompleteMessage { TotalBytes = 0 };
            }

            await outputFile.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

            var context = new DownloadContext(client, response, responseStream, outputFile, totalBytes);
            return new DownloadProgressMessage
            {
                Context = context,
                BytesReceived = bytesRead,
                TotalBytes = totalBytes,
            };
        }
        catch (OperationCanceledException)
        {
            if (responseStream is not null) await responseStream.DisposeAsync();
            if (outputFile is not null) await outputFile.DisposeAsync();
            response?.Dispose();
            client?.Dispose();
            throw;
        }
        catch (Exception ex)
        {
            if (responseStream is not null) await responseStream.DisposeAsync();
            if (outputFile is not null) await outputFile.DisposeAsync();
            response?.Dispose();
            client?.Dispose();
            return new DownloadErrorMessage { Error = ex.Message };
        }
    }

    private static Command ReadChunkCommand(DownloadContext context, long bytesReceived)
    {
        return async cancellationToken =>
        {
            try
            {
                var buffer = new byte[ChunkSize];
                var bytesRead = await context.ResponseStream
                    .ReadAsync(buffer.AsMemory(0, ChunkSize), cancellationToken);

                if (bytesRead == 0)
                {
                    var totalBytes = bytesReceived;
                    await context.DisposeAsync();
                    return new DownloadCompleteMessage { TotalBytes = totalBytes };
                }

                await context.OutputFile
                    .WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                return new DownloadProgressMessage
                {
                    Context = context,
                    BytesReceived = bytesReceived + bytesRead,
                    TotalBytes = context.TotalBytes,
                };
            }
            catch (OperationCanceledException)
            {
                await context.DisposeAsync();
                throw;
            }
            catch (Exception ex)
            {
                await context.DisposeAsync();
                return new DownloadErrorMessage { Error = ex.Message };
            }
        };
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F1} GB",
            >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
            >= 1_024 => $"{bytes / 1_024.0:F1} KB",
            _ => $"{bytes} B",
        };
    }
}
using Spectre.Tui.Doodads.Messages;

namespace Sandbox.Download;

internal record DownloadProgressMessage : Message
{
    public required DownloadContext Context { get; init; }
    public required long BytesReceived { get; init; }
    public required long TotalBytes { get; init; }
}

internal record DownloadCompleteMessage : Message
{
    public required long TotalBytes { get; init; }
}

internal record DownloadErrorMessage : Message
{
    public required string Error { get; init; }
}
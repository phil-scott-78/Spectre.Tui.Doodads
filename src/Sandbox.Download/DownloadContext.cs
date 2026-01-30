namespace Sandbox.Download;

internal sealed class DownloadContext : IDisposable, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly HttpResponseMessage _response;

    public DownloadContext(
        HttpClient client,
        HttpResponseMessage response,
        Stream responseStream,
        FileStream outputFile,
        long totalBytes)
    {
        _client = client;
        _response = response;
        ResponseStream = responseStream;
        OutputFile = outputFile;
        TotalBytes = totalBytes;
    }

    public Stream ResponseStream { get; }
    public FileStream OutputFile { get; }
    public long TotalBytes { get; }

    public void Dispose()
    {
        ResponseStream.Dispose();
        OutputFile.Dispose();
        _response.Dispose();
        _client.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await ResponseStream.DisposeAsync();
        await OutputFile.DisposeAsync();
        _response.Dispose();
        _client.Dispose();
    }
}
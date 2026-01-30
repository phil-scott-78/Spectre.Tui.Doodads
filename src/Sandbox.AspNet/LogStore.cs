using System.Collections.Concurrent;

namespace Sandbox.AspNet;

/// <summary>
/// Thread-safe in-memory log collection.
/// </summary>
public sealed class LogStore
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private volatile int _version;

    /// <summary>
    /// Gets the version counter, incremented on each new entry.
    /// </summary>
    public int Version => _version;

    /// <summary>
    /// Adds a log entry to the store.
    /// </summary>
    public void Add(LogLevel level, string category, string message, string? exception = null)
    {
        _entries.Enqueue(new LogEntry(DateTimeOffset.Now, level, category, message, exception));
        Interlocked.Increment(ref _version);
    }

    /// <summary>
    /// Returns a point-in-time snapshot of all entries.
    /// </summary>
    public LogEntry[] GetSnapshot()
    {
        return _entries.ToArray();
    }
}
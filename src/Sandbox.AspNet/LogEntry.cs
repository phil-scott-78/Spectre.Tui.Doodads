namespace Sandbox.AspNet;

/// <summary>
/// An immutable record representing a captured log entry.
/// </summary>
public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    string? Exception);
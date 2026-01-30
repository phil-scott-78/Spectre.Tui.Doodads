namespace Sandbox.AspNet;

/// <summary>
/// A logger provider that writes log entries to a <see cref="LogStore"/>.
/// </summary>
public sealed class MemoryLoggerProvider(LogStore store) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new MemoryLogger(store, categoryName);
    }

    public void Dispose()
    {
    }

    private sealed class MemoryLogger(LogStore store, string category) : ILogger
    {
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            store.Add(logLevel, category, message, exception?.ToString());
        }
    }
}
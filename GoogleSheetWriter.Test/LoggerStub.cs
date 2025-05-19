using Microsoft.Extensions.Logging;

internal class LoggerStub<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => new DummyScope();
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

    private class DummyScope : IDisposable { public void Dispose() { } }
}
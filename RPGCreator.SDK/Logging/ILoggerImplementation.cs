namespace RPGCreator.SDK.Logging;

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public interface ILoggerImplementation
{
    void Write(LogLevel level, string message, params object[] args);
    void Dump(object? objToDump);
}
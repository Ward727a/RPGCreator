namespace RPGCreator.SDK.Logging;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public interface ILoggerImplementation
{
    void Write(LogLevel level, string message, params object[] args);
}
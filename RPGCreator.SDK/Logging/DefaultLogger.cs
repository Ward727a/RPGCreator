namespace RPGCreator.SDK.Logging;

public class DefaultLogger : ILoggerImplementation
{
    public void Write(LogLevel level, string message, params object[] args)
    {
        Console.WriteLine($"[{level}] {string.Format(message, args)}");
    }
}
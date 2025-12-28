namespace RPGCreator.SDK.Logging;

public static class Logger
{
    // ReSharper disable once MemberCanBePrivate.Global
    public static ILoggerImplementation? Implementation { get; set; } = new DefaultLogger();
    
    public static void Info(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Info, message, args);
    }
    
    public static void Information(string message, params object[] args) => Info(message, args);
    
    public static void Warning(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Warning, message, args);
    }
    
    public static void Error(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Error, message, args);
    }
    
    public static void Debug(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Debug, message, args);
    }
    
    public static void Critical(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Critical, message, args);
    }
}
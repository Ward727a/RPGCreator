namespace RPGCreator.SDK.Logging;

public static class Logger
{
    // ReSharper disable once MemberCanBePrivate.Global
    public static ILoggerImplementation? Implementation { get; set; } = new DefaultLogger();
    public static ScopedLogger ForContext<T>()
    {
        return new ScopedLogger(typeof(T).FullName ?? "Unknown", typeof(T).Assembly.GetName().Name ?? "Unknown");
    }
    
    public static void Trace(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Trace, message, args);
    }
    
    public static void Info(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Info, message, args);
    }
    
    public static void Warning(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Warning, message, args);
    }
    
    public static void Error(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Error, message, args);
    }
    
    public static void Error(Exception exception, string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Error, $"{message} | Exception: {exception}", args);
    }
    
    public static void Debug(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Debug, message, args);
    }
    
    public static void Critical(string message, params object[] args)
    {
        Implementation?.Write(LogLevel.Critical, message, args);
    }
    
    public static void Dump(object obj)
    {
        var finalMessage = "--- ADDITIONAL DEBUG INFORMATION ---";
        finalMessage += "\n----------------------------------";
        finalMessage += "\n--- PLEASE READ BEFORE SHARING ---";
        finalMessage += "\n----------------------------------";
        finalMessage += "\n/!\\ CAUTION: THE DUMP BELOW MAY CONTAIN SENSITIVE DATA FROM YOUR PROJECT OR SYSTEM PATHS.";
        finalMessage += "\n/!\\ DO NOT SHARE THIS LOG PUBLICLY UNLESS YOU TRUST THE RECIPIENT.";
        
        Implementation?.Write(LogLevel.Critical, finalMessage);
        Implementation?.Dump(obj);
    }
}
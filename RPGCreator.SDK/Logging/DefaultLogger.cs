namespace RPGCreator.SDK.Logging;

public class DefaultLogger : ILoggerImplementation
{
    public void Write(LogLevel level, string message, params object[] args)
    {
        #if !DEBUG
        if(level == LogLevel.Debug)
            return; // Ignore debug messages in the default logger
        #endif
        Console.WriteLine($"[{level}] {string.Format(message, args)}");
    }

    public void Dump(object? objToDump)
    {
        if (objToDump == null)
        {
            Console.WriteLine("Dump: null");
            return;
        }
        
        Console.WriteLine("Dump:");
        foreach (var prop in objToDump.GetType().GetProperties())
        {
            var value = prop.GetValue(objToDump);
            Console.WriteLine($"  {prop.Name}: {value}");
        }
    }
}
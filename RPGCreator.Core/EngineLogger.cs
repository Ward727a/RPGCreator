using Newtonsoft.Json;
using RPGCreator.SDK.Logging;
using Serilog;

namespace RPGCreator.Core;

/// <summary>
/// This class is responsible for initializing the Serilog logger for the RPG Creator engine.
/// </summary>
public class EngineLogger : ILoggerImplementation
{
    public EngineLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        // Here we need to add manually the assembly name because this class CANNOT use ScopedLogger as it would create a circular dependency.
        Log.Information("[RPGCreator.Core.EngineLogger.Constructor] EngineLogger initialized.");
    }
    public void Write(LogLevel level, string message, params object[] args)
    {
        switch (level)
        {
            case LogLevel.Trace:
                Log.Verbose(message, args);
                break;
            case LogLevel.Debug:
                Log.Debug(message, args);
                break;
            case LogLevel.Info:
                Log.Information(message, args);
                break;
            case LogLevel.Warning:
                Log.Warning(message, args);
                break;
            case LogLevel.Error:
                Log.Error(message, args);
                break;
            case LogLevel.Critical:
                Log.Fatal(message, args);
                break;
        }
    }

    public void Dump(object? objToDump)
    {
        string finalMessage = "[OBJECT DUMP]";
        try 
        {
            var settings = new JsonSerializerSettings 
            { 
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (sender, args) => { args.ErrorContext.Handled = true; } 
            };
    
            var jsonDump = JsonConvert.SerializeObject(objToDump, settings);
            finalMessage += $"\n--- OBJECT DUMP ---\n{jsonDump}\n-------------------";
        }
        catch (Exception ex)
        {
            finalMessage += $"\n(Failed to dump object of type {objToDump.GetType().Name}. Reason: {ex.Message})";
        }
        
        Log.Fatal(finalMessage);
    }
}
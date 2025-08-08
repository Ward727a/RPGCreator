using Serilog;

namespace RPGCreator.Core;

/// <summary>
/// This class is responsible for initializing the Serilog logger for the RPG Creator engine.
/// </summary>
internal class EngineLogger
{
    internal EngineLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        Log.Information($"EngineLogger initialized.");
    }
}
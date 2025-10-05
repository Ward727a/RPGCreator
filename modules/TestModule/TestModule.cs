using RPGCreator.Core.ModuleSDK;
using Serilog;

namespace TestModule;

public class TestModule : IEngineModule
{
    public Version TargetEngineVersion { get; } = new Version(1, 0, 0);
    public string Name { get; } = "Test Module";
    public string Version { get; } = "1.0.0";
    public string Author { get; } = "Your Name";
    public string Description { get; } = "A test module for RPG Creator.";
    public void Initialize()
    {
        // Initialization code here
        Log.Information("Test Module initialized.");
    }
}
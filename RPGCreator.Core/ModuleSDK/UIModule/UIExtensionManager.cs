using Avalonia.Controls;

namespace RPGCreator.Core.ModuleSDK.UIModule;

public static class UIExtensionManager
{
    private static Dictionary<UIRegion, List<Action<Control, object?>>> _extensions = new();
    
    public static void RegisterExtension(UIRegion region, Action<Control, object?> extension)
    {
        if (!_extensions.ContainsKey(region))
            _extensions[region] = new List<Action<Control, object?>>();
        
        _extensions[region].Add(extension);
    }
    
    public static void ApplyExtensions(UIRegion region, Control targetControl, object? context = null)
    {
        if (_extensions.TryGetValue(region, out var extensions))
        {
            foreach (var extension in extensions)
            {
                extension(targetControl, context);
            }
        }
    }
}
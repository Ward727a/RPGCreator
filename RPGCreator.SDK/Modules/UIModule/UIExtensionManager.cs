namespace RPGCreator.SDK.Modules.UIModule;

public static class UIExtensionManager
{
    private static Dictionary<UIRegion, List<Action<object, object?>>> _extensions = new();
    
    public static void RegisterExtension(UIRegion region, Action<object, object?> extension)
    {
        if (!_extensions.ContainsKey(region))
            _extensions[region] = new List<Action<object, object?>>();
        
        _extensions[region].Add(extension);
    }
    
    public static void ApplyExtensions(UIRegion region, object targetControl, object? context = null)
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
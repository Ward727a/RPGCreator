using System;
using System.Collections.Generic;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.EditorUiService;

namespace RPGCreator.UI.UiService;

public class UiExtensionManager : IUiExtensionManager
{
    private Dictionary<UIRegion, List<Action<object, object?>>> _extensions = new();
    
    public void RegisterExtension(UIRegion region, Action<object, object?> extension)
    {
        if (!_extensions.ContainsKey(region))
            _extensions[region] = new List<Action<object, object?>>();
        
        _extensions[region].Add(extension);
    }
    
    public void ApplyExtensions(UIRegion region, object targetControl, object? context = null)
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
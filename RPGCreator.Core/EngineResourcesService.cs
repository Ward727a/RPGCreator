using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Resources;

namespace RPGCreator.Core;

public class EngineResourcesService : IResourceService
{
    private readonly ScopedLogger _logger = Logger.ForContext<EngineResourcesService>();
    private Dictionary<Type, IResourceLoader> _resourceLoaders = new();
    private Dictionary<string, object> _resourceCache = new();
    
    public T? Load<T>(string path) where T : class
    {
        if (_resourceCache.TryGetValue(path, out var cachedResource) && cachedResource is T resource)
        {
            return resource;
        }
        if (_resourceLoaders.TryGetValue(typeof(T), out var loader))
        {
            var loadedResource = loader.Load(path) as T;
            if (loadedResource != null)
            {
                _resourceCache[path] = loadedResource;
            }
            return loadedResource;
        }
        _logger.Error($"No resource loader registered for type {typeof(T).FullName}");
        return null;
    }

    public void RegisterLoader<T>(IResourceLoader loader) where T : class
    {
        _resourceLoaders[typeof(T)] = loader;
    }

    public void Unload(string path)
    {
        _resourceCache.Remove(path);
    }

    public void ClearCache()
    {
        _resourceCache.Clear();
    }
}
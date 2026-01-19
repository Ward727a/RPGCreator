namespace RPGCreator.SDK.Resources;

public interface IResourceService : IService
{
    T? Load<T>(string path) where T : class;
    
    void RegisterLoader<T>(IResourceLoader loader) where T : class;
    
    void Unload(string path);
    void ClearCache();
}
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.SDK;

public interface IGameFactory : IService
{
    public void Register<TInst, TDef>(IAssetFactory<TInst, TDef> factory)
        where TInst : class
        where TDef : IAssetDef;

    public TInst CreateInstance<TInst>(IAssetDef def) where TInst : class;

    public ValueTask<TInst> CreateInstanceAsync<TInst>(IAssetDef def, CancellationToken ct = default)
        where TInst : class;

    public void ReleaseInstance<TInst>(TInst instance) where TInst : class;

    public void Release(IAssetDef def);
    
    public void Refresh(IAssetDef def);

    public void ClearAll();
}

public interface IAssetsManager : IService
{
    event Action<IAssetDef>? OnAssetRegistered;
    event Action<IAssetDef>? OnAssetUnregistered;
    public void RegisterRegistry(IAssetRegistry registry);
    public void RegisterAsset(object asset);
    public bool TryResolveRegistry(string ModuleName, [NotNullWhen(true)] out IAssetRegistry? registry);
    public bool TryResolveRegistry(System.Type type, [NotNullWhen(true)] out IAssetRegistry? registry);
    public bool TryResolveAsset<T>(URN urn, [NotNullWhen(true)] out T? result) where T : class, IHasUniqueId;
    public bool TryResolveAsset<T>(Ulid uniqueId, [NotNullWhen(true)] out T? result) where T : class, IHasUniqueId;
    public T CreateAsset<T>() where T : IAssetDef, new();
    public T CreateTransientAsset<T>(IAssetScope? scope = null) where T : IAssetDef, new();
    public void DestroyTransientAsset<T>(T asset) where T : IAssetDef;
    public IAssetScope CreateAssetScope(string? name = null);
    public void AddPack(string dbPath);
    public void RegisterPack(IAssetsPack pack);
    public void UnregisterPack(Ulid packId);
    public bool TryGetPack(string? packName, [NotNullWhen(true)] out IAssetsPack? pack);
    public bool TryGetPack(Ulid packId, [NotNullWhen(true)] out IAssetsPack? pack);
    public IAssetsPack GetPack(Ulid packId);
    public void AddNewAssetLocation(Ulid assetId, IAssetsPack? pack, string relativePath, string typeName, bool isTransient = false);
    public List<IAssetsPack> GetLoadedPacks();
    public IEnumerable<PackSearchResult> SearchAllPacks<T>();
}

public interface IProjectsManager : IService
{
    List<BaseProjectLink> GetAllProjects();
    public IBaseProject? CreateProject(string projectName, string projectPath);
    public bool TryGetProject(string configPath, out IBaseProject? project);
    public void OpenProject(IBaseProject project);
    public void CloseCurrentProject();
}

public interface IECSService : IService
{
    public IECSWorld CreateWorld();
}

public interface IBrushManager : IService
{
    public void ClickAt(Vector2 at);
    public void PreviewAt(Vector2 at);
    public void ClearPreview();
    public Vector2 NormalizedPositionToTile(Vector2 position);
}


public class EngineServicesProvider : IServiceProvider
{
    private readonly Dictionary<Type, IService> _services = new();
    
    public T GetService<T>() where T : class, IService
    {
        return (T)_services[typeof(T)];
    }

    public bool TryGetService<T>([NotNullWhen(true)] out T? service) where T : class, IService
    {
        if (_services.TryGetValue(typeof(T), out var svc))
        {
            service = (T)svc;
            return true;
        }
        
        service = null;
        return false;
    }

    public void RegisterService<T>(T service) where T : class, IService
    {
        _services[typeof(T)] = service;
    }
}

public static class EngineServices
{
    private static readonly EngineServicesProvider ServiceProvider = new();
    
    // ReSharper disable MemberCanBePrivate.Global
    public static void RegisterService<T>(T service) where T : class, IService
    {
        ServiceProvider.RegisterService(service);
    }
    public static T GetService<T>() where T : class, IService
    {
        if (ServiceProvider.TryGetService<T>(out var service))
        {
            return service;
        }

        throw new InvalidOperationException($"[Engine] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during engine initialization.");
    }

    public static IGameFactory GameFactory
    {
        get => GetService<IGameFactory>();
        set => RegisterService(value);
    }

    public static IAssetsManager AssetsManager
    {
        get => GetService<IAssetsManager>();
        set => RegisterService(value);
    }

    public static IBrushManager BrushManager
    {
        get => GetService<IBrushManager>();
        set => RegisterService(value);
    }

    public static ISerializerService SerializerService
    {
        get => GetService<ISerializerService>();
        set => RegisterService(value);
    }

    public static IAssetTypeRegistry AssetTypeRegistry
    {
        get => GetService<IAssetTypeRegistry>();
        set => RegisterService(value);
    }
    
    public static IResourceService ResourcesService
    {
        get => GetService<IResourceService>();
        set => RegisterService(value);
    }
    public static IProjectsManager ProjectsManager
    {
        get => GetService<IProjectsManager>();
        set => RegisterService(value);
    }
    public static IGraphService GraphService
    {
        get => GetService<IGraphService>();
        set => RegisterService(value);
    }
    public static IGraphNodeScanner GraphNodeScanner
    {
        get => GetService<IGraphNodeScanner>();
        set => RegisterService(value);
    }
    public static IPrattFormulaService PrattFormulaService
    {
        get => GetService<IPrattFormulaService>();
        set => RegisterService(value);
    }
    public static IECSService ECS
    {
        get => GetService<IECSService>();
        set => RegisterService(value);
    }
}
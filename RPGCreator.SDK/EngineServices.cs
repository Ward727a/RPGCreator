using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.SDK;

public interface IGameFactory
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

public interface IAssetsManager
{
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
    public void AddNewAssetLocation(Ulid assetId, IAssetsPack pack, string relativePath, string typeName);
    public List<IAssetsPack> GetLoadedPacks();
    public IEnumerable<PackSearchResult> SearchAllPacks<T>();
}

public interface IProjectsManager
{
    List<IBaseProjectLink> GetAllProjects();
    public IBaseProject? CreateProject(string projectName, string projectPath);
    public bool TryGetProject(string configPath, out IBaseProject? project);
}

public static class EngineServices
{
    public static IGameFactory GameFactory { get; set; } = null!;
    public static IAssetsManager AssetsManager { get; set; } = null!;
    public static ISerializerService SerializerService { get; set; } = null!;
    public static IAssetTypeRegistry AssetTypeRegistry { get; set; } = null!;
    public static IResourceService ResourcesService { get; set; } = null!;
    public static IProjectsManager ProjectsManager { get; set; } = null!;
    public static IGraphService GraphService { get; set; } = null!;
    public static IPrattFormulaService PrattFormulaService { get; set; } = null!;
}
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Features;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Exceptions;
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

public sealed record EntityFeaturePropertyMetadata(
    PropertyInfo PropertyInfo,
    EntityFeaturePropertyAttribute Attribute,
    Type PropertyType);

public interface IEcsService : IService
{
    public IEcsWorld CreateWorld();
    public IEntityFeature GetFeature(URN featureUrn);
    public List<IEntityFeature> GetAllFeatures();
    public bool TryGetFeature(URN featureUrn, [NotNullWhen(true)] out IEntityFeature? feature);
    public IEnumerable<EntityFeaturePropertyMetadata> GetEditableProperties(URN feature);
    public void RegisterFeature(IEntityFeature feature);
    public bool HasFeature(URN featureUrn);
    public void UnregisterFeature(URN featureUrn);
}

public interface IModulePathResolver : IService
{
    /// <summary>
    /// Return the path registered for the given URN.<br/>
    /// If no path is registered, returns an empty string.
    /// </summary>
    /// <param name="targetUrn">The target URN to resolve the path for.</param>
    /// <returns>
    /// The resolved path as a string, or an empty string if not found.
    /// </returns>
    public string ResolvePath(URN targetUrn);
    
    /// <summary>
    /// Return the full file path for a given URN and relative file path.<br/>
    /// Combines the registered path for the URN with the provided file path.<br/>
    /// If no path is registered for the URN, returns the original file path.
    /// <example>
    ///
    /// If we have [rpgc://modules/MyModuleFolder] registered to "C:/Games/MyGame/Modules/MyModuleFolder",<br/>
    /// then calling <code>ResolveFilePath(rpgc://modules/MyModuleFolder, "Assets/Textures/texture.png")</code> will return:<br/>
    /// "C:/Games/MyGame/Modules/MyModuleFolder/Assets/Textures/texture.png"
    ///
    /// </example>
    /// </summary>
    /// <param name="targetUrn">The target URN to resolve the path for.</param>
    /// <param name="filePath">The relative file path to combine with the resolved path.</param>
    /// <returns>
    /// The combined full file path as a string.
    /// </returns>
    public string ResolveFilePath(URN targetUrn, string filePath);
    
    /// <summary>
    /// Registers a path for a given URN.<br/>
    /// This path will be used when resolving paths for the specified URN.
    /// </summary>
    /// <param name="targetUrn">The target URN to register the path for.</param>
    /// <param name="path">The path to register.</param>
    public void RegisterPath(URN targetUrn, string path);
}

public interface ICommandManager : IService
{
    public bool CanUndo { get; }
    public bool CanRedo { get; }
    public void ExecuteCommand(RPGCreator.SDK.Commands.ICommand command);
    public void UndoLastCommand();
    public void RedoLastCommand();
    public void ClearHistory();
    public string GetUndoCommandName();
    public string GetRedoCommandName();
}

/// <summary>
/// The brush manager service.<br/>
/// Manages brushes used for painting or drawing within the engine/editor.<br/>
/// Be aware that the service is different from the brush state.<br/>
/// <br/>
/// The brush state manage the 'memory' of the brush system, such as the currently selected brush, preview state, etc...<br/>
/// While the brush manager is responsible for 'using' the brush state and providing methods to manipulate brushes.
/// </summary>
public interface IBrushManager : IService
{
    /// <summary>
    /// The current state of the brush manager.<br/>
    /// This includes information such as the selected brush and preview state.<br/>
    /// <br/>
    /// In fact, this is just a reference to <see cref="EngineStates.BrushState"/>.<br/>
    /// </summary>
    IBrushState State { get; }

    /// <summary>
    /// Gets a brush by its URN.<br/>
    /// Returns null if the brush does not exist.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to retrieve.</param>
    /// <returns>
    /// The brush info if found; otherwise, null.
    /// </returns>
    public IBrushInfo? GetBrush(URN brushUrn);

    /// <summary>
    /// Tries to get a brush by its URN.<br/>
    /// Returns true if the brush exists, false otherwise.
    /// </summary>
    /// <param name="brushName">The URN of the brush to retrieve.</param>
    /// <param name="brush">The output brush info if found; otherwise, null.</param>
    /// <returns>
    /// True if the brush was found; otherwise, false.
    /// </returns>
    public bool TryGetBrush(URN brushName, [NotNullWhen(true)] out IBrushInfo? brush);
    
    /// <summary>
    /// Adds a brush to the manager.<br/>
    /// If a brush with the same URN already exists and <paramref name="overwriteIfExists"/> is false, the method does nothing.<br/>
    /// If <paramref name="overwriteIfExists"/> is true, the existing brush will be replaced.
    /// </summary>
    /// <param name="brush">The brush info to add.</param>
    /// <param name="overwriteIfExists">Whether to overwrite the existing brush if it already exists.</param>
    public void AddBrush(IBrushInfo brush, bool overwriteIfExists = false);
    
    /// <summary>
    /// Tries to add a brush to the manager.<br/>
    /// Returns true if the brush was added successfully, false if a brush with the same URN already exists.
    /// </summary>
    /// <param name="brush">The brush info to add.</param>
    /// <returns>
    /// True if the brush was added successfully; otherwise, false.
    /// </returns>
    public bool TryAddBrush(IBrushInfo brush);
    
    /// <summary>
    /// Removes a brush from the manager.
    /// </summary>
    /// <param name="brush">The brush info to remove.</param>
    public void RemoveBrush(IBrushInfo brush);
    
    /// <summary>
    /// Tries to remove a brush by its URN.<br/>
    /// Returns true if the brush was removed successfully, false if the brush does not exist.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to remove.</param>
    /// <returns>
    /// True if the brush was removed successfully; otherwise, false.
    /// </returns>
    public bool TryRemoveBrush(URN brushUrn);
    
    /// <summary>
    /// Checks if a brush exists by its URN.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to check.</param>
    /// <returns>
    /// True if the brush exists; otherwise, false.
    /// </returns>
    public bool HasBrush(URN brushUrn);
    
    /// <summary>
    /// Selects a brush by its URN.<br/>
    /// Returns true if the brush was selected successfully, false if the brush does not exist.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to select.</param>
    /// <returns>
    /// True if the brush was selected successfully; otherwise, false.
    /// </returns>
    public bool SelectBrush(URN brushUrn);
    
    /// <summary>
    /// Selects a brush directly.<br/>
    /// Sets the selected brush in the brush state to the provided brush.<br/>
    /// This forces the selection to the given brush without any checks.
    /// </summary>
    /// <param name="brush">The brush info to select.</param>
    public void SelectBrush(IBrushInfo brush);
    
    /// <summary>
    /// Gets the currently selected brush.<br/>
    /// Returns null if no brush is selected.
    /// </summary>
    /// <returns>
    /// The currently selected brush info if any; otherwise, null.
    /// </returns>
    public IBrushInfo? GetSelectedBrush();

    /// <summary>
    /// Verifies if a brush supports a specific feature type.<br/>
    /// This method checks if the brush identified by the given URN implements the specified brush feature interface.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to check.</param>
    /// <typeparam name="BrushFeature">The type of brush feature to check for.</typeparam>
    /// <returns>
    /// True if the brush supports the specified feature; otherwise, false.
    /// </returns>
    public bool IsBrushAbleTo<BrushFeature>(URN brushUrn) where BrushFeature : IBrushFeature;
    
    /// <summary>
    /// Retrieves all features of a specific type from a brush.<br/>
    /// This method returns an enumerable of brush features of the specified type that are implemented by the brush identified by the given URN.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to retrieve features from.</param>
    /// <returns>
    /// An enumerable of brush features of the specified type.
    /// </returns>
    public IEnumerable<IBrushFeature> GetBrushFeatures(URN brushUrn);
    
    /// <summary>
    /// Retrieves all registered brushes' URNs.<br/>
    /// Returns an enumerable of URNs representing all brushes managed by the brush manager.
    /// </summary>
    /// <returns>
    /// An enumerable of URNs for all registered brushes.
    /// </returns>
    public IEnumerable<URN> GetAllBrushes();
    
    /// <summary>
    /// Draws with the selected brush at the specified position.<br/>
    /// This method uses the current brush selected in the brush state to perform the drawing action.
    /// </summary>
    /// <param name="at">The position where to draw.</param>
    public void DrawAt(Vector2 at);
    
    /// <summary>
    /// Previews the brush at the specified position.<br/>
    /// This method shows a preview of the brush effect at the given position without actually applying it.
    /// </summary>
    /// <param name="at">The position where to preview the brush.</param>
    public void PreviewAt(Vector2 at);
    public void ClearPreview();
    public Vector2 NormalizedPositionToTile(Vector2 position);
}


public class EngineServicesProvider : IServiceProvider
{
    private readonly Dictionary<Type, Dictionary<string, IService>> _services = new();

    public T GetService<T>(string groupName = "") where T : class, IService
    {
        if (_services.TryGetValue(typeof(T), out var groups))
        {
            if (groups.TryGetValue(groupName, out var service))
            {
                return (T)service;
            }
            
            if (string.IsNullOrEmpty(groupName) && groups.Count == 1)
            {
                return (T)groups.Values.First();
            }
        }

        throw new CriticalEngineException($"[Engine] Critical Service Missing: {typeof(T).Name} (Group: '{groupName}')", _services);
    }
    
    
    public bool TryGetService<T>([NotNullWhen(true)] out T? service, string groupName = "") where T : class, IService
    {
        if (_services.TryGetValue(typeof(T), out var groups))
        {
            if (groups.TryGetValue(groupName, out var svc))
            {
                service = (T)svc;
                return true;
            }
            
            if (string.IsNullOrEmpty(groupName) && groups.Count == 1)
            {
                service = (T)groups.Values.First();
                return true;
            }
        }

        service = null;
        return false;
    } 

    public void RegisterService<T>(T service, string groupName) where T : class, IService
    {
        if (!_services.TryGetValue(typeof(T), out var groups))
        {
            groups = new Dictionary<string, IService>();
            _services[typeof(T)] = groups;
        }

        if (groups.ContainsKey(groupName))
        {
            throw new InvalidOperationException($"[Engine] Service '{typeof(T).Name}' already registered in group '{groupName}'.");
        }

        groups[groupName] = service;
    }
}


public static class EngineServices
{
    private static readonly EngineServicesProvider ServiceProvider = new();
    
    // ReSharper disable MemberCanBePrivate.Global
    public static void RegisterService<T>(T service, string groupName) where T : class, IService
    {
        if(string.Equals(groupName, "default", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("[Engine] 'default' is a reserved group name. Use a different name for the service group.");
        }
        
        ServiceProvider.RegisterService(service, groupName);
    }
    public static T GetService<T>(string groupName = "default") where T : class, IService
    {
        if (ServiceProvider.TryGetService<T>(out var service, groupName))
        {
            return service;
        }

        throw new InvalidOperationException($"[Engine] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during engine initialization.");
    }
    
    private static void RegisterService<T>(T service) where T : class, IService
    {
        ServiceProvider.RegisterService(service, "default");
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
    
    public static ICommandManager UndoRedoService
    {
        get => GetService<ICommandManager>();
        set => RegisterService(value);
    }
    
    public static IEcsService ECS
    {
        get => GetService<IEcsService>();
        set => RegisterService(value);
    }
    
    public static IInputsService InputsService
    {
        get => GetService<IInputsService>();
        set => RegisterService(value);
    }
    
    public static IModulePathResolver ModulePathResolver
    {
        get => GetService<IModulePathResolver>();
        set => RegisterService(value);
    }
}
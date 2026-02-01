using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK;


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
    
    public static IModuleManager ModuleManager
    {
        get => GetService<IModuleManager>();
        set => RegisterService(value);
    }
    
    public static IFeaturesManager FeaturesManager
    {
        get => GetService<IFeaturesManager>();
        set => RegisterService(value);
    }
}
using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.SDK;


public class RuntimeServicesProvider : IServiceProvider
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

        throw new CriticalEngineException($"[Runtime] Critical Service Missing: {typeof(T).Name} (Group: '{groupName}')", _services);
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
            throw new InvalidOperationException($"[Runtime] Service '{typeof(T).Name}' already registered in group '{groupName}'.");
        }

        groups[groupName] = service;
    }
}

/// <summary>
/// Every services related to the runtime environment.<br/>
/// ex: game loop, time management, map editing, etc...<br/>
/// <br/>
/// For a better understanding of runtime services, you need to think like that:<br/>
/// - Where is the service being initialized?<br/>
/// >>> In the Core? Then it's a EngineServices.<br/>
/// >>> In the RTP? Then it's a RuntimeServices.<br/>
/// - What 'parts' the service is trying to 'connect'?
/// >>> Core to UI? UI to Core? Then it's a EngineServices.<br/>
/// >>> RTP to UI? UI to RTP? Then it's a RuntimeServices.<br/>
/// >>> RTP to Core? Core to RTP? Then it depends on what the service need to do.<br/>
/// - Is the service need to <b>mainly</b> interact with or use MonoGame/XNA or any other game framework?<br/>
/// >>> Yes? Then it's probably a RuntimeServices.<br/>
/// >>> No? Then it's a EngineServices.<br/>
/// - In the condition where the service need to interact with both Core and RTP, then it's a EngineServices.<br/>
/// <br/>
/// The core need to be as decoupled as possible from any other parts of the engine. Meaning that even if the RTP or UI are not present, the core should still be able to function properly.<br/>
/// But not the other way around, the RTP and UI can depend on the core to function properly as the core will always be present, and if not, then the engine is not supposed to work at all.<br/>
/// </summary>
public static class RuntimeServices
{ 
    private static readonly RuntimeServicesProvider ServiceProvider = new();
    
    private static readonly Dictionary<Type, List<Action<IService>>> ServiceReadyCallbacks = new();
    private static readonly object ServiceReadyLock = new object();
    
    // ReSharper disable MemberCanBePrivate.Global
    public static void RegisterService<T>(T service, string groupName) where T : class, IService
    {
        if(string.Equals(groupName, "default", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("[Runtime] 'default' is a reserved group name. Use a different name for the service group.");
        }
        
        ServiceProvider.RegisterService(service, groupName);
    }
    public static T GetService<T>(string groupName = "default") where T : class, IService
    {
        if (ServiceProvider.TryGetService<T>(out var service, groupName))
        {
            return service;
        }

        throw new InvalidOperationException($"[Runtime] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during Runtime initialization.");
    }
    
    private static void RegisterService<T>(T service) where T : class, IService
    {
        ServiceProvider.RegisterService(service, "default");
        InvokeServiceReadyCallbacks(service);
    }
    
    private static void InvokeServiceReadyCallbacks<T>(T service) where T : class, IService
    {
        lock (ServiceReadyLock)
        {

            if (ServiceReadyCallbacks.TryGetValue(typeof(T), out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback(service);
                }

                ServiceReadyCallbacks.Remove(typeof(T));
            }
        }
    }
    
    public static IMapService MapService
    {
        get => GetService<IMapService>();
        set => RegisterService(value);
    }
    public static ILayerService LayerService
    {
        get => GetService<ILayerService>();
        set => RegisterService(value);
    }
    public static ICameraService CameraService
    {
        get => GetService<ICameraService>();
        set => RegisterService(value);
    }
    public static IRenderService RenderService
    {
        get => GetService<IRenderService>();
        set => RegisterService(value);
    }
    
    public static IChunkService ChunkService
    {
        get => GetService<IChunkService>();
        set => RegisterService(value);
    }
    
    public static IGameRunner GameRunner
    {
        get => GetService<IGameRunner>();
        set => RegisterService(value);
    }
    
    public static IGameSession GameSession
    {
        get => GetService<IGameSession>();
        set => RegisterService(value);
    }

    public static IPlayerController PlayerController
    {
        get => GetService<IPlayerController>();
        set => RegisterService(value);
    }

    public static ISimpleEventExecutor SimpleEventExecutor
    {
        get => GetService<ISimpleEventExecutor>();
        set => RegisterService(value);
    }
    
    /// <summary>
    /// Checks if a service is ready (registered) in the runtime services provider.
    /// </summary>
    /// <param name="groupName">The group name of the service. Defaults to "default".</param>
    /// <typeparam name="T">The type of the service to check.</typeparam>
    /// <returns>
    /// True if the service is registered and ready; otherwise, false.
    /// </returns>
    public static bool IsServiceReady<T>(string groupName = "default") where T : class, IService
    {
        return ServiceProvider.TryGetService<T>(out _, groupName);
    }
    
    /// <summary>
    /// Executes the provided action once the specified service is ready (registered).<br/>
    /// If the service is already registered, the action is executed immediately.<br/>
    /// Otherwise, the action is queued and will be executed when the service becomes available.
    /// </summary>
    /// <param name="action">The action to execute with the service instance.</param>
    /// <param name="groupName">The group name of the service. Defaults to "default".</param>
    /// <typeparam name="T">The type of the service.</typeparam>
    public static void OnceServiceReady<T>(Action<T> action, string groupName = "default") where T : class, IService
    {
        lock (ServiceReadyLock)
        {
            if (ServiceProvider.TryGetService<T>(out var service, groupName))
            {
                action(service);
            }
            else
            {
                if (!ServiceReadyCallbacks.TryGetValue(typeof(T), out var callbacks))
                {
                    callbacks = new List<Action<IService>>();
                    ServiceReadyCallbacks[typeof(T)] = callbacks;
                }

                callbacks.Add(svc => action((T)svc));
            }
        }
    }
    
}
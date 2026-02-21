// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Assets;
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
    private static readonly Dictionary<Type, List<Action<IService>>> ServiceReadyCallbacks = new();
    private static readonly object ServiceReadyLock = new object();

    
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

    public static IEngineConfig EngineConfig
    {
        get => GetService<IEngineConfig>();
        set => RegisterService(value);
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

    public static ISerializerService SerializerService
    {
        get => GetService<ISerializerService>();
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
    
    public static IToolService ToolService
    {
        get => GetService<IToolService>();
        set => RegisterService(value);
    }

    public static IGlobalPathData GlobalPathData
    {
        get => GetService<IGlobalPathData>();
        set => RegisterService(value);
    }

    public static IGlobalContextProvider GlobalContextProvider
    {
        get => GetService<IGlobalContextProvider>();
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
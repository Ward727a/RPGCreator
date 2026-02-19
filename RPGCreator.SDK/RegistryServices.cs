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
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.EditorUiService;

namespace RPGCreator.SDK;

public class RegistryServicesProvider : IServiceProvider
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

        throw new CriticalEngineException($"[Registry] Critical Service Missing: {typeof(T).Name} (Group: '{groupName}')", _services);
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
            throw new InvalidOperationException($"[UI] Service '{typeof(T).Name}' already registered in group '{groupName}'.");
        }

        groups[groupName] = service;
    }
}

public static class RegistryServices
{
    private static readonly EditorUiServicesProvider ServiceProvider = new();
    private static readonly Dictionary<Type, List<Action<IService>>> ServiceReadyCallbacks = new();
    private static readonly object ServiceReadyLock = new object();
    
    // ReSharper disable MemberCanBePrivate.Global
    public static void RegisterService<T>(T service, string groupName) where T : class, IService
    {
        if(string.Equals(groupName, "default", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("[Registry] 'default' is a reserved group name. Use a different name for the service group.");
        }
        
        ServiceProvider.RegisterService(service, groupName);
    }
    public static T GetService<T>(string groupName = "default", T? defaultInstance = null) where T : class, IService
    {
        if (ServiceProvider.TryGetService<T>(out var service, groupName))
        {
            return service;
        }

        #if !DEBUG
        if(defaultInstance == null)
            throw new InvalidOperationException($"[Registry] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during UI initialization.");

        return defaultInstance;
        #else
        throw new InvalidOperationException($"[Registry] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during UI initialization.");
        #endif
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

    public static IUrnRegistry UrnRegistry
    {
        get => GetService<IUrnRegistry>();
        set => RegisterService(value);
    }
    
    public static IAssetTypeRegistry AssetTypeRegistry
    {
        get => GetService<IAssetTypeRegistry>();
        set => RegisterService(value);
    }

    public static IEventsRegisterService EventsRegisterService
    {
        get => GetService<IEventsRegisterService>();
        set => RegisterService(value);
    }

    public static ISimpleEventRegistry SimpleEventRegistry
    {
        get => GetService<ISimpleEventRegistry>();
        set => RegisterService(value);
    }

    public static ISignalRegistry SignalRegistry
    {
        get => GetService<ISignalRegistry>();
        set => RegisterService(value);
    }

    public static IActionRegistry ActionRegistry
    {
        get => GetService<IActionRegistry>();
        set => RegisterService(value);
    }

    public static IToolRegistry ToolRegistry
    {
        get => GetService<IToolRegistry>();
        set => RegisterService(value);
    }
    
    #region DefaultInstance
    // All instances here SHOULD NOT be used!
    // They are only here to avoid null reference exceptions in case a service is not registered.

    public class DefaultActionRegistry : IActionRegistry 
    {
        public void RegisterAction(ActionInfo actionInfo, bool overrideIfExists = false)
        {
            Logger.Error($"[Registry] Attempted to register action '{actionInfo.DisplayName}' but no IActionRegistry service is registered. Make sure to register an IActionRegistry implementation during UI initialization.");
        }

        public void UnregisterAction(URN urn)
        {
            Logger.Error($"[Registry] Attempted to unregister action with URN '{urn}' but no IActionRegistry service is registered. Make sure to register an IActionRegistry implementation during UI initialization.");
        }

        public ActionInfo? GetAction(URN urn)
        {
            Logger.Error($"[Registry] Attempted to get action with URN '{urn}' but no IActionRegistry service is registered. Make sure to register an IActionRegistry implementation during UI initialization.");
            return null;
        }
    }
    
    #endregion
    
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
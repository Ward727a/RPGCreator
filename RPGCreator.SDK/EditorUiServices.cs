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
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using Size = System.Drawing.Size;

namespace RPGCreator.SDK;

public class EditorUiServicesProvider : IServiceProvider
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

        throw new CriticalEngineException($"[UI] Critical Service Missing: {typeof(T).Name} (Group: '{groupName}')", _services);
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

/// <summary>
/// This static class provides access to various UI-related services.<br/>
/// It serves as a centralized point to interact with UI functionalities within the RPG Creator engine.<br/>
/// <br/>
/// NOTE: As the SDK is made in a way that it should have as fewer dependencies as possible, this service provider as a lots of 'object' parameters.<br/>
/// It's up to the implementer to cast them to the correct types.<br/>
/// Failing to do so will result in runtime exceptions.
/// </summary>
public static class EditorUiServices
{
    private static readonly EditorUiServicesProvider ServiceProvider = new();
    private static readonly Dictionary<Type, List<Action<IService>>> ServiceReadyCallbacks = new();
    private static readonly object ServiceReadyLock = new object();
    
    // ReSharper disable MemberCanBePrivate.Global
    public static void RegisterService<T>(T service, string groupName) where T : class, IService
    {
        if(string.Equals(groupName, "default", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("[UI] 'default' is a reserved group name. Use a different name for the service group.");
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
            throw new InvalidOperationException($"[UI] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during UI initialization.");

        return defaultInstance;
        #else
        throw new InvalidOperationException($"[UI] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during UI initialization.");
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

    public static IMenuService MenuService
    {
        get => GetService<IMenuService>(defaultInstance: field);
        set => RegisterService(value);
    } = new DefaultMenuService();

    public static IDialogService DialogService
    {
        get => GetService<IDialogService>(defaultInstance: field);
        set => RegisterService(value);
    } = new DefaultDialogService();

    public static INotificationService NotificationService
    {
        get => GetService<INotificationService>(defaultInstance: field);
        set => RegisterService(value);
    } = new DefaultNotificationService();

    public static IUiExtensionManager ExtensionManager
    {
        get => GetService<IUiExtensionManager>(defaultInstance: field);
        set => RegisterService(value);
    } = new DefaultUiExtensionManager();
    
    public static IDocService DocService
    {
        get => GetService<IDocService>(defaultInstance: field);
        set => RegisterService(value);
    } = new DefaultDocService();

    public static IMonogameViewport MonogameViewport
    {
        get => GetService<IMonogameViewport>(defaultInstance: field);
        set => RegisterService(value);
    } = new DefaultMonogameViewport();

    #region DefaultInstance
    // All instances here SHOULD NOT be used!
    // They are only here to avoid null reference exceptions in case a service is not registered.
    
    public class DefaultMenuService : IMenuService
    {
        public void OpenContextMenu(object host, IEnumerable<MenuAction> actions)
        {
            Logger.Error("[UI] No IMenuService registered. Cannot open context menu.");
        }

        public void OpenContextMenu(object host, object menu)
        {
            Logger.Error("[UI] No IMenuService registered. Cannot open context menu.");
        }
    }
    
    public class DefaultDialogService : IDialogService
    {
        public Task<string?> PromptTextAsync(string title, string message, string defaultText = "", DialogStyle style = new DialogStyle(),
            bool selectAllText = true)
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show prompt dialog.");
            return Task.FromResult<string?>(null);
        }

        public Task<string?> PromptTextAsync(string title, object content, string defaultText = "", DialogStyle style = new DialogStyle(),
            bool selectAllText = true)
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show prompt dialog.");
            return Task.FromResult<string?>(null);
        }

        public Task<bool> ConfirmAsync(string title, string message, DialogStyle style = new DialogStyle(), string confirmButtonText = "OK",
            string cancelButtonText = "Cancel")
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show confirmation dialog.");
            return Task.FromResult(false);
        }

        public Task<bool> ConfirmAsync(string title, object content, DialogStyle style = new DialogStyle(), string confirmButtonText = "OK",
            string cancelButtonText = "Cancel")
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show confirmation dialog.");
            return Task.FromResult(false);
        }

        public Task ShowMessageAsync(string title, string message, DialogStyle style = new DialogStyle())
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show message dialog.");
            return Task.CompletedTask;
        }

        public Task ShowPromptAsync(string title, object content, DialogStyle style = new DialogStyle())
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show prompt dialog.");
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string title, string message, DialogStyle style = new DialogStyle())
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show error dialog.");
            return Task.CompletedTask;
        }

        public Task<T?> ShowSelectAsync<T>(string title, string message, IEnumerable<T> items, Func<T, string>? labelSelector = null,
            DialogStyle style = new DialogStyle(), string confirmButtonText = "OK", string cancelButtonText = "Cancel")
        {
            Logger.Error("[UI] No IDialogService registered. Cannot show selection dialog.");
            return Task.FromResult<T?>(default);
        }
    }
    
    public class DefaultNotificationService : INotificationService
    {
        public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info,
            NotificationOptions options = default)
        {
            Logger.Error("[UI] No INotificationService registered. Cannot show notification.");
        }

        public void ShowCustomNotification(object? content, NotificationType type = NotificationType.Info,
            NotificationOptions options = default)
        {
            Logger.Error("[UI] No INotificationService registered. Cannot show notification.");
        }
    }
    
    public class DefaultUiExtensionManager : IUiExtensionManager
    {
        public void RegisterExtension(UIRegion region, Action<object, object?> extension)
        {
            Logger.Error("[UI] No IUiExtensionManager registered. Cannot register UI extension.");
        }

        public void ApplyExtensions(UIRegion region, object targetControl, object? context = null)
        {
            Logger.Error("[UI] No IUiExtensionManager registered. Cannot apply UI extensions.");
        }
    }

    public class DefaultDocService : IDocService
    {
        public string GetDocumentation(URN topicUrn)
        {
            Logger.Error("[UI] No IDocService registered. Cannot get documentation.");
            return string.Empty;
        }

        public bool AddDocumentation(URN topicUrn, string content)
        {
            Logger.Error("[UI] No IDocService registered. Cannot add documentation.");
            return false;
        }

        public bool AddDocumentationFromPath(URN topicUrn, string path)
        {
            Logger.Error("[UI] No IDocService registered. Cannot add documentation from path.");
            return false;
        }
    }
    
    public class DefaultMonogameViewport : IMonogameViewport
    {
        public bool IsCoreReady { get; } = false;
        public event Action? OnCoreReady;

        public void Initialize()
        {
            Logger.Error("[UI] No IMonogameViewport registered. Cannot initialize viewport.");
        }
        public void CreateNewViewport(string viewportId, IntPtr bitmapControlAddress, Size sizeWanted, ViewportType viewportType)
        {
            Logger.Error("[UI] No IMonogameViewport registered. Cannot create new viewport.");
        }

        public void ResizeViewport(string viewportId, int width, int height)
        {
            Logger.Error("[UI] No IMonogameViewport registered. Cannot resize viewport.");
        }

        public BaseMonogameViewport GetViewport(string viewportId)
        {
            Logger.Error("[UI] No IMonogameViewport registered. Cannot get viewport.");
            return null!;
        }

        public void DestroyViewport(string viewportId)
        {
            Logger.Error("[UI] No IMonogameViewport registered. Cannot destroy viewport.");
        }

        public void Tick()
        {
            Logger.Error("[UI] No IMonogameViewport registered. Cannot tick viewports.");
        }

        public void AttachToWindow(IntPtr avaloniaWindowHandle)
        {
            Logger.Error("[UI] No IMonogameViewport registered. Cannot attach to window.");
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
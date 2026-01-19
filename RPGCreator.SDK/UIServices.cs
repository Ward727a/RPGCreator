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
using RPGCreator.SDK.UiService;

namespace RPGCreator.SDK;

public class UiServicesProvider : IServiceProvider
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

/// <summary>
/// This static class provides access to various UI-related services.<br/>
/// It serves as a centralized point to interact with UI functionalities within the RPG Creator engine.<br/>
/// <br/>
/// NOTE: As the SDK is made in a way that it should have as fewer dependencies as possible, this service provider as a lots of 'object' parameters.<br/>
/// It's up to the implementer to cast them to the correct types.<br/>
/// Failing to do so will result in runtime exceptions.
/// </summary>
public static class UiServices
{
    private static readonly UiServicesProvider ServiceProvider = new();

    
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

        throw new InvalidOperationException($"[UI] Critical Service Missing: {typeof(T).Name}. Make sure it's registered during UI initialization.");
    }

    public static IMenuService MenuService
    {
        get => GetService<IMenuService>();
        set => RegisterService(value);
    }

    public static IDialogService DialogService
    {
        get => GetService<IDialogService>();
        set => RegisterService(value);
    }
}
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

using System.Reflection;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.States;
using RPGCreator.SDK.States.Actions;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.EngineLib;

// ReSharper disable once ClassCannotBeInstantiated => False, you need to use the 'Build' method to instantiate this class.
public class EngineHost
{
    public IServiceProvider Services { get; }
    public EngineModeFlags Mode { get; }
    public ISceneManager SceneManager { get; }
    
    private EngineHost(IServiceProvider services, EngineModeFlags engineMode, ISceneManager sceneManager)
    {
        Services = services;
        Mode = engineMode;
        SceneManager = sceneManager;
    }
    
    private static Assembly[] LoadAndGetRpgCreatorAssemblies()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var loadedNames = new HashSet<string>(
            loadedAssemblies.Select(a => a.GetName().Name ?? string.Empty),
            StringComparer.OrdinalIgnoreCase);

        var toInspect = new Queue<Assembly>(loadedAssemblies.Where(a => a.FullName?.StartsWith("RPGCreator") == true));
        while (toInspect.Count > 0)
        {
            var current = toInspect.Dequeue();
            foreach (var referencedName in current.GetReferencedAssemblies())
            {
                if (referencedName.Name == null || !referencedName.Name.StartsWith("RPGCreator"))
                    continue;

                if (loadedNames.Add(referencedName.Name))
                {
                    var loaded = Assembly.Load(referencedName);
                    loadedAssemblies.Add(loaded);
                    toInspect.Enqueue(loaded);
                }
            }
        }

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        foreach (var dllPath in Directory.EnumerateFiles(baseDir, "RPGCreator.*.dll"))
        {
            var asmName = Path.GetFileNameWithoutExtension(dllPath);
            if (loadedNames.Add(asmName))
            {
                loadedAssemblies.Add(Assembly.LoadFrom(dllPath));
            }
        }

        return loadedAssemblies
            .Where(a => a.FullName != null && a.FullName.StartsWith("RPGCreator"))
            .ToArray();
    }

    public static EngineHost Build(
        Game game, SpriteBatch spriteBatch, EngineModeFlags engineMode,
        Action<IServiceCollection>? configureCustomServices = null,
        Action<IAttributeScanner, Assembly>? customAttributeScan = null)
    {
        EngineCore.StartLogger();
        Logger.Info("Engine Host started. Begin building engine services...");
        
        var services = new ServiceCollection();

        services.AddSingleton(game);
        services.AddSingleton(game.GraphicsDevice);
        services.AddSingleton(spriteBatch);

        var assemblies = LoadAndGetRpgCreatorAssemblies();

        configureCustomServices?.Invoke(services);

        services.AddLogging(); // Needed for MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        services.AddFluxor(cfg => cfg.ScanAssemblies(assemblies[0], assemblies));
        
        RegisterAttributedService(services, assemblies, engineMode);
        
        var provider = services.BuildServiceProvider();
        
        var store = provider.GetRequiredService<IStore>();
        store.InitializeAsync().GetAwaiter().GetResult();
        provider.GetRequiredService<IDispatcher>().Dispatch(new EngineActions.ChangeModeAction(engineMode));
        
        var scanner = provider.GetRequiredService<IAttributeScanner>();
        
        scanner.AutoScan(assemblies, engineMode);
        foreach (var assembly in assemblies)
        {
            customAttributeScan?.Invoke(scanner, assembly);
        }
        
        RpgEnv.PathFormat.Boot(provider.GetRequiredService<IState<ProjectState>>());
        
        var sceneManager = provider.GetRequiredService<ISceneManager>();
        
        Logger.Info("EngineHost created. All services registered.");
        
        return new EngineHost(provider, engineMode, sceneManager);
    }

    private static void RegisterAttributedService(
        IServiceCollection services,
        Assembly[] assemblies,
        EngineModeFlags engineMode)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if(type.IsAbstract || type.IsInterface) continue;
                
                var attributes = type.GetCustomAttributes<EngineServiceAttribute>();
                foreach (var attr in attributes)
                {
                    if ((attr.Mode & engineMode) == 0)
                        continue;
                    var serviceType = attr.ServiceType ?? type;

                    if (serviceType == type || attr.Lifetime == ServiceLifetime.Transient)
                    {
                        Logger.Info($"Adding service {type.Name} with lifetime {attr.Lifetime}");
                        services.Add(new ServiceDescriptor(serviceType, type, attr.Lifetime));
                    }
                    else
                    {
                        Logger.Info($"Adding service {type.Name} with lifetime {attr.Lifetime} and service type {serviceType.Name}");
                        services.TryAdd(new ServiceDescriptor(type, type, attr.Lifetime));

                        for (int i = services.Count - 1; i >= 0; i--)
                        {
                            if (services[i].ServiceType == serviceType && services[i].ImplementationType == type)
                                services.RemoveAt(i);
                        }

                        services.Add(new ServiceDescriptor(
                            serviceType,
                            sp => sp.GetRequiredService(type),
                            attr.Lifetime));
                    }
                }
            }
        }
    }
}
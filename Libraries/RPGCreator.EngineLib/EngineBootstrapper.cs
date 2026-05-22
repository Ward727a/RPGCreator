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

using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.EngineLib.Inputs;
using RPGCreator.EngineLib.Managers.AssetsManager;
using RPGCreator.EngineLib.Managers.ProjectsManager;
using RPGCreator.EngineLib.Module;
using RPGCreator.EngineLib.Parser.PRATT;
using RPGCreator.EngineLib.Registry;
using RPGCreator.EngineLib.Scheduler;
using RPGCreator.EngineLib.Services;
using RPGCreator.EngineLib.Services.StorageService;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.EngineService;

namespace RPGCreator.EngineLib;

public static class EngineBootstrapper
{
    public static void AddServices(IServiceCollection services)
    {
        // Services without dependencies
        services.AddSingleton<ISerializerService, EngineSerializer>();
        services.AddSingleton<IUndoRedoService, UndoRedoService>();
        services.AddSingleton<IFeaturesManager, FeatureManager>();
        services
            .AddSingleton<IToolService,
                ToolService>(); // Need to clean that maybe, will check that later. [Used, but weirdly, where it should not need a full service]
        services.AddSingleton<IScheduler, EngineScheduler>();
        services.AddSingleton<IPrattFormulaService, PrattFormulaService>();
        services.AddSingleton<IInputsService, InputsService>();
        services.AddSingleton<IModulePathResolver, ModulePathResolver>();
        services.AddSingleton<IGamePlayerService, GamePlayerService>();
        services.AddSingleton<IModuleManager, EngineModules>();
        services.AddSingleton<IResourceService, EngineResourcesService>();
        services.AddSingleton<ISignalRegistry, EngineSignalRegistry>();

        #region "TypesRegistry"

        var typeMapping = new TypesRegistryDiscriminator();
        typeMapping.ScanAllEngineAssemblies();
        services.AddSingleton<ITypesRegistry>(typeMapping);

        #endregion

        services.AddSingleton<INativeActionRegistry, NativeActionRegistry>();
        services.AddSingleton<IBlueprintRegistry, BlueprintRegistry>();
        services.AddSingleton<IGUIControlRegistry, GUIControlRegistry>();
        services.AddSingleton<IGuiActionRegistry, GuiActionRegistry>();
        services.AddSingleton<IGuiPropertyEditorRegistry, GuiPropertyEditorRegistry>();
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        services
            .AddSingleton<ISimpleEventRegistry,
                EngineSimpleEventRegistry>(); // Need to clean that maybe, will check that later. [Not used anymore?]
        services
            .AddSingleton<IRuntimeCompilerRegistry,
                RuntimeCompilerRegistry>(); // Need to clean that maybe, will check that later. [Not used anymore?]
        services.AddFluxor(options =>
            options.ScanAssemblies
            (
                typeof(EngineBootstrapper).Assembly, 
                typeof(RpgEnv).Assembly
            )
        );

        // Services with dependencies (1 or more)
        services.AddSingleton<IFileStorageService, JsonFileStorageService>();
        services.AddSingleton<IAssetsManager, AssetsManager>();
        services.AddSingleton<IProjectsManager, ProjectsManager>();
    }
}
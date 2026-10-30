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
using RPGCreator.EngineLib.IO.Config;
using RPGCreator.EngineLib.Managers.Assets;
using RPGCreator.EngineLib.Managers.Project;
using RPGCreator.EngineLib.Parser.PRATT;
using RPGCreator.EngineLib.Registry;
using RPGCreator.EngineLib.Scheduler;
using RPGCreator.EngineLib.Services;
using RPGCreator.EngineLib.Services.Modules;
using RPGCreator.EngineLib.Services.Modules.Features;
using RPGCreator.EngineLib.Services.Modules.Features.UiFeatureProvider;
using RPGCreator.EngineLib.Services.StorageService;
using RPGCreator.SDK;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.AttributeScanners;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Factories;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.PipelineManager;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.SDK.Tools;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Interfaces;

namespace RPGCreator.EngineLib;

public static class EngineBootstrapper
{
    public static void AddServices(IServiceCollection services)
    {
        services.AddSingleton<ISerializerService, EngineSerializer>();
        services.AddSingleton<IUndoRedoService, UndoRedoService>();
        services.AddSingleton<IScheduler, EngineScheduler>();
        services.AddSingleton<IPrattFormulaService, PrattFormulaService>();
        services.AddSingleton<IKeyedActionManager, KeyedActionManager>();
        services.AddSingleton<IGamePlayerService, GamePlayerService>();
        services.AddSingleton<IModuleService, ModuleService>();
        services.AddSingleton<IResourceManager, ResourceManager>();

        /*
         * Hey, if you got a crash here, don't worry,
         * You probably just need to clean your solution and rebuild it.
         *
         * Don't remove this comment =)
         * - Ward
         */
        services.AddFluxor(options =>
            options.ScanAssemblies
            (
                typeof(EngineBootstrapper).Assembly,
                typeof(RpgEnv).Assembly
            )
        );

        services.AddLogging();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(
                typeof(EngineBootstrapper).Assembly,
                typeof(RpgEnv).Assembly
            )
        );

        services.AddSingleton<IEngineConfig, EngineConfigManager>();


        // Services with dependencies (1 or more)
        services.AddSingleton<IFileStorageService, JsonFileStorageService>();
        services.AddSingleton<IProjectsManager, ProjectsManager>();
        services.AddSingleton<IInputsService, EngineInputs>();
        AddAttributeScannerService(services);
        AddAssetManagerService(services);
    }

    private static void AddAttributeScannerService(IServiceCollection services)
    {
        services.AddSingleton<IAttributeScanner, AttributeScanner>();
        services.AddSingleton<IAttributeHandler<AssetTypeAttribute>, AssetTypeAttributeHandler>();
        services.AddSingleton<IAttributeHandler<LayerTypeAttribute>, LayerTypeAttributeHandler>();
        services.AddSingleton<IAttributeHandler<TypeDiscriminatorAttribute>, TypeDiscriminatorAttributeHandler>();
        services.AddSingleton<IAttributeHandler<ToolLogicAttribute>, ToolLogicAttributeHandler>();
        services.AddSingleton<IAttributeHandler<ToolAttribute>, ToolAttributeHandler>();
        services.AddSingleton<IAttributeHandler<PipelineStepAttribute>, PipelineStepAttributeHandler>();
        services.AddSingleton<IAttributeScannerHandler<ToolParameterDrawerAttribute>, ToolParameterDrawerAttributeHandler>();
        services.AddSingleton<IAttributeScannerHandler<AssetTypeDrawerAttribute>, AssetDrawerAttributeHandler>();
        services.AddSingleton<IAttributeHandler<ActorFeatureAttribute>, ActorFeatureAttributeHandler>();
    }

    private static void AddAssetManagerService(IServiceCollection services)
    {
        services.AddSingleton<IAssetManager, AssetManager>();
        services.AddSingleton<IAssetCache, AssetCache>();
        services.AddSingleton<IAssetIO, AssetIO>();
        services.AddSingleton<IAssetRouter, AssetRouter>();
        services.AddSingleton<IAssetRegistry, AssetRegistry>();
        services.AddSingleton<IAssetDependencyManager, AssetDependencyManager>();
        services.AddSingleton<AssetGenericProvider>();
    }

    public static void AddModuleServices(IServiceCollection services)
    {
        services.AddSingleton<IModuleFeatureProcessor, ModuleFeatureProvider>();
        
        services.AddSingleton<IModuleFeatureRegistry, ModuleFeatureRegistry>();
        services.AddSingleton<IModuleFeatureService, ModuleFeatureService>();

        services.AddSingleton<EditorUiRegistry>();
        services.AddSingleton<IEditorUiRouter, EditorUiRouter>();
        services.AddTransient<IEditorUiRouterLocal, LocalEditorUiRouter>();
        services.AddSingleton<EditorUiFeatureProvider>();
        services.AddSingleton<IEditorUiStore, EditorUiStore>();

        services.AddSingleton<ModuleRegistry>();
        services.AddSingleton<ModuleDependencyResolver>();
        services.AddSingleton<ModuleShadowCopyManager>();
        services.AddSingleton<ModuleLifetimeService>();
        services.AddSingleton<IModuleRegistry>(sp => sp.GetRequiredService<ModuleRegistry>());
        services.AddSingleton<IModuleService, ModuleService>();
    }

    public static void AddFactories(IServiceCollection services)
    {
        services.AddSingleton<IGuiControlFactory, GuiControlFactory>();
        services.AddSingleton<IGuiActionFactory, GuiActionFactory>();
    }

    public static void AddRegistries(IServiceCollection services)
    {
        #region "TypesRegistry"

        var typeMapping = new TypesRegistryDiscriminator();
        services.AddSingleton<ITypesRegistry>(typeMapping);

        #endregion

        services.AddSingleton<ISignalRegistry, EngineSignalRegistry>();
        services.AddSingleton<IBlueprintRegistry, BlueprintRegistry>();
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        services
            .AddSingleton<ISimpleEventRegistry,
                EngineSimpleEventRegistry>(); // Need to clean that maybe, will check that later. [Not used anymore?]

        services.AddSingleton<IGuiControlRegistry, GUIControlRegistry>();
        services.AddSingleton<IGuiActionRegistry, GuiActionRegistry>();
    }
}
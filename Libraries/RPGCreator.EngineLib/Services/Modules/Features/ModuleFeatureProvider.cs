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

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules.Features;

/// <summary>
/// A module feature that can be used by other modules to provide additional functionality.<br/>
/// To use it, you need to enable the 'feature_provider' feature in your module's configuration.<br/>
/// Then create a "features" folder in your module's directory where all your JSON dto files should be placed.<br/>
/// Check <see cref="ModuleFeatureProviderDto"/> for more information.
/// </summary>
[EngineService(typeof(IModuleFeatureProcessor))]
public class ModuleFeatureProvider : IModuleFeatureProcessor
{
    private readonly ScopedLogger _logger = Logger.ForContext<ModuleFeatureProvider>();
    
    // Data
    public StringName FeatureKey => new("feature_provider");
    private readonly Dictionary<URN, List<StringName>> _registeredFeaturesByModule = new();
    private readonly ISerializerService _serializerService;
    private readonly IModuleFeatureRegistry _moduleFeatureRegistry;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Provides functionality for managing and enabling features for modules in the RPG Creator framework.
    /// Implements the <see cref="IModuleFeatureProcessor"/> interface to handle feature-related operations.
    /// </summary>
    public ModuleFeatureProvider(ISerializerService serializerService,
        IModuleFeatureRegistry moduleFeatureRegistry,
        IServiceProvider serviceProvider)
    {
        _serializerService = serializerService;
        _moduleFeatureRegistry = moduleFeatureRegistry;
        _serviceProvider = serviceProvider;
    }

    public bool CanBeEnabled(SDK.Modules.Module module) =>
        Directory.Exists(Path.Combine(module.ModulePath, "features"));

    public void EnableFeatureForModule(SDK.Modules.Module module)
    {
        foreach (var jsonFile in GetJsonFiles(module))
        {
            if (_serializerService.DeserializeFrom<ModuleFeatureProviderDto>(jsonFile, out var dto))
            {
                GetTargetClassType(module, dto, jsonFile).OnSuccess(targetType=>
                {
                    Load(targetType, module, dto, jsonFile);
                });
            }
        }
    }

    public void DisableFeatureForModule(SDK.Modules.Module module)
    {
        var moduleUrn = module.Manifest.ModuleUrn;
        
        if (_registeredFeaturesByModule.Remove(moduleUrn, out var featureKeys))
        {
            foreach (var key in featureKeys)
            {
                _moduleFeatureRegistry.UnregisterFeatureProcessor(key);
                _logger.Info("Unregistered dynamic feature processor '{FeatureKey}' for module '{ModuleUrn}'.",
                    args: [key, moduleUrn]);
            }
        }
    }

    #region Helper Methods

    private static string[] GetJsonFiles(SDK.Modules.Module module) =>
        Directory.GetFiles(Path.Combine(module.ModulePath, "features"), "*.json");

    private Result<Type> GetTargetClassType(SDK.Modules.Module module, ModuleFeatureProviderDto dto, string jsonFile)
    {
        if (string.IsNullOrWhiteSpace(dto.TargetClass))
        {
            _logger.Error("TargetClass is empty or missing in feature file '{File}'.", args: jsonFile);
            return Result.Fail();
        }

        var moduleAssembly = module.ModuleType?.Assembly;
        if (moduleAssembly == null)
        {
            _logger.Error("Module '{ModuleUrn}' has no loaded assembly.", args: module.Manifest.ModuleUrn);
            return Result.Fail();
        }
        
        var targetType = moduleAssembly.GetType(dto.TargetClass, false, false);
        
        if (targetType == null)
        {
            Debug.Assert(moduleAssembly.FullName != null);
            
            _logger.Error("Target class '{TargetClass}' declared in '{File}' was not found in assembly '{Assembly}'.",
                args: [dto.TargetClass, jsonFile, moduleAssembly.FullName]);
            return Result.Fail();
        }

        if (!typeof(IModuleFeatureProcessor).IsAssignableFrom(targetType) || targetType.IsAbstract || targetType.IsInterface)
        {
            _logger.Error("Target class '{TargetClass}' declared in '{File}' must be a non-abstract class implementing '{Interface}'.",
                args: [dto.TargetClass, jsonFile, nameof(IModuleFeatureProcessor)]);
            return Result.Fail();
        }

        return targetType;
    }

    private void Load(Type targetType, SDK.Modules.Module module, ModuleFeatureProviderDto dto, string jsonFile)
    {
        try
        {
            var moduleUrn = module.Manifest.ModuleUrn;
            var processorInstance = (IModuleFeatureProcessor)ActivatorUtilities.CreateInstance(_serviceProvider, targetType);
            
            if (!dto.FeatureKey.IsEmpty && processorInstance.FeatureKey != dto.FeatureKey)
            {
                Debug.Assert(targetType.FullName != null);
                
                _logger.Warning("FeatureKey mismatch in '{File}': JSON declared '{JsonKey}' but class '{Class}' defines '{ClassKey}'.",
                    args: [jsonFile, dto.FeatureKey, targetType.FullName, processorInstance.FeatureKey]);
            }

            _moduleFeatureRegistry.RegisterFeatureProcessor(processorInstance);

            if (!_registeredFeaturesByModule.TryGetValue(moduleUrn, out var list))
            {
                list = [];
                _registeredFeaturesByModule[moduleUrn] = list;
            }
            list.Add(processorInstance.FeatureKey);

            _logger.Info("Registered dynamic feature processor '{FeatureKey}' from module '{ModuleUrn}'.",
                args: [processorInstance.FeatureKey, moduleUrn]);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to instantiate feature processor '{TargetClass}': {Exception}",
                args: [dto.TargetClass, ex]);
        }
        
    }

    #endregion
}

public sealed class ModuleFeatureProviderDto
{
    /// <summary>
    /// Represents a unique identifier for a specific feature within a module.
    /// It is implemented using the <see cref="StringName"/> type to optimize memory usage
    /// and ensure consistent string management across the application.
    /// </summary>
    public StringName FeatureKey = StringName.Empty;

    /// <summary>
    /// Represents the name or designation of a target class within the module feature provider structure.
    /// It is used to store or retrieve class-specific metadata or configuration.<br/>
    /// The TargetClass needs to implement the IModuleFeatureProcessor interface to be accepted.
    /// </summary>
    public string TargetClass = "";
}
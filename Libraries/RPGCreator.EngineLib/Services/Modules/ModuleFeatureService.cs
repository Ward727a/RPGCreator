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

using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules;

[EngineService(typeof(IModuleFeatureService))]
public class ModuleFeatureService : IModuleFeatureService
{
    private readonly IModuleFeatureRegistry _featureRegistry;

    public ModuleFeatureService(IModuleFeatureRegistry featureRegistry, IEnumerable<IModuleFeatureProcessor> baseFeatureProcessors)
    {
        _featureRegistry = featureRegistry;
        
        foreach (var featureProcessor in baseFeatureProcessors)
        {
            _featureRegistry.RegisterFeatureProcessor(featureProcessor);
        }
    }

    public Result LoadFeaturesForModule(SDK.Modules.Module module)
    {
        List<StringName> missingFeatures = [];
        
        var sortedFeatures = module.Manifest.ModuleFeatures
            .OrderByDescending(f => f == "feature_provider");
        
        foreach (var declaredModuleFeature in sortedFeatures)
        {
            _featureRegistry.GetFeatureProcessor(declaredModuleFeature).OnSuccess((featureProcessor) =>
            {
                if (featureProcessor.CanBeEnabled(module))
                {
                    featureProcessor.EnableFeatureForModule(module);
                }
            }).OnFailure(_=>
            {
                missingFeatures.Add(declaredModuleFeature);
            });
        }
        
        return missingFeatures.Count > 0 ? Result.Failure($"The following features are missing: {string.Join(", ", missingFeatures)}") : Result.Success();
    }

    public void UnloadFeaturesForModule(SDK.Modules.Module module)
    {
        foreach (var manifestModuleFeature in module.Manifest.ModuleFeatures)
        {
            _featureRegistry.GetFeatureProcessor(manifestModuleFeature).OnSuccess((featureProcessor) =>
            {
                featureProcessor.DisableFeatureForModule(module);
            });
        }
    }
}
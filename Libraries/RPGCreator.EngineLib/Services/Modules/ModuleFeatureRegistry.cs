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

using System.Collections.Concurrent;
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules;

/// <summary>
/// Provides a registry for managing feature processors in a modular application architecture.
/// Feature processors implement specific functionality tied to features identified by a unique key.
/// </summary>
[EngineService(typeof(IModuleFeatureRegistry))]
public class ModuleFeatureRegistry : IModuleFeatureRegistry
{
    private readonly ConcurrentDictionary<StringName, IModuleFeatureProcessor> _featureProcessors = new();
    
    public Result RegisterFeatureProcessor(IModuleFeatureProcessor feature) =>
        !_featureProcessors.TryAdd(feature.FeatureKey, feature)
            ? Result.Failure("Feature already registered")
            : Result.Success();

    public Result UnregisterFeatureProcessor(StringName featureKey) => !_featureProcessors.Remove(featureKey, out _)
        ? Result.Failure("Feature not registered")
        : Result.Success();

    public Result<IModuleFeatureProcessor> GetFeatureProcessor(StringName featureKey) =>
        !_featureProcessors.TryGetValue(featureKey, out var featureProcessor)
            ? Result<IModuleFeatureProcessor>.Failure("Feature not registered")
            : Result<IModuleFeatureProcessor>.Success(featureProcessor);

    public bool IsFeatureRegistered(StringName featureKey) => _featureProcessors.ContainsKey(featureKey);

    public void Clear() => _featureProcessors.Clear();
}
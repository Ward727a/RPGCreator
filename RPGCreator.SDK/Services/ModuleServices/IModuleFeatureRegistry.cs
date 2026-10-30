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

using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.ModuleServices;

public interface IModuleFeatureRegistry
{
    /// <summary>
    /// Registers a feature processor within the module registry service.
    /// </summary>
    /// <param name="feature">The feature processor to register.</param>
    /// <returns>
    /// A <see cref="Result"/> instance indicating whether the registration was successful.
    /// </returns>
    Result RegisterFeatureProcessor(IModuleFeatureProcessor feature);

    /// <summary>
    /// Unregisters a feature processor from the module registry service.
    /// </summary>
    /// <param name="featureKey">The key of the feature processor to unregister.</param>
    /// <returns>
    /// A <see cref="Result"/> instance indicating whether the unregistration was successful.
    /// </returns>
    Result UnregisterFeatureProcessor(StringName featureKey);

    /// <summary>
    /// Retrieves the feature processor associated with the specified feature key.
    /// </summary>
    /// <param name="featureKey">The key identifying the feature processor to retrieve.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> instance containing the retrieved <see cref="IModuleFeatureProcessor"/>
    /// if the operation is successful, or an error result if the feature processor is not found.
    /// </returns>
    Result<IModuleFeatureProcessor> GetFeatureProcessor(StringName featureKey);

    /// <summary>
    /// Determines whether a feature is registered in the module registry.
    /// </summary>
    /// <param name="featureKey">The key representing the feature to check.</param>
    /// <returns>
    /// A boolean value indicating whether the specified feature is registered.
    /// </returns>
    bool IsFeatureRegistered(StringName featureKey);

    /// <summary>
    /// Clears all registered feature processors from the module registry service.
    /// </summary>
    void Clear();
}
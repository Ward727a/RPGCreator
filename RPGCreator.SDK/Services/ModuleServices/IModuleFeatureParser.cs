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

using RPGCreator.SDK.Modules;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.ModuleServices;

/// <summary>
/// Defines the contract for processing module features in RPG Creator.<br/>
/// This should not be confounded with <see cref="IEntityFeature"/> or other game/world/entity features.
/// </summary>
public interface IModuleFeatureProcessor
{
    /// <summary>
    /// The key that will be used to identify and reference the feature within the engine and be used by other modules to enable this module feature.
    /// </summary>
    StringName FeatureKey { get; }

    /// <summary>
    /// Determines if the feature can be enabled for a given module.
    /// </summary>
    /// <param name="module">The module to check for feature enablement eligibility.</param>
    bool CanBeEnabled(Module module);

    /// <summary>
    /// Enables the feature for the provided module.
    /// </summary>
    /// <param name="module">The module for which the feature will be enabled.</param>
    void EnableFeatureForModule(Module module);

    /// <summary>
    /// Disables the feature for the specified module.
    /// </summary>
    /// <param name="module">The module for which the feature will be disabled.</param>
    void DisableFeatureForModule(Module module);
}
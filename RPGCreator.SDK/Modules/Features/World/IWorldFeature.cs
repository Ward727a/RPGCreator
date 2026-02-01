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

using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Modules.Features.World;

/// <summary>
/// A world feature define custom behavior for a world lifecycle.<br/>
/// You can implement this interface to create your own world features.<br/>
/// World features can be used to add custom systems, handle world logic, etc...
/// </summary>
public interface IWorldFeature : IDisposable
{
    /// <summary>
    /// The display name of this feature.<br/>
    /// This is used in the editor and UI to represent this feature type.
    /// <example>Dynamic Weather</example>
    /// </summary>
    public string FeatureName { get; }
    
    /// <summary>
    /// If this feature depends on other features to function correctly.<br/>
    /// This is used to ensure that all required features are present when this feature is injected into a world.
    /// <example>[ new ("rpgc://world_feature/day_cycles"), new("rpgc://game_feature/date_system") ]</example>
    /// </summary>
    public URN[] DependentFeatures { get; }
    
    /// <summary>
    /// If this feature is incompatible with other features.<br/>
    /// This is used to prevent conflicts when injecting features into a world.
    /// <example>[ new ("author://world_feature/advanced_weather") ]</example>
    /// </summary>
    public URN[] IncompatibleFeatures { get; }
    
    /// <summary>
    /// A brief description of this feature.<br/>
    /// This is used in the editor and UI to provide more context about what this feature does.
    /// <example>Allows the world to have a dynamic weather system.</example>
    /// </summary>
    public string FeatureDescription { get; }
    
    /// <summary>
    /// The feature URN uniquely identifying this feature type.<br/>
    /// This is used to register and look up features in the system.
    /// <example>rpgc://world_features/dynamic_weather</example>
    /// </summary>
    public URN FeatureUrn { get; }
    
    /// <summary>
    /// An icon representing this feature.<br/>
    /// This is used in the editor and UI to visually represent this feature type.<br/>
    /// <br/>
    /// This is fully optional, and can be an empty string if no icon is desired.
    /// </summary>
    public string FeatureIcon { get; }
    
    /// <summary>
    /// The lowest priority a feature can have.<br/>
    /// Features with this priority will be executed last.
    /// </summary>
    const int LowestFeaturePriority = int.MinValue;
    
    /// <summary>
    /// The default priority of a feature.
    /// </summary>
    const int DefaultFeaturePriority = 0;
    
    /// <summary>
    /// The priority for rendering related features.
    /// </summary>
    const int RenderingFeaturePriority = 500;
    
    /// <summary>
    /// The priority for physics related features.
    /// </summary>
    const int PhysicsFeaturePriority = 1000;
    
    /// <summary>
    /// The highest priority a feature can have.<br/>
    /// Features with this priority will be executed first.
    /// </summary>
    const int HighestFeaturePriority = int.MaxValue;
    
    /// <summary>
    /// Define the priority of the feature.<br/>
    /// Features with higher priority will be executed first.<br/>
    /// This is useful when you have multiple features that need to be executed in a specific order.
    /// </summary>
    int FeaturePriority { get; }
    
    /// <summary>
    /// Define a manual forced priority for the feature.<br/>
    /// This priority will override the <see cref="FeaturePriority"/> if set to a value other than null.<br/>
    /// This variable should NEVER be set by the feature itself, it is meant to be used by the user, and will be changed via the world editor UI.<br/>
    /// If set to null, the <see cref="FeaturePriority"/> will be used instead.<br/>
    /// This allows the user to manually adjust the priority of the feature without changing the code.<br/>
    /// Note: This value can be negative or positive.
    /// </summary>
    int? ManualForcedPriority { get; }

    /// <summary>
    /// The effective priority of the feature.<br/>
    /// This is either the <see cref="ManualForcedPriority"/> if set, or the <see cref="FeaturePriority"/> otherwise.
    /// </summary>
    public int EffectivePriority => ManualForcedPriority ?? FeaturePriority;

    /// <summary>
    /// When the feature is being set up.<br/>
    /// This is called once when the feature is registered inside the engine.<br/>
    /// You can use this to initialize any static data, register states, etc...
    /// </summary>
    void OnSetup();
    
    /// <summary>
    /// When the world is being set up.<br/>
    /// This is called once per world lifecycle.<br/>
    /// This is the moment where the world start being loaded in memory, but not yet shown to the player.<br/>
    /// You can use this to initialize systems, load data, etc...
    /// </summary>
    /// <param name="world">The world being set up.</param>
    void OnWorldSetup(IEcsWorld world);
    
    /// <summary>
    /// When the world is shown to the player.<br/>
    /// This is called once per world lifecycle, after <see cref="OnWorldSetup"/>.<br/>
    /// You can use this to start systems, spawn entities, etc...
    /// </summary>
    /// <param name="world">The world being shown.</param>
    void OnWorldShown(IEcsWorld world);
    
    /// <summary>
    /// When the world is being updated.<br/>
    /// This is called every frame while the world is running.<br/>
    /// You can use this to update systems, handle world logic, etc...
    /// </summary>
    /// <param name="world">The world being updated.</param>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    void OnWorldUpdate(IEcsWorld world, TimeSpan deltaTime);
    
    /// <summary>
    /// When the world is being drawn.<br/>
    /// This is called every frame while the world is running, after <see cref="OnWorldUpdate"/>.<br/>
    /// You can use this to handle rendering logic, post-processing, etc...
    /// </summary>
    /// <param name="world">The world being drawn.</param>
    /// <param name="deltaTime">The time elapsed since the last draw.</param>
    void OnWorldDraw(IEcsWorld world, TimeSpan deltaTime);
    
    /// <summary>
    /// When the world is being stopped.<br/>
    /// This is called once per world lifecycle, before the world is unloaded from memory.<br/>
    /// You can use this to stop systems, save data, etc...
    /// </summary>
    /// <param name="world">The world being stopped.</param>
    void OnWorldStop(IEcsWorld world);
    
    /// <summary>
    /// When the world is being torn down.<br/>
    /// This is called once per world lifecycle, after <see cref="OnWorldStop"/>.<br/>
    /// You can use this to clean up resources, unload data, etc...
    /// </summary>
    /// <param name="world">The world being torn down.</param>
    void OnWorldTeardown(IEcsWorld world);
}
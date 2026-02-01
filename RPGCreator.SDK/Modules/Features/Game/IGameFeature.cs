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

using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Modules.Features.Game;

/// <summary>
/// A game feature define a specific functionality or behavior that can be added to the game.<br/>
/// This will be started when the game start, and stopped when the game stop.<br/>
/// This allows to create custom game mechanics, systems, or enhancements that can be easily integrated into the RPG Creator engine.
/// </summary>
public interface IGameFeature : IDisposable
{
    
    /// <summary>
    /// The display name of this feature.<br/>
    /// This is used in the editor and UI to represent this feature type.
    /// <example>Date System</example>
    /// </summary>
    public string FeatureName { get; }
    
    /// <summary>
    /// If this feature depends on other features to function correctly.<br/>
    /// This is used to ensure that all required features are present when this feature is injected into the game.
    /// <example>[ new ("rpgc://game_feature/clock_system"), new("author://game_feature/very_important_feature") ]</example>
    /// </summary>
    public URN[] DependentFeatures { get; }
    
    /// <summary>
    /// If this feature is incompatible with other features.<br/>
    /// This is used to prevent conflicts when injecting features into the game.
    /// <example>[ new ("author://world_feature/advanced_weather") ]</example>
    /// </summary>
    public URN[] IncompatibleFeatures { get; }
    
    /// <summary>
    /// A brief description of this feature.<br/>
    /// This is used in the editor and UI to provide more context about what this feature does.
    /// <example>Allows the game to have a date, that can be used to know what day, month, and year the game universe is at.</example>
    /// </summary>
    public string FeatureDescription { get; }
    
    /// <summary>
    /// The feature URN uniquely identifying this feature type.<br/>
    /// This is used to register and look up features in the system.
    /// <example>rpgc://game_feature/date_system</example>
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
    /// Define the priority of the feature.<br/>
    /// Features with higher priority will be executed first.<br/>
    /// This is useful when you have multiple features that need to be executed in a specific order.
    /// </summary>
    int FeaturePriority { get; }
    
    /// <summary>
    /// Define a manual forced priority for the feature.<br/>
    /// This priority will override the <see cref="FeaturePriority"/> if set to a value other than null.<br/>
    /// This variable should NEVER be set by the feature itself, it is meant to be used by the user, and will be changed via the game settings UI.<br/>
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
    /// This feature's configuration.<br/>
    /// This data <b>WILL</b> be serialized and saved with the project.<br/>
    /// And as such, it only allows serializable data types that are stored as strings, and converted back when retrieved.
    /// </summary>
    CustomData Configuration { get; }
    
    /// <summary>
    /// Sets the configuration for this feature.<br/>
    /// This is mainly used during deserialization to restore the feature's configuration.<br/>
    /// This is an engine reserved method and should not be called directly!!
    /// </summary>
    /// <param name="configuration">The configuration data to set.</param>
    public void SetConfiguration(CustomData configuration, EngineSecurityToken token);
    
    /// <summary>
    /// When the feature is being set up.<br/>
    /// This is called once when the feature is registered inside the engine.<br/>
    /// You can use this to initialize any static data, register states, etc...
    /// </summary>
    void OnSetup();
    
    /// <summary>
    /// When the game is starting up, this method is called to initialize the feature.<br/>
    /// This is where you should set up any necessary resources, configurations, or state needed for the feature to function properly.
    /// </summary>
    void OnGameSetup();
    
    /// <summary>
    /// When the game starts, this method is called to activate the feature.<br/>
    /// This is where you should implement the core logic and behavior of the feature that should be active during the game's runtime.
    /// </summary>
    void OnGameStart();
    
    /// <summary>
    /// When the game is doing a new frame update, this method is called.<br/>
    /// This is where you should implement any per-frame logic or updates needed for the feature to function correctly.
    /// </summary>
    /// <param name="deltaTime"></param>
    void OnGameUpdate(TimeSpan deltaTime);
    
    /// <summary>
    /// When the game is drawing a new frame, this method is called.<br/>
    /// This is where you should implement any rendering or drawing logic needed for the feature to visually represent itself in the game.
    /// </summary>
    /// <param name="deltaTime"></param>
    void OnGameDraw(TimeSpan deltaTime);
    
    /// <summary>
    /// When the game is stopping, this method is called to deactivate the feature.<br/>
    /// This is where you should clean up any resources, save state, or perform any necessary shutdown procedures for the feature.
    /// </summary>
    void OnStopGame();
}
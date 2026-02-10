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

using System.Runtime.CompilerServices;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Modules.Features.Game;

public abstract class BaseGameFeature : IGameFeature
{
    protected const string FeatureUrnModule = "game_features";
    
    public abstract URN FeatureUrn { get; }
    public abstract int FeaturePriority { get; }
    public abstract string FeatureName { get; }
    public abstract string FeatureDescription { get; }
    public virtual URN[] DependentFeatures { get; } = [];
    public virtual URN[] IncompatibleFeatures { get; } = [];
    public virtual string FeatureIcon { get; } = "";
    public int? ManualForcedPriority { get; } = null;
    
    public int EffectivePriority => ManualForcedPriority ?? FeaturePriority;

    public CustomData Configuration { get; private set; } = new();
    
    /// <summary>
    /// Sets the configuration for this feature.<br/>
    /// This is mainly used during deserialization to restore the feature's configuration.<br/>
    /// This is an engine reserved method and should not be called directly!!
    /// </summary>
    /// <param name="configuration">The configuration data to set.</param>
    public void SetConfiguration(CustomData configuration, EngineSecurityToken token)
    {
        if(token == null) 
            throw new UnauthorizedAccessException("EngineSecurityToken is required to set the feature configuration.");

        Configuration = configuration;
    }

    public abstract void OnSetup();

    public virtual void OnGameStart()
    {
    }
    
    public virtual void OnGameSetup()
    {
    }

    public virtual void OnGameUpdate(TimeSpan deltaTime)
    {
    }

    public virtual void OnGameDraw(TimeSpan deltaTime)
    {
    }

    public virtual void OnStopGame()
    {
    }
    
    /// <summary>
    /// Accessor for configuration values with a default fallback.<br/>
    /// Uses the caller member name as the key if none is provided.
    /// </summary>
    /// <param name="defaultValue"> The default value to return if the key does not exist.</param>
    /// <param name="key"> The configuration key. Defaults to the caller member name.</param>
    /// <typeparam name="T"> The type of the configuration value.</typeparam>
    /// <example>
    /// <code language="csharp">
    /// // Usage:
    /// int mySetting = GetConfig(42); // Retrieves the value for "mySetting" or returns 42 if not set.
    ///
    /// int anotherSetting = GetConfig(100, "customKey"); // Retrieves the value for "customKey" or returns 100 if not set.
    ///
    /// int getSetSetting {
    ///   get => GetConfig(10); // Retrieves the value for "getSetSetting" or returns 10 if not set.
    ///   set => SetConfig(value); // Sets the value for "getSetSetting".
    /// }
    /// </code>
    /// </example>
    /// <returns> The configuration value associated with the key, or the default value if the key does not exist.</returns>
    protected T GetConfig<T>(T defaultValue, [CallerMemberName] string key = "")
    {
        return Configuration.GetAsOrDefault(key, defaultValue);
    }

    /// <summary>
    /// Sets a configuration value.<br/>
    /// Uses the caller member name as the key if none is provided.
    /// </summary>
    /// <param name="value"> The value to set.</param>
    /// <param name="key"> The configuration key. Defaults to the caller member name.</param>
    /// <typeparam name="T"> The type of the configuration value.</typeparam>
    /// <example>
    /// <code language="csharp">
    /// // Usage:
    /// int mySetting = GetConfig(42); // Retrieves the value for "mySetting" or returns 42 if not set.
    ///
    /// int anotherSetting = GetConfig(100, "customKey"); // Retrieves the value for "customKey" or returns 100 if not set.
    ///
    /// int getSetSetting {
    ///   get => GetConfig(10); // Retrieves the value for "getSetSetting" or returns 10 if not set.
    ///   set => SetConfig(value); // Sets the value for "getSetSetting".
    /// }
    /// </code>
    /// </example>
    protected void SetConfig<T>(T value, [CallerMemberName] string key = "")
    {
        Configuration.Set(key, value);
    }

    public virtual void Dispose()
    {
        
    }
}
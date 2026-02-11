using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Modules.Features.Entity;


/// <summary>
/// Just an extension class to provide shared custom data access for BaseEntityFeature instances.
/// </summary>
public static class SharedDataFeatures
{
    private static readonly ConcurrentDictionary<URN, CustomData> SharedCustomDataCache = new();

    /// <summary>
    /// Retrieves the shared custom data for the given feature.<br/>
    /// This data is shared across all instances of the feature type, allowing for global settings or configurations.
    /// </summary>
    /// <param name="feature">The feature instance for which to retrieve the shared custom data.</param>
    /// <returns>
    /// The shared custom data associated with the feature type.
    /// </returns>
    internal static CustomData GetSharedCustomData(this BaseEntityFeature feature)
    {
        return SharedCustomDataCache.GetOrAdd(feature.FeatureUrn, _ => new CustomData());
    }

    internal static void SetSharedCustomData(URN featureUrn, CustomData customData)
    {
        SharedCustomDataCache[featureUrn] = customData;
    }
    
    internal static CustomData GetSharedCustomData(URN featureUrn)
    {
        return SharedCustomDataCache.GetOrAdd(featureUrn, _ => new CustomData());
    }
    
    internal static void SetValueShared<T>(URN featureUrn, string key, T value)
    {
        var customData = GetSharedCustomData(featureUrn);
        customData.Set(key, value);
    }
    
    internal static T GetValueShared<T>(URN featureUrn, string key, T defaultValue)
    {
        var customData = GetSharedCustomData(featureUrn);
        return customData.GetAsOrDefault(key, defaultValue);
    }
}

public abstract class BaseEntityFeature : IEntityFeature
{
    
    /// <summary>
    /// Just the module URN where all entity features are stored.<br/>
    /// This is mainly a constant to avoid hardcoding the string everywhere.
    /// </summary>
    protected static UrnSingleModule FeatureUrnModule => "entity_features".ToUrnSingleModule();
    
    /// <summary>
    /// Just the module URN where all entity feature tags are stored.<br/>
    /// This is mainly a constant to avoid hardcoding the string everywhere.
    /// </summary>
    protected const string TagsUrnModule = "tags";

    /// <summary>
    /// If this feature depends on other features to function correctly.<br/>
    /// This is used to ensure that all required features are present when this feature is injected into an entity.
    /// <example>[ new ("rpgc://entity_features/HasInventory"), new("rpgc://entity_features/CanEquipItems") ]</example>
    /// </summary>
    public virtual URN[] DependentFeatures { get; } = [];

    /// <summary>
    /// If this feature is incompatible with other features.<br/>
    /// This is used to prevent conflicts when injecting features into an entity.
    /// <example>[ new ("author://entity_features/AugmentedMovement") ]</example>
    /// </summary>
    public virtual URN[] IncompatibleFeatures { get; } = [];

    /// <summary>
    /// The display name of this feature.<br/>
    /// This is used in the editor and UI to represent this feature type.
    /// <example>Has Inventory</example>
    /// </summary>
    public abstract string FeatureName { get; }
    
    /// <summary>
    /// A brief description of this feature.<br/>
    /// This is used in the editor and UI to provide more context about what this feature does.
    /// <example>Allows the entity to have an inventory system for storing items.</example>
    /// </summary>
    public abstract string FeatureDescription { get; }

    /// <summary>
    /// An icon representing this feature.<br/>
    /// This is used in the editor and UI to visually represent this feature type.<br/>
    /// <br/>
    /// This is fully optional, and can be an empty string if no icon is desired.
    /// </summary>
    public virtual string FeatureIcon { get; } = "";

    /// <summary>
    /// The feature URN uniquely identifying this feature type.<br/>
    /// This is used to register and look up features in the system.
    /// <example>rpgc://entity_features/HasInventory</example>
    /// </summary>
    public abstract URN FeatureUrn { get; }
    
    /// <summary>
    /// Cached reference to the shared memory configuration.<br/>
    /// This is used to avoid multiple lookups for the shared custom data.
    /// </summary>
    private CustomData? _cachedSharedMemoryConfiguration;

    /// <summary>
    /// The shared memory configuration for this feature.<br/>
    /// This data <b>WILL</b> be shared across all instances of this feature in the same ECS world (e.g. for global settings).<br/>
    /// And as such, it only allows serializable data types that are stored as strings, and converted back when retrieved.
    /// </summary>
    public CustomData SharedMemoryConfiguration
    {
        get
        {
            _cachedSharedMemoryConfiguration ??= this.GetSharedCustomData();
            return _cachedSharedMemoryConfiguration;
        }
    }
    
    /// <summary>
    /// This feature's configuration.<br/>
    /// This data <b>WILL</b> be serialized and saved with the project.<br/>
    /// And as such, it only allows serializable data types that are stored as strings, and converted back when retrieved.
    /// </summary>
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

    public virtual void OnAddingToDefinition(IEntityDefinition definition, object itemControl)
    {
    }

    /// <summary>
    /// Called when this feature is added to an entity definition.<br/>
    /// This is called once when the feature is added to the definition, allowing it to perform any necessary setup or registration.
    /// </summary>
    /// <param name="definition">The entity definition to which this feature is being added.</param>
    public virtual bool OnAddedToDefinition(IEntityDefinition definition)
    {
        return true;
    }
    
    /// <summary>
    /// Called when this feature is removed from an entity definition.<br/>
    /// This is called once when the feature is removed from the definition, allowing it to perform any necessary cleanup or deregistration.
    /// </summary>
    /// <param name="definition">The entity definition from which this feature is being removed.</param>
    public virtual void OnRemovedFromDefinition(IEntityDefinition definition) { }

    /// <summary>
    /// When this feature is initialized (created).<br/>
    /// This is called once when the feature instance is created, before being injected into any entity, when the engine loads the feature definitions.
    /// </summary>
    public virtual void OnSetup() { }
    
    /// <summary>
    /// When the ECS world is being set up.<br/>
    /// This is called once when the ECS world is initialized, allowing the feature to register any necessary systems.<br/>
    /// Note: This is called only once per world, and ONLY if any entity in the world has this feature.
    /// </summary>
    /// <param name="world"></param>
    public virtual void OnWorldSetup(IEcsWorld world) { }

    /// <summary>
    /// When this feature need to be injected (added on runtime) on an entity.
    /// </summary>
    /// <param name="entity">The entity on which this feature is being injected.</param>
    /// <param name="entityDefinition"></param>
    public virtual void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition) { }

    /// <summary>
    /// When this feature is being destroyed (removed) from an entity (e.g. when the entity is deleted).<br/>
    /// This is the last chance to clean up any resources or references related to this feature on the entity.
    /// </summary>
    /// <param name="entity">The entity from which this feature is being removed.</param>
    public virtual void OnDestroy(BufferedEntity entity) { }

    /// <summary>
    /// Accessor for configuration values with a default fallback.<br/>
    /// Uses the caller member name as the key if none is provided.<br/>
    /// Each config is unique for each feature, for each entity!
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
    /// Uses the caller member name as the key if none is provided.<br/>
    /// Each config is unique for each feature, for each entity!
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
    
    /// <summary>
    /// Get a value from the shared memory configuration with a default fallback.<br/>
    /// Uses the caller member name as the key if none is provided.<br/>
    /// <br/>
    /// The shared memory configuration is shared across all instances of this feature in the same ECS world (e.g. for global settings).<br/>
    /// It works similarly to GetConfig, but operates on the shared memory configuration.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the key does not exist.</param>
    /// <param name="key">The configuration key. Defaults to the caller member name.</param>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    /// <returns>
    /// The configuration value associated with the key, or the default value if the key does not exist.
    /// </returns>
    protected T GetShared<T>(T defaultValue, [CallerMemberName] string key = "")
    {
        return SharedMemoryConfiguration.GetAsOrDefault(key, defaultValue);
    }
    
    /// <summary>
    /// Sets a value in the shared memory configuration.<br/>
    /// Uses the caller member name as the key if none is provided.<br/>
    /// <br/>
    /// The shared memory configuration is shared across all instances of this feature in the same ECS world (e.g. for global settings).<br/>
    /// It works similarly to SetConfig, but operates on the shared memory configuration.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="key">The configuration key. Defaults to the caller member name.</param>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    protected void SetShared<T>(T value, [CallerMemberName] string key = "")
    {
        SharedMemoryConfiguration.Set(key, value);
    }

    public virtual IEntityFeature Clone()
    {
        var clone = (BaseEntityFeature)MemberwiseClone();
        clone.Configuration = this.Configuration.Clone();
        clone.OnAfterClone();
        return clone;
    }
    
    /// <summary>
    /// When this feature has been cloned.<br/>
    /// This is called after the feature has been duplicated to allow for any necessary adjustments or initializations.
    /// </summary>
    protected virtual void OnAfterClone()
    {
    }

    /// <summary>
    /// To reset the feature's state to its default configuration.<br/>
    /// This can be used to clear any runtime data or settings specific to this feature.
    /// </summary>
    public virtual void Reset()
    {
    }

    public virtual void Dispose()
    {
    }
}
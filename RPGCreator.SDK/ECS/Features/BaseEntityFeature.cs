using System.Runtime.CompilerServices;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.Modules.Definition;

namespace RPGCreator.SDK.ECS.Features;

public abstract class BaseEntityFeature : IEntityFeature
{
    /// <summary>
    /// This feature's configuration.<br/>
    /// This data <b>WILL</b> be serialized and saved with the project.<br/>
    /// And as such, it only allows serializable data types that are stored as strings, and converted back when retrieved.
    /// </summary>
    public CustomData Configuration { get; private set; } = new();
    
    /// <summary>
    /// When this feature is initialized (created) on an entity.<br/>
    /// </summary>
    /// <param name="entity">The entity on which this feature is being initialized.</param>
    public abstract void OnInitialize(IEntity entity);

    /// <summary>
    /// When the entity is being updated.<br/>
    /// Do not confuse with drawing - This is for logic updates only.
    /// </summary>
    /// <param name="entity">The entity being updated.</param>
    /// <param name="deltaTime">The game time.</param>
    public virtual void OnUpdate(Entity entity, double deltaTime)
    {
    }

    /// <summary>
    /// When the entity is being drawn.<br/>
    /// Do not confuse with updating - This is for drawing only.
    /// </summary>
    /// <param name="entity">The entity being drawn.</param>
    /// <param name="deltaTime">The game time.</param>
    public virtual void OnDraw(Entity entity, double deltaTime)
    {
    }

    /// <summary>
    /// When this feature is being destroyed (removed) from an entity (e.g. when the entity is deleted).<br/>
    /// This is the last chance to clean up any resources or references related to this feature on the entity.
    /// </summary>
    /// <param name="entity"></param>
    public abstract void OnDestroy(IEntity entity);

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
        return Configuration.GetOrDefault(key, defaultValue);
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
}
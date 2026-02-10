using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Modules.Features.Entity;

public interface IEntityFeature : IDisposable
{
    
    /// <summary>
    /// The display name of this feature.<br/>
    /// This is used in the editor and UI to represent this feature type.
    /// <example>Has Inventory</example>
    /// </summary>
    public string FeatureName { get; }
    
    /// <summary>
    /// If this feature depends on other features to function correctly.<br/>
    /// This is used to ensure that all required features are present when this feature is injected into an entity.
    /// <example>[ new ("rpgc://entity_features/HasInventory"), new("rpgc://entity_features/CanEquipItems") ]</example>
    /// </summary>
    public URN[] DependentFeatures { get; }
    
    /// <summary>
    /// If this feature is incompatible with other features.<br/>
    /// This is used to prevent conflicts when injecting features into an entity.
    /// <example>[ new ("author://entity_features/AugmentedMovement") ]</example>
    /// </summary>
    public URN[] IncompatibleFeatures { get; }
    
    /// <summary>
    /// A brief description of this feature.<br/>
    /// This is used in the editor and UI to provide more context about what this feature does.
    /// <example>Allows the entity to have an inventory system for storing items.</example>
    /// </summary>
    public string FeatureDescription { get; }
    
    /// <summary>
    /// The feature URN uniquely identifying this feature type.<br/>
    /// This is used to register and look up features in the system.
    /// <example>rpgc://entity_features/HasInventory</example>
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
    /// The shared memory configuration for this feature.<br/>
    /// This data <b>WILL</b> be shared across all instances of this feature in the same ECS world (e.g. for global settings).<br/>
    /// And as such, it only allows serializable data types that are stored as strings, and converted back when retrieved.
    /// </summary>
    public CustomData SharedMemoryConfiguration { get; }
    
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
    /// Called before this feature is added to an entity definition.<br/>
    /// This is called once when the feature is added to the definition, allowing it to perform any necessary validation or preparation before being added to the definition.
    /// </summary>
    /// <param name="definition">The entity definition to which this feature is being added.</param>
    /// <param name="itemControl">The item control associated with this feature in the editor, allowing it to modify the control or add additional controls if necessary.</param>
    void OnAddingToDefinition(IEntityDefinition definition, object itemControlOrContext);
    
    /// <summary>
    /// Called when this feature is added to an entity definition.<br/>
    /// This is called once when the feature is added to the definition, allowing it to perform any necessary setup or registration.
    /// </summary>
    /// <param name="definition">The entity definition to which this feature is being added.</param>
    bool OnAddedToDefinition(IEntityDefinition definition);
    
    /// <summary>
    /// Called when this feature is removed from an entity definition.<br/>
    /// This is called once when the feature is removed from the definition, allowing it to perform any necessary cleanup or deregistration.
    /// </summary>
    /// <param name="definition">The entity definition from which this feature is being removed.</param>
    void OnRemovedFromDefinition(IEntityDefinition definition);
    
    /// <summary>
    /// When this feature is initialized (created).<br/>
    /// This is called once when the feature instance is created, before being injected into any entity, when the engine loads the feature definitions.
    /// </summary>
    void OnSetup();
    
    /// <summary>
    /// When the ECS world is being set up.<br/>
    /// This is called once when the ECS world is initialized, allowing the feature to register any necessary systems.<br/>
    /// Note: This is called only once per world, and ONLY if any entity in the world has this feature.
    /// </summary>
    /// <param name="world"></param>
    public void OnWorldSetup(IEcsWorld world);

    /// <summary>
    /// When this feature is injected (added on runtime) on an entity.<br/>
    /// This is called in a separate feature for each entity that has this feature injected, allowing it to perform any necessary setup or initialization related to the specific entity.<br/>
    /// Note: This is called for EACH entity that has this feature injected.
    /// </summary>
    /// <param name="entity">The entity on which this feature is being injected.</param>
    /// <param name="entityDefinition">The entity definition of the entity on which this feature is being injected. This can be used to access other features or data on the entity.</param>
    void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition);

    /// <summary>
    /// When this feature is being destroyed (removed) from an entity (e.g. when the entity is deleted).<br/>
    /// This is the last chance to clean up any resources or references related to this feature on the entity.
    /// </summary>
    /// <param name="entity"></param>
    void OnDestroy(BufferedEntity entity);

    public IEntityFeature Clone();
    public void Reset();
}
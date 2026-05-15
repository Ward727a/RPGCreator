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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Modules.Features.Game;
using RPGCreator.SDK.Modules.Features.World;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;

public sealed record EntityFeaturePropertyMetadata(
    PropertyInfo PropertyInfo,
    EntityFeaturePropertyAttribute Attribute,
    Type PropertyType);
public sealed record WorldFeaturePropertyMetadata(
    PropertyInfo PropertyInfo,
    WorldFeaturePropertyAttribute Attribute,
    Type PropertyType);

public sealed record GameFeaturePropertyMetadata(
    PropertyInfo PropertyInfo,
    GameFeaturePropertyAttribute Attribute,
    Type PropertyType);

public sealed record EntityFeatureAnimationRequirement(string AnimDisplayName, URN AnimUrn, AnimationDirection Direction);

/// <summary>
/// The features manager service.<br/>
/// Manages all features available in the engine/editor.<br/>
/// This service is responsible for registering, retrieving, and managing features that can be applied to entities, world, or game (Entity Features, World Features, Game Features).
/// </summary>
public interface IFeaturesManager : IService
{
    
    /// <summary>
    /// Registers a new entity feature type.<br/>
    /// The feature type must implement <see cref="IEntityFeature"/>.<br/>
    /// This method uses generics to specify the feature type to register.
    /// </summary>
    /// <typeparam name="T">The type of the entity feature to register.</typeparam>
    void RegisterEntityFeature<T>() where T : IEntityFeature, new();
    
    void OnceEntityFeaturesRegistered(URN feature, Action<URN> callback);
    
    /// <summary>
    /// Registers a new world feature type.<br/>
    /// The feature type must implement <see cref="IWorldFeature"/>.<br/>
    /// This method uses generics to specify the feature type to register.
    /// </summary>
    /// <typeparam name="T">The type of the world feature to register.</typeparam>
    void RegisterWorldFeature<T>() where T : IWorldFeature, new();
    
    /// <summary>
    /// Registers a new game feature type.<br/>
    /// The feature type must implement <see cref="IGameFeature"/>.<br/>
    /// This method uses generics to specify the feature type to register.
    /// </summary>
    /// <typeparam name="T">The type of the game feature to register.</typeparam>
    void RegisterGameFeature<T>() where T : IGameFeature, new();
    
    /// <summary>
    /// Checks if an entity feature with the specified URN is registered.
    /// </summary>
    /// <param name="featureUrn">The URN of the entity feature to check.</param>
    /// <returns>
    /// True if the entity feature is registered; otherwise, false.
    /// </returns>
    public bool HasEntityFeature(URN featureUrn);
    
    /// <summary>
    /// Checks if a world feature with the specified URN is registered.
    /// </summary>
    /// <param name="featureUrn">The URN of the world feature to check.</param>
    /// <returns>
    /// True if the world feature is registered; otherwise, false.
    /// </returns>
    public bool HasWorldFeature(URN featureUrn);
    
    /// <summary>
    /// Checks if a game feature with the specified URN is registered.
    /// </summary>
    /// <param name="featureUrn">The URN of the game feature to check.</param>
    /// <returns>
    /// True if the game feature is registered; otherwise, false.
    /// </returns>
    public bool HasGameFeature(URN featureUrn);
    
    /// <summary>
    /// Unregisters an entity feature with the specified URN.
    /// </summary>
    /// <param name="featureUrn">The URN of the entity feature to unregister.</param>
    void UnregisterEntityFeature(URN featureUrn);
    
    /// <summary>
    /// Unregisters a world feature with the specified URN.
    /// </summary>
    /// <param name="featureUrn">The URN of the world feature to unregister.</param>
    void UnregisterWorldFeature(URN featureUrn);
    
    /// <summary>
    /// Unregisters a game feature with the specified URN.
    /// </summary>
    /// <param name="featureUrn">The URN of the game feature to unregister.</param>
    void UnregisterGameFeature(URN featureUrn);
    
    /// <summary>
    /// Return the entity feature template associated with the specified URN.<br/>
    /// <remarks>
    /// All changes made to the returned feature will affect all entities using this feature template.<br/>
    /// To get a unique instance of the feature for an entity, use the <see cref="CreateEntityFeatureInstance"/> method.
    /// </remarks>
    /// </summary>
    /// <param name="featureUrn">The URN of the entity feature to retrieve.</param>
    /// <returns>The entity feature template associated with the specified URN.</returns>
    IEntityFeature GetEntityFeature(URN featureUrn);

    public URN GetEntityFeatureReplacedFeature(URN replacingFeature);

    /// <summary>
    /// Checks if the given entity feature is replacing another feature.
    /// </summary>
    /// <param name="featureToReplace"></param>
    /// <param name="replacingFeature"></param>
    /// <returns></returns>
    public bool IsEntityFeatureReplacing(URN featureToReplace, URN replacingFeature);
    public IEnumerable<EntityFeatureAnimationRequirement> GetEntityRequiredAnimations(URN feature);
    public bool AddEntityFeatureAnimationRequirement(URN featureUrn, EntityFeatureAnimationRequirement requirement);
    
    
    /// <summary>
    /// Return the world feature template associated with the specified URN.
    /// </summary>
    /// <param name="featureUrn">The URN of the world feature to retrieve.</param>
    /// <returns>The world feature template associated with the specified URN.</returns>
    IWorldFeature GetWorldFeature(URN featureUrn);
    
    /// <summary>
    /// Return the game feature template associated with the specified URN.
    /// </summary>
    /// <param name="featureUrn">The URN of the game feature to retrieve.</param>
    /// <returns>The game feature template associated with the specified URN.</returns>
    IGameFeature GetGameFeature(URN featureUrn);
    
    /// <summary>
    /// Tries to get the entity feature INSTANCE associated with the specified URN.<br/>
    /// If found, the feature is returned via the out parameter and the method returns true.<br/>
    /// If not found, the out parameter is set to null and the method returns false.<br/>
    /// <remarks>This method retrieves the actual feature instance, not the template, and as such can be modified without affecting other entities.</remarks>
    /// </summary>
    /// <param name="featureUrn">The URN of the entity feature to retrieve.</param>
    /// <param name="feature">The out parameter to hold the retrieved entity feature instance if found.</param>
    /// <returns>
    /// True if the entity feature instance is found; otherwise, false.
    /// </returns>
    bool TryGetEntityFeature(URN featureUrn, [NotNullWhen(true)] out IEntityFeature? feature);
    
    /// <summary>
    /// Tries to get the world feature associated with the specified URN.<br/>
    /// If found, the feature is returned via the out parameter and the method returns true.<br/>
    /// If not found, the out parameter is set to null and the method returns false.
    /// </summary>
    /// <param name="featureUrn">The URN of the world feature to retrieve.</param>
    /// <param name="feature">The out parameter to hold the retrieved world feature if found.</param>
    /// <returns>
    /// True if the world feature is found; otherwise, false.
    /// </returns>
    bool TryGetWorldFeature(URN featureUrn, [NotNullWhen(true)] out IWorldFeature? feature);
    
    /// <summary>
    /// Tries to get the game feature associated with the specified URN.<br/>
    /// If found, the feature is returned via the out parameter and the method returns true.<br/>
    /// If not found, the out parameter is set to null and the method returns false.
    /// </summary>
    /// <param name="featureUrn">The URN of the game feature to retrieve.</param>
    /// <param name="feature">The out parameter to hold the retrieved game feature if found.</param>
    /// <returns>
    /// True if the game feature is found; otherwise, false.
    /// </returns>
    bool TryGetGameFeature(URN featureUrn, [NotNullWhen(true)] out IGameFeature? feature);
    
    /// <summary>
    /// Returns the entity feature instance back to the pool for reuse.<br/>
    /// This helps optimize memory usage by reusing feature instances instead of creating new ones each time.
    /// </summary>
    /// <param name="feature">The entity feature instance to return to the pool.</param>
    public void ReturnFeatureEntityToPool(IEntityFeature feature);
    
    /// <summary>
    /// Creates a new instance of the entity feature associated with the specified URN.<br/>
    /// This instance is unique and can be modified without affecting other entities using the same feature template.
    /// </summary>
    /// <param name="featureUrn">The URN of the entity feature to create an instance of.</param>
    /// <returns>A new instance of the entity feature associated with the specified URN.</returns>
    public IEntityFeature CreateEntityFeatureInstance(URN featureUrn);
    
    /// <summary>
    /// Gets all the properties metadata for the specified entity feature URN.<br/>
    /// This includes property info, attributes, and types for all properties decorated with <see cref="EntityFeaturePropertyAttribute"/>.<br/>
    /// </summary>
    /// <param name="feature">The URN of the entity feature to get properties for.</param>
    /// <returns>
    /// An enumerable of <see cref="EntityFeaturePropertyMetadata"/> containing metadata for each property of the specified entity feature.
    /// </returns>
    public IEnumerable<EntityFeaturePropertyMetadata> GetEntityProperties(URN feature);
    
    /// <summary>
    /// Gets all the properties metadata for the specified world feature URN.<br/>
    /// This includes property info, attributes, and types for all properties decorated with <see cref="WorldFeaturePropertyAttribute"/>.<br/>
    /// </summary>
    /// <param name="feature">The URN of the world feature to get properties for.</param>
    /// <returns>
    /// An enumerable of <see cref="WorldFeaturePropertyMetadata"/> containing metadata for each property of the specified world feature.
    /// </returns>
    public IEnumerable<WorldFeaturePropertyMetadata> GetWorldProperties(URN feature);
    
    /// <summary>
    /// Gets all the properties metadata for the specified game feature URN.<br/>
    /// This includes property info, attributes, and types for all properties decorated with <see cref="GameFeaturePropertyAttribute"/>.<br/>
    /// </summary>
    /// <param name="feature">The URN of the game feature to get properties for.</param>
    /// <returns>
    /// An enumerable of <see cref="GameFeaturePropertyMetadata"/> containing metadata for each property of the specified game feature.
    /// </returns>
    public IEnumerable<GameFeaturePropertyMetadata> GetGameProperties(URN feature);

    /// <summary>
    /// Returns all entity features that are macro features.
    /// </summary>
    /// <returns></returns>
    public List<IEntityFeature> GetAllMacroEntityFeatures();
    
    /// <summary>
    /// Returns all entity features that are not macro features.
    /// </summary>
    /// <returns></returns>
    public List<IEntityFeature> GetAllAtomicEntityFeatures();
    
    /// <summary>
    /// Gets all registered entity features.<br/>
    /// This is a copy of the internal list of registered entity features and as such modifying it will not affect the manager's state.
    /// </summary>
    /// <returns>Returns a list of all registered entity features.</returns>
    List<IEntityFeature> GetAllEntityFeatures();
    
    /// <summary>
    /// Returns all registered world features.<br/>
    /// This is a copy of the internal list of registered world features and as such modifying it will not affect the manager's state.
    /// </summary>
    /// <param name="inPriorityOrder">If true, the features are returned in priority order.</param>
    /// <returns>
    /// Returns a list of all registered world features.
    /// </returns>
    List<IWorldFeature> GetAllWorldFeatures(bool inPriorityOrder = false);
    
    /// <summary>
    /// Returns all registered game features.<br/>
    /// This is a copy of the internal list of registered game features and as such modifying it will not affect the manager's state.
    /// </summary>
    /// <param name="inPriorityOrder">If true, the features are returned in priority order.</param>
    /// <returns>
    /// Returns a list of all registered game features.
    /// </returns>
    List<IGameFeature> GetAllGameFeatures(bool inPriorityOrder = false);
    
    /// <summary>
    /// Removes all registered entity features from the manager.<br/>
    /// Use with caution as this will clear all entity features and may affect entities relying on them.
    /// </summary>
    void ClearAllEntityFeatures();
    
    /// <summary>
    /// Removes all registered world features from the manager.<br/>
    /// Use with caution as this will clear all world features and may affect worlds relying on them.
    /// </summary>
    void ClearAllWorldFeatures();
    
    /// <summary>
    /// Removes all registered game features from the manager.<br/>
    /// Use with caution as this will clear all game features and may affect game systems relying on them.
    /// </summary>
    void ClearAllGameFeatures();
    
    /// <summary>
    /// Removes all registered features (entity, world, and game) from the manager.<br/>
    /// Use with caution as this will clear all features and may affect entities, worlds, and game systems relying on them.
    /// </summary>
    void ClearAllFeatures();
}
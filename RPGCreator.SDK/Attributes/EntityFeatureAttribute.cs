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

using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Attributes;

/// <summary>
/// Attribute to mark a class as an entity feature for entities.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EntityFeatureAttribute() : Attribute
{
    /// <summary>
    /// Defines the maximum number of instances of this feature that can be added to a single character.<br/>
    /// Default is 1. Set to 0 for unlimited instances.
    /// </summary>
    public int MaxInstancesPerCharacter { get; set; } = 1;
    
    /// <summary>
    /// Defines the URN of an existing feature that this feature replaces when added to a character.<br/>
    /// Default is null, meaning it does not replace any existing feature.<br/>
    /// This is particularly useful when some parts of the engine expect a specific feature to be present on an entity.
    /// </summary>
    public string? ReplacingFeatureUrn { get; set; } = null;
}

/// <summary>
/// Attribute to mark a class as a macro entity feature for entities.<br/>
/// A macro entity feature is a feature that can contain multiple sub-features.<br/>
/// This allows to group features together, and simplify the management of features on entities for the user.<br/>
/// <br/>
/// Note: A macro-feature should be easy to use and understand for the user, and SHOULD NOT require complex configurations.<br/>
/// As all macro-features will be directly accessible from the editor UI for the user without having the 'advanced' mode enabled.
/// </summary>
public sealed class EntityMacroFeatureAttribute() : Attribute
{
    /// <summary>
    /// Defines the maximum number of instances of this feature that can be added to a single character.<br/>
    /// Default is 1. Set to 0 for unlimited instances.
    /// </summary>
    public int MaxInstancesPerCharacter { get; set; } = 1;
    
    /// <summary>
    /// Defines the URN of an existing feature that this feature replaces when added to a character.<br/>
    /// Default is null, meaning it does not replace any existing feature.<br/>
    /// This is particularly useful when some parts of the engine expect a specific feature to be present on an entity.
    /// </summary>
    public string? ReplacingFeatureUrn { get; set; } = null;
}

/// <summary>
/// Attribute to mark a property or field as an entity feature property.<br/>
/// This will make the property/field editable in the editor.
/// </summary>
/// <param name="name">The display name of the property.</param>
/// <param name="description">The description of the property.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EntityFeaturePropertyAttribute(string name, string description = "") : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }
    
    /// <summary>
    /// You can use this to group properties in the editor.<br/>
    /// Default is "Settings".<br/>
    /// You can create subgroups using slashes, e.g., "Settings/Advanced".<br/>
    /// </summary>
    public string Category = "Settings";
    /// <summary>
    /// Set this to true if the property use the SharedConfiguration!<br/>
    /// </summary>
    public bool IsShared = false;
}

/// <summary>
/// Define the possible animation directions for an entity feature.<br/>
/// The engine will search for example an animation named "walk_up" for the 'up' direction and the 'walk' animation name.<br/>
/// Just for info, if the user want to use "8" directions, then you NEED to put "EightDirections".
/// If the user want to use "4" directions, then you can either put "FourDirections" or "EightDirections".<br/>
/// If you put "SingleDirection", then the engine will just use the base animation name without any direction suffix, and the user can't change the direction of the animation inside the anim editor.<br/>
/// This is useful for entities that don't need directional animations, like effects.
/// </summary>
public enum AnimationDirection
{
    
    /// <summary>
    /// Define a single direction for the animation.<br/>
    /// For info, inside the engine, this will be treated as "up" direction.
    /// </summary>
    SingleDirection,
    
    /// <summary>
    /// Define four possible directions for the animation.<br/>
    /// Up, Down, Left, Right.
    /// </summary>
    FourDirections,
    
    /// <summary>
    /// Define eight possible directions for the animation.<br/>
    /// Up, Down, Left, Right, Up-Left, Up-Right, Down-Left, Down-Right.
    /// </summary>
    EightDirections,
}

/// <summary>
/// Define that an entity feature requires a specific animation to function properly.<br/>
/// The engine will ensure that the required animation is present for the entity using this feature.
/// </summary>
/// <param name="animationUrn">The URN of the required animation.</param>
/// <param name="displayName">The name of the required animation.</param>
/// <param name="animationDirection">The direction type of the required animation.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class EntityAnimationRequiredAttribute(string animationUrn, string displayName, AnimationDirection animationDirection) : Attribute
{
    public URN AnimationUrn { get; } = new URN(animationUrn);
    public string DisplayName { get; } = displayName;
    public AnimationDirection AnimationDirection { get; } = animationDirection;
}
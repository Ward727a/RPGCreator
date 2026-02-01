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
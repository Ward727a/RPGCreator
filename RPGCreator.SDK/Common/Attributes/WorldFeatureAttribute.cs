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

namespace RPGCreator.SDK.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class WorldFeatureAttribute() : Attribute
{
    public int MaxInstancesPerWorld = 1;
    
    /// <summary>
    /// Defines the URN of an existing feature that this feature replaces when added to a world.<br/>
    /// Default is null, meaning it does not replace any existing feature.<br/>
    /// This is particularly useful when some parts of the engine expect a specific feature to be present on a world.
    /// </summary>
    public string? ReplacingFeatureUrn { get; set; } = null;
}

/// <summary>
/// Attribute to mark a property inside a World Feature as editable in the editor.<br/>
/// This allows game designers to customize the behavior of the feature without modifying the code.<br/>
/// You can specify a name and description for the property to provide context in the editor.
/// </summary>
/// <param name="name">The display name of the property in the editor.</param>
/// <param name="description">The description of the property in the editor (default = "").</param>
[AttributeUsage(AttributeTargets.Property)]
public  sealed class WorldFeaturePropertyAttribute(string name, string description = "") : Attribute
{
    /// <summary>
    /// The display name of the property in the editor.
    /// </summary>
    public string Name { get; } = name;
    
    /// <summary>
    /// The description of the property in the editor.
    /// </summary>
    public string Description { get; } = description;
    
    /// <summary>
    /// The minimum value for numeric properties.<br/>
    /// Set to null to have no minimum.
    /// </summary>
    public object? MinValue { get; set; }
    
    /// <summary>
    /// The maximum value for numeric properties.<br/>
    /// Set to null to have no maximum.
    /// </summary>
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
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

[AttributeUsage(AttributeTargets.Field)]
public class EPropertyAttribute : Attribute
{
    /// <summary>
    /// (optional - default: Field Name)<br/>
    /// Define the display name in the editor UI.
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// (optional)<br/>
    /// A tooltip or description shown in the editor.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// (optional - default: true)<br/>
    /// If true, modifying this property will trigger the IsDirty flag of the parent EClass.
    /// <remarks>
    /// This is only relevant if the EClass has the SupportDirtyFlag flag set to true.
    /// </remarks>
    /// </summary>
    public bool Dirtying { get; set; } = true;

    /// <summary>
    /// (optional - default: true)<br/>
    /// If true, this property will be included in the automatic serialization process.
    /// <remarks>
    /// This is only relevant if the EClass has the SupportSerialization flag set to true.
    /// </remarks>
    /// </summary>
    public bool Serializable { get; set; } = true;

    /// <summary>
    /// (optional - default: empty)<br/>
    /// Define a category to group properties in the editor UI (e.g., "Stats", "Inventory").
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// (optional)<br/>
    /// Specify a custom editor type or hint for the UI (e.g., "Slider", "ColorPicker").
    /// </summary>
    public string EditorHint { get; set; } = "";
}
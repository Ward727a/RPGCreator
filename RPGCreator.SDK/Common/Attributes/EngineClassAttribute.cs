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

namespace RPGCreator.SDK.Common.Attributes;

/// <summary>
/// Attribute to mark a class as an engine class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class EngineClassAttribute : Attribute
{
    /// <summary>
    /// Unique URN of the class.
    /// </summary>
    public string Urn;
    public Type ParentType;
    public string? DisplayName;
    public string? Description;
    public string? Icon;
    public string? SerializationFolder;
    public string[]? Tags;

    public EngineClassAttribute(params string[] urnParts)
    {
        Urn = $"{urnParts[0]}://{string.Join('/', urnParts.AsSpan(1))}";
    }
}
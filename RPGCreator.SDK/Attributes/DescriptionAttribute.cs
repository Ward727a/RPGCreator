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

using System.Reflection;

namespace RPGCreator.SDK.Attributes;

/// <summary>
/// Specifies a description for a property or event.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class DescriptionAttribute(string name, string description = "") : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
}

public static class EnumExtensions
{
    public static DescriptionAttribute GetDescription(this Enum value)
    {
        Type type = value.GetType();
        string? name = Enum.GetName(type, value);
        if (name == null) return new DescriptionAttribute(value.ToString());

        FieldInfo? field = type.GetField(name);
        if (field == null) return new DescriptionAttribute(name);

        var attr = field.GetCustomAttribute<DescriptionAttribute>();
        return attr ?? new DescriptionAttribute(name);
    }
}
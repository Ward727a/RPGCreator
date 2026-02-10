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

using RPGCreator.SDK.Modules.Definition;

namespace RPGCreator.SDK.Modules.SimpleEvents;

/// <summary>
/// A simple descriptor for a property of a simple event.
/// </summary>
/// <param name="Order">The order of the property, which will be used to sort the properties in the editor. The lower the order, the higher the property will be displayed in the editor.</param>
/// <param name="Key">The key where the property will be stored.</param>
/// <param name="PropertyType">The property type awaited for this property.</param>
/// <param name="DisplayName">The display name of the property, which will be shown in the editor.</param>
/// <param name="Description">The description of the property, which will be shown in the editor as a tooltip.</param>
/// <param name="DefaultValue">The default value of the property, which will be used if the user doesn't set any value for this property.</param>
public record SimpleEventPropertyDescriptor(
    int Order,
    string Key,
    Type PropertyType,
    string DisplayName,
    string Description,
    Func<CustomData, Task<object?>> GetInputControl,
    object? DefaultValue = null);

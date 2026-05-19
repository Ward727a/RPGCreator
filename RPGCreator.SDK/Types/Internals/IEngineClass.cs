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

using System.Text.Json.Serialization;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Types.Internals;

/// <summary>
/// Marker interface for engine classes.<br/>
/// Automatically added to all classes that are using [EngineClass] attribute.
/// </summary>
/// <remarks>
/// You should normally not need to implement this interface yourself.
/// </remarks>
public interface IEngineClass
{
    public Ulid Unique { get; internal set; }
    [JsonInclude]
    public URN ClassUrn { get; internal set; }
}

public static class IEngineClassExtensions
{
    public static URN GetClassUrn(this IEngineClass @class)
    {
        var type = @class.GetType();
        if (ClassesRegistry.HasType(type))
        {
            return ClassesRegistry.GetUrn(type).Value;
        }
        return URN.Empty;
    }
}
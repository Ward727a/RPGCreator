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
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Registry;

public interface IUrnRegistry : IService
{
    bool RegisterUrn(ref URN urn, UrnCollisionStrategy collisionStrategy = UrnCollisionStrategy.RenameWithUlidSuffix);
    bool UnregisterUrn(URN urn);
    bool IsUrnRegistered(URN urn);
}

public enum UrnCollisionStrategy
{
    /// <summary>
    /// Return false and do not register the new URN if it collides with an existing one.
    /// </summary>
    Fail,
    /// <summary>
    /// Rename the new URN by appending a ULID suffix if it collides with an existing one, and register it.
    /// </summary>
    RenameWithUlidSuffix,
    /// <summary>
    /// Ignore the collision and don't register the new URN if it collides with an existing one, but return true as if it was registered successfully.
    /// </summary>
    Ignore
}
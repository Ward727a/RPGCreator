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

using RPGCreator.SDK.Attributes;

namespace RPGCreator.SDK.Types.EngineClass;

/// <summary>
/// A base class for all engine classes.<br/>
/// Note: If possible, use <see cref="EClassAttribute"/> instead of implementing this class.<br/>
/// The <see cref="EClassAttribute"/> automatically implements this class for you.
/// </summary>
public abstract class EBaseClass
{
    /// <summary>
    /// The unique identifier of the class.
    /// </summary>
    public virtual Ulid Id { get; protected set; }
    
    /// <summary>
    /// The Urn of the class.<br/>
    /// Urn stands for Universal Resource Name.
    /// </summary>
    public virtual URN Urn { get; protected set; }

    public abstract URN GetClassUrn();
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is EBaseClass otherClass)
        {
            return Id == otherClass.Id;
        }

        return false;
    }
}
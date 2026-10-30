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

namespace RPGCreator.SDK.Assets.AutoTiles;

public readonly record struct MaterialId
{
    public readonly byte Value;

    public MaterialId(byte value)
    {
        if (value >= 128)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "MaterialId must be between 0 and 127.");
        }

        Value = value;
    }
    
    public static implicit operator byte(MaterialId materialId) => materialId.Value;
    
    public static implicit operator MaterialId(byte value) => new(value);
    
    public override string ToString() => Value.ToString();
    
    public static MaterialId Empty => new(0);
}
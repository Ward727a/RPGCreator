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

namespace RPGCreator.SDK.Assets.AutoTiles;

/// <summary>
/// Represents a mask used to store a combination of materials in a highly efficient bitwise format.<br/>
/// It's mainly used for AutoTiles ruleset.
/// </summary>
[method: JsonConstructor]
public readonly struct MaterialMask(UInt128 mask)
{
    [JsonInclude]
    public readonly UInt128 Mask = mask;
    
    public static MaterialMask Empty => new(0);

    public MaterialMask AddMaterial(MaterialId materialId)
    {
        return new MaterialMask(Mask | ((UInt128)1 << materialId.Value));
    }
    
    public MaterialMask RemoveMaterial(MaterialId materialId)
    {
        return new MaterialMask(Mask & ~((UInt128)1 << materialId.Value));
    }
    
    public IEnumerable<MaterialId> GetMaterials()
    {
        UInt128 tempMask = Mask;
        
        while (tempMask != 0)
        {
            int bitIndex = (int)UInt128.TrailingZeroCount(tempMask);
            
            yield return new MaterialId((byte)bitIndex);
            
            tempMask &= ~(UInt128.One << bitIndex); 
        }
    }
    
    public bool Contains(MaterialId materialId) => (Mask & ((UInt128)1 << materialId.Value)) != 0;

    public override string ToString() => Mask.ToString();
}
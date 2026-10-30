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

namespace RPGCreator.Shared.Types;

public record struct TileIndex
{
    public ushort Index { get; private init; }
    
    public int TilesByRows { get; init; }

    public int X => Index % TilesByRows;
    public int Y => Index / TilesByRows;
    
    public static TileIndex Make(int x, int y, int tilesByRows)
    {
        return new TileIndex() { Index = (ushort)(x + y * tilesByRows), TilesByRows = tilesByRows };
    }
    
    public static TileIndex Make(int index) => new() { Index = (ushort)index, TilesByRows = -1 };
    
    public static TileIndex Zero = new() { Index = 0, TilesByRows = -2 };
    
    public static implicit operator ushort(TileIndex index) => index.Index;
}
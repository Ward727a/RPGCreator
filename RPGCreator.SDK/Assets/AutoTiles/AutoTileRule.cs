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

using RPGCreator.SDK.Services.Map;

namespace RPGCreator.SDK.Assets.AutoTiles;

public readonly record struct AutoTileRule(
    CellCondition N,
    CellCondition W,
    CellCondition E,
    CellCondition S,
    CellCondition Nw,
    CellCondition Ne,
    CellCondition Sw,
    CellCondition Se,
    TileData Result
)
{
    public AutoTileRule(CellCondition N, CellCondition W, CellCondition E, CellCondition S, TileData Result) : this(N,
        W, E, S, default, default, default, default, Result)
    {
    }
    
    public bool Evaluate(
        MaterialId n, MaterialId e, MaterialId s, 
        MaterialId w, MaterialId ne, MaterialId nw, 
        MaterialId se, MaterialId sw)
    {
        return N.IsMet(n) && E.IsMet(e) && S.IsMet(s) && W.IsMet(w) &&
               Ne.IsMet(ne) && Nw.IsMet(nw) && Se.IsMet(se) && Sw.IsMet(sw);
    }

    public CellCondition Get(int i)
    {
        return i switch
        {
            0 => Nw,
            1 => N,
            2 => Ne,
            3 => W,
            4 => CellCondition.Invalid,
            5 => E,
            6 => Sw,
            7 => S,
            8 => Se,
            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
        };
    }

    public AutoTileRule Set(int i, CellCondition condition)
    {
        return i switch
        {
            0=> this with {Nw = condition},
            1=> this with {N = condition},
            2=> this with {Ne = condition},
            3=> this with {W = condition},
            5=> this with {E = condition},
            6=> this with {Sw = condition},
            7=> this with {S = condition},
            8=> this with {Se = condition},
            _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
        };
    }
    
    public static AutoTileRule Invalid => new(CellCondition.Invalid, CellCondition.Invalid, CellCondition.Invalid, CellCondition.Invalid, default);

    public CellCondition this[int i] => Get(i);

    public AutoTileRule SetResult(TileData tileData)
    {
        return this with {Result = tileData};
    }

    public AutoTileRule WithResultTileId(ushort tileId)
    {
        return this with { Result = Result.SetTile(tileId) };
    }

    public AutoTileRule WithResultTilesetId(byte tilesetId)
    {
        return this with { Result = Result.SetTileset(tilesetId) };
    }

    public AutoTileRule WithResultTileFlip(TileFlipType flip)
    {
        return this with { Result = Result.SetFlip(flip) };
    }
}
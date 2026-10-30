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

using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.AutoTiles;

[TypeDiscriminator("rpgc", "sdk", "definitions", "auto_tiles_ruleset")]
[AssetType("autoTiles", "ratset")]
public class AutoTileRulesetDefinition : IAssetDto
{
    public Ulid AssetId { get; init; }
    public List<Ulid> Dependencies { get; init; } = [];
    public string Name { get; set; } = string.Empty;
    
    [AssetReference]
    public Ulid TargetTileset { get; set; } = Ulid.Empty;
    public TileIndex DefaultTile { get; set; } = TileIndex.Zero;
    
    public List<AutoTileRule> Rules { get; set; } = [];
    
    public TileData? EvaluateNeighbors(byte n, byte e, byte s, byte w, byte ne, byte nw, byte se, byte sw)
    {
        foreach (var rule in Rules)
        {
            if (rule.Evaluate(n, e, s, w, ne, nw, se, sw))
            {
                return rule.Result.SetFlip(TileFlipType.None);
            }

            var allowedFlip = rule.Result.Flip;
            if (allowedFlip == TileFlipType.None)
                continue;

            if ((allowedFlip & TileFlipType.Horizontal) != 0)
            {
                if (rule.Evaluate(n, w, s, e, nw, ne, sw, se))
                {
                    return rule.Result.SetFlip(TileFlipType.Horizontal);
                }
            }

            if ((allowedFlip & TileFlipType.Vertical) != 0)
            {
                if (rule.Evaluate(s, e, n, w, se, sw, ne, nw))
                {
                    return rule.Result.SetFlip(TileFlipType.Vertical);
                }
            }

            if (allowedFlip == TileFlipType.Both)
            {
                if (rule.Evaluate(s, w, n, e, sw, se, nw, ne))
                {
                    return rule.Result.SetFlip(TileFlipType.Both);
                }
            }
        }
        return null;
    }
}
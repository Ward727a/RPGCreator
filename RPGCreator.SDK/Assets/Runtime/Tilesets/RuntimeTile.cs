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

using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Runtime.Tilesets;

public struct RuntimeTile(ITileDef tileDef) : ILayerElem
{
    public readonly Size SizeInTileset = tileDef.SizeInTileset;
    public readonly Vector2 PositionInTileset = tileDef.PositionInTileset;
    public readonly Ulid TilesetId = tileDef.TilesetDef.Unique;
    public readonly TileFlip Flip = tileDef.Flip;
    public Vector2 Offset { get; set; }
    public Vector2 PositionInMap { get; set; }
    public SpriteEffects Effects { get; set; }
    public readonly Rect SourceRect = new Rect(tileDef.PositionInTileset, tileDef.SizeInTileset);
}
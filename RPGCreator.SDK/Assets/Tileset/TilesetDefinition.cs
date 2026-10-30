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
using RPGCreator.Shared.Attributes;

namespace RPGCreator.SDK.Assets.Tileset;

[TypeDiscriminator("rpgc", "sdk", "definitions", "tileset")]
[AssetType("Tilesets", "rtset")]
public class TilesetDefinition : IAssetDto
{
    public Ulid AssetId { get; init;  }
    public List<Ulid> Dependencies { get; init; } = [];
    public string Name { get; set; } = string.Empty;
    public string TextureResourcePath { get; set; } = string.Empty;
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    /// <summary>
    /// Define how many tiles are in a row in the tileset texture.<br/>
    /// E.g., If the tileset texture is 100x100 pixels and each tile is 10x10 pixels, then there are 10 tiles in a row.
    /// </summary>
    public int TilesByRow { get; set; }
}
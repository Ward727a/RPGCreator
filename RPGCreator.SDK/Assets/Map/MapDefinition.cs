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
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using Vector3 = System.Numerics.Vector3;

namespace RPGCreator.SDK.Assets.Map;

[TypeDiscriminator("rpgc", "sdk", "definitions", "map")]
[AssetType("maps", "rmap")]
public class MapDefinition : IAssetDto
{
    [JsonPropertyOrder(-3)]
    public Ulid AssetId { get; init;  }
    [JsonPropertyOrder(-2)]
    public List<Ulid> Dependencies { get; init; } = [];
    public string Name { get; set; } = string.Empty;
    public Point Size { get; set; } = Point.Zero;
    public Vector3 BackgroundColor { get; set; } = Vector3.Zero;
    [JsonPropertyOrder(-1)]
    [AssetContainer]
    public MapDefinitionHeader Header { get; init; } = new();
    [JsonPropertyOrder(999)]
    public List<MapLayerDefinition> Layers { get; init; } = [];
}

public class MapDefinitionHeader
{
    [AssetReference]
    public Dictionary<byte, Ulid> TilesetMappings { get; set; } = new();
    [AssetReference]
    public List<Ulid> PreloadResources { get; set; } = new();
    
    public byte GetNextAvailableTilesetId()
    {
        for (int i = 0; i <= byte.MaxValue; i++)
        {
            byte id = (byte)i;
        
            if (!TilesetMappings.ContainsKey(id))
            {
                return id;
            }
        }
    
        throw new InvalidOperationException("Absolute limit of 256 tilesets per map has been reached.");
    }
    
    [JsonIgnore]
    public IReadOnlyDictionary<Ulid, byte> TilesetMappingsReverse => TilesetMappings.ToDictionary(x => x.Value, x => x.Key);
}

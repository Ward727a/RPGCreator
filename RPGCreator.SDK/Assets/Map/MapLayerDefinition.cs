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

using System.Text.Json;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using Point = Microsoft.Xna.Framework.Point;

namespace RPGCreator.SDK.Assets.Map;

[TypeDiscriminator("rpgc", "sdk", "definitions", "map_layer")]
public class MapLayerDefinition
{
    public string Name { get; set; } = string.Empty;
    public URN DataType { get; set; } = URN.Empty;
    public int ZIndex { get; set; } = 0;
    public Point Size { get; set; }
    /// <summary>
    /// Miscellaneous data that is needed by the current LayerType defined by the <see cref="DataType"/> property.
    /// </summary>
    public JsonElement Data { get; set; }
}
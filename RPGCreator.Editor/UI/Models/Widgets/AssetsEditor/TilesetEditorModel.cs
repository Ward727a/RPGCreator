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

using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;

[EditorUiModel]
public class TilesetEditorModel
{
    public TilesetDefinition? Definition;
    public string TilesetOriginalName => Definition?.Name ?? "ERROR";
    public string TilesetName = "";
    public int[] TilesetTileSize = new int[2];
    public string TilesetImagePath = "";
    public Point TilesetImageSize = Point.Zero;
    
    public ImTextureRef? TilesetTexture;
}
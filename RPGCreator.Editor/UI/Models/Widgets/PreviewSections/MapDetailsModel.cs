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

using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.Shared.Attributes;
using Vector3 = System.Numerics.Vector3;

namespace RPGCreator.Editor.UI.Models.Widgets.PreviewSections;

[EditorUiModel]
public class MapDetailsModel
{

    public void Update(ref MapDefinition mapDefinition)
    {
        mapDefinition.Name = MapName;
        mapDefinition.BackgroundColor = MapBackColor;
    }

    public void Load(in MapDefinition mapDefinition)
    {
        MapName = mapDefinition.Name;
        MapBackColor = mapDefinition.BackgroundColor;
        MapSize = mapDefinition.Size;
    }
    
    public string MapName = string.Empty;
    public Vector3 MapBackColor;
    public Point MapSize;
}
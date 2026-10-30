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

using System;
using System.Collections.Generic;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.Shared.Attributes;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;

[EditorUiModel]
public class AutoTileAssetEditorModel
{
    public AutoTileRulesetDefinition? RulesetDefinition;
    public IReadOnlyList<(string Name, MaterialId MaterialId)> Materials = [];
    public CellConditionMode SelectedToolType = CellConditionMode.Ignore;
    public int SelectedRuleIndex = -1;
    public AutoTileRule SelectedRule
    {
        get
        {
            if(RulesetDefinition == null)
                return AutoTileRule.Invalid;
            
            if(SelectedRuleIndex < 0 || SelectedRuleIndex >= RulesetDefinition.Rules.Count)
                return AutoTileRule.Invalid;
            
            return RulesetDefinition.Rules[SelectedRuleIndex];
        }
    }

    public Ulid AssignedTilesetId { get; set; }
    public uint MaterialsVersion { get; set; }

    public TilesetDefinition? AssignedTilesetDefinition;
    public ImTextureRef? TilesetTexture;
    public Point TilesetTextureSize;
    public Point TilesetTileSize;
    public MaterialId SelectedMaterialId = MaterialId.Empty;
    public MaterialId AssignedMaterialId = MaterialId.Empty;
    public (Vector2 uv0, Vector2 uv1) TileUv;
    public string AssignedMaterialName = "";
    public bool TileFlipV = false;
    public bool TileFlipH = false;
}

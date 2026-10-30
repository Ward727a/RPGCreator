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
using RPGCreator.Editor.UI.Controllers.Widgets;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets;

public class TilesetSelector(IEditorViewContext viewContext, EditorViewDefinition definition, TilesetSelectorController controller) : ComplexEditorUiWindowBase<TilesetSelectorController>(viewContext, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (ImGui.Begin(Name))
        {
            if (ImGui.BeginCombo("Tileset###tileset_selector", "none"))
            {
                foreach (var tilesetId in Controller.GetTilesets())
                {
                    var tileset = Controller.GetTileset(tilesetId);
                    if (tileset is not null)
                    {
                        if (ImGui.Selectable(tileset.Name, Controller.IsSelected(tilesetId)))
                        {
                            Controller.Select(tilesetId);
                        }
                    }
                }
                ImGui.EndCombo();
            }
        }
        ImGui.End();
    }
}
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
using RPGCreator.Editor.UI.Controllers.EditorTools.Payload;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.EditorTools.Payload;

public class TilesetPayload(
    IEditorViewContext context,
    EditorViewDefinition definition,
    TilesetPayloadController controller)
    : ComplexEditorUi<TilesetPayloadController>(context, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        var tilesets = Controller.GetTilesets();
        var availSize = ImGui.GetContentRegionAvail();
        if (ImGui.BeginChild(Name, availSize))
        {
            if (ImGui.BeginCombo("tileset", Controller.GetSelectedTilesetName()))
            {
                foreach (var id in tilesets)
                {
                    ImGui.PushID(id.ToString());
                    var tileset = Controller.GetTileset(id);
                    if (ImGui.Selectable(tileset?.Name ?? "ERROR", id == Controller.GetSelectedTileset()))
                    {
                        Controller.SelectTileset(tileset);
                    }
                    ImGui.PopID();
                }

                ImGui.EndCombo();
            }

            ImGui.BeginDisabled(!Controller.HasSelectedTileset());
            
            var isAlreadyInMap = Controller.IsTilesetAlreadyInMap(Controller.GetSelectedTileset());
            
            if(isAlreadyInMap)
                ImGui.BeginDisabled();
            if (ImGui.Button("Add to map"))
            {
                Controller.AddTilesetInMap(Controller.GetSelectedTileset());
            }
            if(isAlreadyInMap)
                ImGui.EndDisabled();
            else
                ImGui.BeginDisabled();
            if (ImGui.Button("Remove from map"))
            {
                Controller.RemoveTilesetInMap(Controller.GetSelectedTileset());
            }
            if(!isAlreadyInMap)
                ImGui.EndDisabled();
            
            var textureRef = Controller.GetTextureRef(out var success);

            ImGui.BeginDisabled(!isAlreadyInMap);
            if (success)
            {
                DrawTilesetPicker(textureRef);
            }
            else
                ImGui.Text("No texture?");
            
            ImGui.EndDisabled();
            ImGui.EndDisabled();
            ImGui.EndChild();
        }
        
    }
    
    public void DrawTilesetPicker(ImTextureRef textureRef)
    {
        var arg = Controller.GetArgs(textureRef, out var fromCache);
        
        if(!arg.HasValue)
            ImGui.Text("[ERROR] Arg for tile picker is null");
        else
        {
            if (fromCache)
            {
                LocalRouter.GetIfActive("rpgc://views/tileset_tile_picker").OnSuccess(view => view.Draw());
            }
            else
            {
                LocalRouter.ActivateView("rpgc://views/tileset_tile_picker", arg).OnSuccess(view => view.Draw());
            }
        }
    }
}
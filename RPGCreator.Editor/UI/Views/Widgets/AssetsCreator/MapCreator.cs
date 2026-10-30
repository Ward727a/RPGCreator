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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class MapCreator(
    IEditorViewContext viewContext,
    EditorViewDefinition definition,
    MapCreatorController controller) : ComplexEditorUi<MapCreatorController>(viewContext, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        ImGui.Text("Map Creator");
        ImGui.Separator();
        ImGui.InputText("Map Name###map_name", ref Controller.State.MapName, 256);
        
        int[] mapSize = [Controller.State.MapSize.X, Controller.State.MapSize.Y];
        if (ImGui.InputInt2("Map Size (by tile)###map_size", ref mapSize[0]))
        {
            Controller.State.MapSize = new Point(mapSize[0], mapSize[1]);
        }
        ImGui.SetItemTooltip("Enter the size of the map by tile!\nBased on the tile size defined in the project settings.\n- First: Width\n- Second: Height");
        
        ImGui.ColorEdit3("Map Background Color###map_color", ref Controller.State.MapBackgroundColor);
        ImGui.SetItemTooltip("This color will be used as the background color of the map where no tile is placed.");

        var isValid = Controller.IsValid();
        if (!isValid)
        {
            ImGui.BeginDisabled();
        }
        
        var size = ImGui.GetContentRegionAvail();
        if (ImGui.Button("Create Map", size with { Y = 0f }))
        {
            if (Controller.Create())
            {
                ImGui.CloseCurrentPopup();
            }
        }
        ImGui.SetItemTooltip("Don't worry, you can still change every setting after creating the map ;)");
        
        if (!isValid)
        {
            ImGui.EndDisabled();
            ImGui.SetItemTooltip("Huho, some parameters are invalid :/\nCheck if the name is not empty, and if the size is greater than zero!");
        }
    }
}
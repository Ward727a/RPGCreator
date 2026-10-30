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
using Fluxor;
using Hexa.NET.ImGui;
using RPGCreator.Editor.UI.Controllers.Widgets.PreviewSections;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Scenes;

namespace RPGCreator.Editor.UI.Views.Widgets.PreviewSections;

public class MapDetails(IEditorViewContext context, EditorViewDefinition definition, MapDetailsController controller) : ComplexEditorUi<MapDetailsController>(context, definition, controller)
{
    
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        var hasMapLoaded = Controller.GetMapDefinition() != null;
        if (!hasMapLoaded)
        {
            ImGui.SeparatorText("No map selected");
            ImGui.BeginDisabled();
        }
        
        DrawMapDetails();
        
        if(!hasMapLoaded)
        {
            ImGui.EndDisabled();
        }
    }

    private void DrawMapDetails()
    {
        if (ImGui.InputText("Name", ref controller.State.MapName, 256))
        {
            Controller.SetName();
        }
        
        if(ImGui.ColorEdit3("Background Color", ref controller.State.MapBackColor))
        {
            Controller.SetBackgroundColor();
        }
        
        ImGui.BeginDisabled();
        int[] mapSize = [controller.State.MapSize.X, controller.State.MapSize.Y];
        ImGui.InputInt2("Size", ref mapSize[0]);
        ImGui.EndDisabled();
    }
}
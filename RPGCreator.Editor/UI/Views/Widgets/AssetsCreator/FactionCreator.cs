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
using Hexa.NET.ImGui;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class FactionCreator(
    IEditorViewContext context,
    EditorViewDefinition definition,
    FactionCreatorController controller)
    : ComplexEditorUi<FactionCreatorController>(context, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        ImGui.InputText("Name", ref Controller.Model.Name, 256);
        if (ImGui.InputInt("Default Relation", ref Controller.Model.DefaultRelation))
        {
            Controller.Model.DefaultRelation = Math.Clamp(Controller.Model.DefaultRelation, FactionDefinition.HOSTILE_MAX, FactionDefinition.FRIENDLY_MAX);
        }
        ImGui.SetItemTooltip("The default relation value of this faction to other factions. For example, if this faction is hostile to all other faction, the default relation value would be -100.\nMinimum: -100\nMaximum: 100");

        if (ImGui.Button("Create"))
        {
            Controller.Create();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            Controller.Cancel();
        }
    }
}
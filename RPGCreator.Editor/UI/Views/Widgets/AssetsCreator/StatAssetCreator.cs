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
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class StatAssetCreator(
    IEditorViewContext context,
    EditorViewDefinition definition,
    StatAssetCreatorController controller)
    : ComplexEditorUi<StatAssetCreatorController>(context, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        Controller.TryDrawResourceSelector();
        

        if (Controller.Model.ValidationState.HasError("Name"))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
        }
        ImGui.InputText("Name", ref controller.Model.Name, 256);
        if (Controller.Model.ValidationState.HasError("Name"))
        {
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
        ImGui.SetItemTooltip("Stat name (max 255 characters).\ne.g: Strength, Health, Mana");

        if (Controller.Model.ValidationState.HasError("Code"))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
        }
        ImGui.InputText("Code", ref controller.Model.Code, 33);
        if (Controller.Model.ValidationState.HasError("Code"))
        {
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
        
        ImGui.SetItemTooltip("Stat code (max 32 characters).\ne.g: STR, HP, MP");

        if (ImGui.BeginCombo("Kind", controller.Model.KindStr))
        {
            foreach (var kindName in Enum.GetNames<StatKind>())
            {
                if (ImGui.Selectable(kindName, controller.Model.KindStr == kindName))
                {
                    controller.Model.Kind = Enum.Parse<StatKind>(kindName);
                }
                
                ImGui.SetItemTooltip(controller.Model.KindTooltip.GetValueOrDefault(kindName, $"[ERROR] No description for {kindName}!"));
            }
            
            ImGui.EndCombo();
        }
        ImGui.SetItemTooltip("Stat kind define how the stat works.\nBe warned that it's NOT easy to change it after creation!");
        
        if (Controller.Model.ValidationState.HasWarning("Icon"))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.85f, 0.72f, 0.17f,1));
        }
        if (ImGui.Button("Select Icon", new Vector2(0, 32)))
        {
            Controller.OpenIconSelector(LocalRouter);
        }
        
        if (Controller.Model.ValidationState.HasWarning("Icon"))
        {
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }

        ImGui.SetItemTooltip("Select an icon for the stat.\nGood idea to use a 32x32 texture, or any power of 2.");
        if (Controller.Model.IconTexture != null)
        {
            ImGui.SameLine();
            ImGui.Image(Controller.Model.IconTexture.Value, new Vector2(32, 32));
            if(ImGui.BeginItemTooltip())
            {
                ImGui.Text("Max preview size: 256x256. \nIf your icon is bigger, it is scaled down.");
                ImGui.Image(Controller.Model.IconTexture.Value, new Vector2(Controller.Model.IconSize.X, Controller.Model.IconSize.Y));
                ImGui.EndTooltip();
            }
        }
        
        ImGui.Separator();
        if (ImGui.Button("Create"))
        {
            Controller.TryCreateStatAsset();
        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            ImModal.CloseCurrent();
        }
    }
}
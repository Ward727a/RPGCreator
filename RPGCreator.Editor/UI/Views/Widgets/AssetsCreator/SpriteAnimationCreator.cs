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
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class SpriteAnimationCreator(
    IEditorViewContext context,
    EditorViewDefinition definition,
    SpriteAnimationCreatorController controller)
    : ComplexEditorUi<SpriteAnimationCreatorController>(context, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Modal)
            return;
        
        ImGui.InputText("Name", ref Controller.Model.Name, 256);
        if (ImGui.Button("Select Image", new Vector2(0, 32)))
        {
            Controller.OpenSpriteSheetSelector(LocalRouter);
        }
        Controller.TryDrawResourceSelector();
        ImGui.SameLine();
        if (!Controller.Model.Texture.HasValue)
        {
            ImGui.ColorButton("##NoImg", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop, new Vector2(32));
        }
        else
        {
            ImGui.Image(Controller.Model.Texture.Value, new Vector2(32));
        }
        
        ImGui.PushFont(null, 14f);
        ImGui.TextDisabled("More settings can be configured in the editor once the animation is created.");
        ImGui.PopFont();
        
        ImGui.Separator();

        if (ImGui.Button("Create"))
        {
            Controller.Create();
            Controller.Clear();
            ImModal.CloseCurrent();
        }
        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            Controller.Clear();
            ImModal.CloseCurrent();
        }
    }
}
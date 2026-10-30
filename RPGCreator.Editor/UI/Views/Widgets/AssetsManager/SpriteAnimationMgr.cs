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
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;
using RPGCreator.SDK.Assets.SpriteAnimation;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsManager;

public class SpriteAnimationMgr(
    IEditorViewContext context,
    EditorViewDefinition definition,
    SpriteAnimationMgrController controller)
    : ComplexEditorUi<SpriteAnimationMgrController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        Controller.LoadAnimation();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        if (ImGui.Button("New"))
        {
            LocalRouter.ActivateView("rpgc://views/asset_creator/sprite_animation_creator").OnSuccess(view =>
            {
                ImModal.Show(view.Name, ()=>view.Draw(EDrawAs.Modal), true, ()=>Controller.LoadAnimation());
            }).OnFailure(err =>
            {
                Logger.Error(err);
            });
        }
        ImGui.SameLine();
        ImGui.BeginDisabled(Controller.Model.SelectedAnimation == null);
        if (ImGui.Button("Edit"))
        {
            GlobalRouter.ActivateView("rpgc://views/asset_editor/sprite_animation_editor", Controller.Model.SelectedAnimation)
                .OnSuccess(view =>
                {
                    if (view is IEditorUiWindow window)
                        EditorWindowManager.Open(window, MainEditorUiWindow.WindowGroup);
                });
        }
        ImGui.SameLine();
        if (ImGui.Button("Delete", ImGuiExt.ButtonType.Error))
        {
            ImModal.Show("Are you sure?", () =>
            {
                ImGui.Text("Are you sure that you want to delete this animation?");
                ImGui.TextDisabled("Animation:");
                ImGui.SameLine(0f);
                ImGui.Text(Controller.Model.SelectedAnimation!.Name);
                ImGui.Separator();
                if (ImGui.Button("Yes, delete it.", ImGuiExt.ButtonType.Error))
                {
                    Controller.DeleteAnimation(Controller.Model.SelectedAnimation!);
                    ImModal.CloseCurrent();
                }
                ImGui.SetItemTooltip("This action can not be undone!");
                ImGui.SameLine();
                if (ImGui.Button("No, do not delete it."))
                {
                    ImModal.CloseCurrent();
                }
            });
        }
        ImGui.EndDisabled();
        
        ImGui.Separator();

        if (Controller.Model.Animations.Count <= 0)
        {
            ImGui.TextDisabled("No animations found. Create a new one for it to appear here.");
        } else
        {
            if (!ImGui.BeginTable("##SpriteAnimationList", 2, ImGuiTableFlags.BordersInner)) return;
            ImGui.TableSetupColumn("Image", ImGuiTableColumnFlags.WidthFixed, 32f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                
            ImGui.TableHeadersRow();
                
            var clipper = ImGui.ImGuiListClipper();

            clipper.Begin(Controller.Model.Animations.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var animation = Controller.Model.Animations[i];
                    ImGui.PushID(i);

                    DrawAnimationItem(animation);

                    ImGui.PopID();
                }
            }

            ImGui.EndTable();
        }
    }

    private void DrawAnimationItem(SpriteAnimationDefinition animationDefinition)
    {
        ImGui.TableNextColumn();

        var pos = ImGui.GetCursorPos();
        if (ImGui.Selectable("##" + animationDefinition.AssetId, Controller.Model.SelectedAnimation == animationDefinition, ImGuiSelectableFlags.SpanAllColumns,
                new Vector2(0, 32)))
        {
            Controller.Model.SelectedAnimation = animationDefinition;
        }
        ImGui.SetCursorPos(pos);
        var texture = Controller.GetTexture(animationDefinition);
        if(texture.HasValue)
            ImGui.Image(texture.Value, new Vector2(32, 32));
        else
            ImGui.ColorButton("##NoImg", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop, new Vector2(32));
        ImGui.TableNextColumn();
        ImGui.Text(animationDefinition.Name);
    }
}
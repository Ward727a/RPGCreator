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
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsManager;

public class SkillMgr(
    IEditorViewContext context,
    EditorViewDefinition definition,
    SkillMgrController controller)
    : ComplexEditorUi<SkillMgrController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        Controller.LoadSkills();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
            return;

        Controller.LoadSkills();

        if (ImGui.Button("New"))
        {
            LocalRouter.ActivateView("rpgc://views/asset_creator/skill_creator").OnSuccess(view =>
            {
                ImModal.Show(view.Name, () => view.Draw(EDrawAs.Modal), true, () => Controller.LoadSkills(true));
            }).OnFailure(err => Logger.Error(err));
        }

        ImGui.SameLine();
        ImGui.BeginDisabled(Controller.Model.SelectedSkill == null);

        if (ImGui.Button("Edit"))
        {
            GlobalRouter.ActivateView("rpgc://views/asset_editor/skill_editor", Controller.Model.SelectedSkill)
                .OnSuccess(view =>
                {
                    if (view is IEditorUiWindow window)
                        EditorWindowManager.Open(window, MainEditorUiWindow.WindowGroup);
                })
                .OnFailure(err => Logger.Error(err));
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete", ImGuiExt.ButtonType.Error))
        {
            ImModal.Show("Are you sure?", () =>
            {
                ImGui.Text("Are you sure that you want to delete the selected skill?");
                ImGui.TextDisabled("Selected:");
                ImGui.SameLine(0f);
                ImGui.Text(Controller.Model.SelectedSkill?.Name ?? "[!!ERROR!!]");
                ImGui.Separator();

                if (ImGui.Button("Yes, delete it.", ImGuiExt.ButtonType.Error))
                {
                    Controller.DeleteSkill(Controller.Model.SelectedSkill!);
                    Controller.LoadSkills(true);
                    ImModal.CloseCurrent();
                }
                ImGui.SetItemTooltip("This action cannot be undone!");

                ImGui.SameLine();
                if (ImGui.Button("No"))
                {
                    ImModal.CloseCurrent();
                }
            });
        }

        ImGui.EndDisabled();
        ImGui.Separator();

        if (Controller.Model.Skills.Count <= 0)
        {
            ImGui.TextDisabled("No skills found. Create a new skill to see it appear here.");
            return;
        }

        if (ImGui.BeginTable("##SkillsListTable", 4, ImGuiTableFlags.BordersInner | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 32f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Target", ImGuiTableColumnFlags.WidthFixed, 110f);
            ImGui.TableSetupColumn("Effects", ImGuiTableColumnFlags.WidthFixed, 70f);
            ImGui.TableHeadersRow();

            var clipper = ImGui.ImGuiListClipper();
            clipper.Begin(Controller.Model.Skills.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var skill = Controller.Model.Skills[i];
                    ImGui.PushID(i);
                    DrawSkillRow(skill);
                    ImGui.PopID();
                }
            }

            clipper.End();
            ImGui.EndTable();
        }
    }

    private void DrawSkillRow(SkillDefinition skill)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        var pos = ImGui.GetCursorPos();
        if (ImGui.Selectable("##" + skill.AssetId, Controller.Model.SelectedSkill == skill,
                ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 32)))
        {
            Controller.Model.SelectedSkill = skill;
        }

        ImGui.SetCursorPos(pos);
        var texture = Controller.GetTexture(skill);
        if (texture.HasValue)
        {
            ImGui.Image(texture.Value, new Vector2(32, 32));
        }
        else
        {
            ImGui.ColorButton("##NoImg", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop,
                new Vector2(32, 32));
        }

        ImGui.TableNextColumn();
        ImGui.TextEllipsis(skill.Name);

        ImGui.TableNextColumn();
        ImGui.TextDisabled(skill.TargetType.ToString());

        ImGui.TableNextColumn();
        ImGui.Text($"{skill.Effects.Count}");
    }
}
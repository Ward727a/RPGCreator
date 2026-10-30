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
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.SDK.Modules.Editor.Ui;
using ImGuiExt = RPGCreator.Editor.ImGuiLib.ImGuiExt;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class SkillCreator(
    IEditorViewContext context,
    EditorViewDefinition definition,
    SkillCreatorController controller)
    : ComplexEditorUi<SkillCreatorController>(context, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Modal && drawAs != EDrawAs.Content)
            return;

        ImGui.InputText("Name", ref Controller.Model.Name, 256);
        ImGui.SetItemTooltip("Name of the skill (e.g. Slash, Fireball, Heal).");

        ImGui.InputTextMultiline("Description", ref Controller.Model.Description, 1024, new Vector2(0, 60));

        if (ImGui.Button("Select Icon", new Vector2(0, 32)))
        {
            Controller.OpenIconSelector(LocalRouter);
        }
        Controller.TryDrawResourceSelector();

        ImGui.SameLine();
        if (Controller.Model.IconTexture.HasValue)
        {
            ImGui.Image(Controller.Model.IconTexture.Value, new Vector2(32, 32));
        }
        else
        {
            ImGui.ColorButton("##NoIcon", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop,
                new Vector2(32, 32));
        }

        ImGui.SeparatorText("Starting Template");
        DrawPresetButton("Attack / Damage", SkillPreset.DamageSkill, "Targets a single enemy and adds a default Damage effect.");
        ImGui.SameLine();
        DrawPresetButton("Heal / Restore", SkillPreset.HealSkill, "Targets an ally and adds a default Heal effect.");
        ImGui.SameLine();
        DrawPresetButton("Self Buff", SkillPreset.SelfBuff, "Targets self and adds a Status Effect.");
        ImGui.SameLine();
        DrawPresetButton("Blank", SkillPreset.Blank, "Starts with no effects.");

        if (ImGui.BeginCombo("Target Type", Controller.Model.TargetType.ToString()))
        {
            foreach (var targetType in Enum.GetValues<SkillTargetType>())
            {
                if (ImGui.Selectable(targetType.ToString(), targetType == Controller.Model.TargetType))
                {
                    Controller.Model.TargetType = targetType;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.PushFont(null, 14f);
        ImGui.TextDisabled("Costs, formulas, animations and additional effects can be configured in the Skill Editor.");
        ImGui.PopFont();

        ImGui.Separator();

        if (ImGui.Button("Create", ImGuiExt.ButtonType.Success))
        {
            if (Controller.Create())
            {
                Controller.Clear();
                ImModal.CloseCurrent();
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel"))
        {
            Controller.Clear();
            ImModal.CloseCurrent();
        }
    }

    private void DrawPresetButton(string label, SkillPreset preset, string tooltip)
    {
        bool isSelected = Controller.Model.Preset == preset;
        if (isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.25f, 0.50f, 0.85f, 1.0f));
        }

        if (ImGui.Button(label))
        {
            Controller.ApplyPreset(preset);
        }

        if (isSelected)
        {
            ImGui.PopStyleColor();
        }

        ImGui.SetItemTooltip(tooltip);
    }
}
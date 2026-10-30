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
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.SDK.Modules.Editor.Ui;
using ImGuiExt = RPGCreator.Editor.ImGuiLib.ImGuiExt;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class CharacterCreator(
    IEditorViewContext context,
    EditorViewDefinition definition,
    CharacterCreatorController controller)
    : ComplexEditorUi<CharacterCreatorController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        Controller.InitializeDefaults();
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Modal && drawAs != EDrawAs.Content)
            return;

        ImGui.InputText("Name", ref Controller.Model.Name, 256);
        ImGui.InputTextMultiline("Description", ref Controller.Model.Description, 1024, new Vector2(0, 60));

        if (ImGui.Button("Select Portrait", new Vector2(0, 32)))
        {
            Controller.OpenPortraitSelector(LocalRouter);
        }
        Controller.TryDrawResourceSelector();

        ImGui.SameLine();
        if (Controller.Model.PortraitTexture.HasValue)
        {
            ImGui.Image(Controller.Model.PortraitTexture.Value, new Vector2(32, 32));
        }
        else
        {
            ImGui.ColorButton("##NoPortrait", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop,
                new Vector2(32, 32));
        }

        ImGui.SeparatorText("Starting Template (Preset)");
        ImGui.TextDisabled("Choose a role to pre-select recommended features:");

        DrawPresetButton("Playable Hero", CharacterPreset.PlayableHero, "Adds Stats, Faction, Movement, Collider & Player Controller.");
        ImGui.SameLine();
        DrawPresetButton("Enemy / Monster", CharacterPreset.EnemyMonster, "Adds Stats, Faction, Movement & Collider.");
        ImGui.SameLine();
        DrawPresetButton("Town NPC", CharacterPreset.TownNpc, "Adds Faction, Movement & Collider.");
        ImGui.SameLine();
        DrawPresetButton("Blank", CharacterPreset.Custom, "Starts with no features.");

        if (ImGui.CollapsingHeader("Included Features (Advanced)"))
        {
            foreach (var (urn, _, attr) in Controller.GetRegisteredFeatures())
            {
                bool isSelected = Controller.Model.SelectedFeatureUrns.Contains(urn);
                if (ImGui.Checkbox($"{attr.DisplayName} ({attr.Category})##{urn}", ref isSelected))
                {
                    Controller.ToggleFeature(urn, isSelected);
                }

                if (!string.IsNullOrEmpty(attr.Description))
                {
                    ImGui.SetItemTooltip(attr.Description);
                }
            }
        }

        ImGui.PushFont(null, 14f);
        ImGui.TextDisabled("Stats, animations and feature settings can be configured in the Character Editor after creation.");
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

    private void DrawPresetButton(string label, CharacterPreset preset, string tooltip)
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
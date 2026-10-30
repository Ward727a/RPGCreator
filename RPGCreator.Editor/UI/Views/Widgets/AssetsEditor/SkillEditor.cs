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
using System.Linq;
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using ImGuiExt = RPGCreator.Editor.ImGuiLib.ImGuiExt;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;

public class SkillEditor(
    IEditorViewContext context,
    EditorViewDefinition definition,
    SkillEditorController controller)
    : ComplexEditorUiWindowBase<SkillEditorController>(context, definition, controller)
{
    public override void OnActivate(object? args)
    {
        if (args is not SkillDefinition skill)
        {
            Logger.Once.Error("Invalid skill definition provided to SkillEditor.");
            Close();
            return;
        }

        Controller.Setup(skill);
    }

    protected override void OnClosed()
    {
        Controller.Clear();
    }

    private readonly float _footerHeight = ImGui.CalcButtonSize("Save").Y + ImGui.GetStyle().FramePadding.Y * 3f;
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Window)
            return;

        if (ImGui.Begin(Name, ref IsOpenInternal, Controller.Model.IsDirty ? ImGuiWindowFlags.UnsavedDocument : ImGuiWindowFlags.None))
        {

            if (ImGui.BeginTable("##SkillEditor_MainTable", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable, new Vector2(0, -_footerHeight)))
            {
                ImGui.TableSetupColumn("##LeftGeneralColumn", ImGuiTableColumnFlags.WidthFixed, 340f);
                ImGui.TableSetupColumn("##RightEffectsColumn", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextColumn();
                DrawGeneralSettingsColumn();

                ImGui.TableNextColumn();
                DrawEffectsColumn();

                ImGui.EndTable();
            }

            ImGui.Separator();

            if (ImGui.Button("Save", ImGuiExt.ButtonType.Success))
            {
                Controller.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                Close();
            }
        }
        ImGui.End();
    }

    private void DrawGeneralSettingsColumn()
    {
        var allStats = Controller.GetAllStats();
        var allAnims = Controller.GetAllSpriteAnimations();

        ImGui.SeparatorText("Info");

        if (Controller.Model.IconTexture.HasValue)
        {
            ImGui.Image(Controller.Model.IconTexture.Value, new Vector2(48, 48));
        }
        else
        {
            ImGui.ColorButton("##NoSkillIcon", new Vector4(0.26f, 0.27f, 0.29f, 1f),
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop,
                new Vector2(48, 48));
        }

        ImGui.SameLine();
        if (ImGui.Button("Select Icon", new Vector2(0, 48)))
        {
            Controller.OpenIconSelector(LocalRouter);
        }

        if (ImGui.InputText("Name", ref Controller.Model.Name, 256))
            Controller.Model.IsDirty = true;

        if (ImGui.InputTextMultiline("Description", ref Controller.Model.Description, 1024, new Vector2(-1, 70)))
            Controller.Model.IsDirty = true;

        ImGui.SeparatorText("Cost & Cooldown");

        var currentCostStat = allStats.FirstOrDefault(s => s.AssetId == Controller.Model.CostStatId);
        string costStatPreview = currentCostStat != null ? $"{currentCostStat.Name} ({currentCostStat.Code})" : "None (Free)";

        if (ImGui.BeginCombo("Resource Stat", costStatPreview))
        {
            if (ImGui.Selectable("None (Free)", Controller.Model.CostStatId == Ulid.Empty))
            {
                Controller.Model.CostStatId = Ulid.Empty;
                Controller.Model.IsDirty = true;
            }

            foreach (var stat in allStats.Where(s => s.Kind == StatKind.Resource))
            {
                if (ImGui.Selectable($"{stat.Name} ({stat.Code})##{stat.AssetId}", stat.AssetId == Controller.Model.CostStatId))
                {
                    Controller.Model.CostStatId = stat.AssetId;
                    Controller.Model.IsDirty = true;
                }
            }
            ImGui.EndCombo();
        }

        if (Controller.Model.CostStatId != Ulid.Empty)
        {
            if (ImGui.InputDouble("Cost Amount", ref Controller.Model.CostAmount, 1, 10, "%.0f"))
                Controller.Model.IsDirty = true;
        }

        if (ImGui.DragFloat("Cooldown (s)", ref Controller.Model.Cooldown, 0.1f, 0f, 3600f, "%.1f"))
            Controller.Model.IsDirty = true;

        ImGui.SeparatorText("Targeting");

        if (ImGui.BeginCombo("Target Type", Controller.Model.TargetType.ToString()))
        {
            foreach (var targetType in Enum.GetValues<SkillTargetType>())
            {
                if (ImGui.Selectable(targetType.ToString(), targetType == Controller.Model.TargetType))
                {
                    Controller.Model.TargetType = targetType;
                    Controller.Model.IsDirty = true;
                }
            }
            ImGui.EndCombo();
        }

        if (Controller.Model.TargetType != SkillTargetType.Self)
        {
            if (ImGui.DragFloat("Range (px)", ref Controller.Model.Range, 1f, 0f, 5000f, "%.0f"))
                Controller.Model.IsDirty = true;
        }

        ImGui.SeparatorText("Visuals & Audio");

        ImGui.Text("Caster Anim Slot:");
        ImGui.SetNextItemWidth(80f);
        if (ImGui.InputText("##CasterNs", ref Controller.Model.CasterAnimNamespace, 64))
            Controller.Model.IsDirty = true;
        ImGui.SameLine(0f, 2f);
        ImGui.TextDisabled("://anim_slots/");
        ImGui.SameLine(0f, 2f);
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.InputText("##CasterSlot", ref Controller.Model.CasterAnimSlotName, 64))
            Controller.Model.IsDirty = true;

        var currentHitAnim = allAnims.FirstOrDefault(a => a.AssetId == Controller.Model.HitSpriteAnimationId);
        string hitAnimPreview = currentHitAnim != null ? currentHitAnim.Name : "None";

        if (ImGui.BeginCombo("Hit Animation", hitAnimPreview))
        {
            if (ImGui.Selectable("None", Controller.Model.HitSpriteAnimationId == Ulid.Empty))
            {
                Controller.Model.HitSpriteAnimationId = Ulid.Empty;
                Controller.Model.IsDirty = true;
            }

            foreach (var anim in allAnims)
            {
                if (ImGui.Selectable($"{anim.Name}##{anim.AssetId}", anim.AssetId == Controller.Model.HitSpriteAnimationId))
                {
                    Controller.Model.HitSpriteAnimationId = anim.AssetId;
                    Controller.Model.IsDirty = true;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Text("Hit Sound:");
        ImGui.SameLine();
        ImGui.TextDisabled(Controller.Model.HitSoundDisplayName);
        ImGui.SameLine();
        if (ImGui.SmallButton("Select Audio"))
        {
            Controller.OpenSoundSelector(LocalRouter);
        }
        Controller.TryDrawResourceSelector();
        if (Controller.Model.HitSoundResourceId != Ulid.Empty)
        {
            ImGui.SameLine();
            if (ImGui.SmallButton("Clear##ClearAudio"))
            {
                Controller.Model.HitSoundResourceId = Ulid.Empty;
                Controller.Model.HitSoundDisplayName = "None";
                Controller.Model.IsDirty = true;
            }
        }
    }

    private void DrawEffectsColumn()
    {
        ImGui.Text($"Skill Effects ({Controller.Model.Effects.Count})");
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                            ImGui.CalcButtonSize("+ Add Effect").X - ImGui.GetStyle().ItemSpacing.X);

        if (ImGui.Button("+ Add Effect", ImGuiExt.ButtonType.Success))
        {
            Controller.AddEffect();
        }

        ImGui.Separator();

        if (Controller.Model.Effects.Count == 0)
        {
            ImGui.TextDisabled("No effects configured. Click '+ Add Effect' to make this skill deal damage, heal, or apply buffs.");
            return;
        }

        var allStats = Controller.GetAllStats();
        int? indexToRemove = null;

        ImGui.BeginChild("##SkillEffectsScrollRegion", new Vector2(0, -_footerHeight));

        for (int i = 0; i < Controller.Model.Effects.Count; i++)
        {
            var effect = Controller.Model.Effects[i];
            ImGui.PushID(i);

            bool open = ImGui.CollapsingHeader($"Effect #{i + 1}: {effect.Kind}###SkillEffect_{i}", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap);

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                                ImGui.CalcButtonSize("Remove").X - ImGui.GetStyle().ItemSpacing.X);
            if (ImGui.SmallButton("Remove"))
            {
                indexToRemove = i;
            }

            if (open)
            {
                ImGui.Indent();

                if (ImGui.BeginCombo("Effect Type", effect.Kind.ToString()))
                {
                    foreach (var kind in Enum.GetValues<SkillEffectKind>())
                    {
                        if (ImGui.Selectable(kind.ToString(), kind == effect.Kind))
                        {
                            effect.Kind = kind;
                            Controller.Model.IsDirty = true;
                        }
                    }
                    ImGui.EndCombo();
                }

                float chancePct = effect.Chance * 100f;
                if (ImGui.SliderFloat("Chance (%)", ref chancePct, 1f, 100f, "%.0f%%"))
                {
                    effect.Chance = Math.Clamp(chancePct / 100f, 0.01f, 1f);
                    Controller.Model.IsDirty = true;
                }

                if (effect.Kind is SkillEffectKind.DamageStat or SkillEffectKind.HealStat)
                {
                    var targetStat = allStats.FirstOrDefault(s => s.AssetId == effect.TargetStatId);
                    string statLabel = targetStat != null ? $"{targetStat.Name} ({targetStat.Code})" : "Select Target Stat...";

                    if (ImGui.BeginCombo("Target Stat", statLabel))
                    {
                        foreach (var stat in allStats)
                        {
                            if (ImGui.Selectable($"{stat.Name} ({stat.Code})##{stat.AssetId}", stat.AssetId == effect.TargetStatId))
                            {
                                effect.TargetStatId = stat.AssetId;
                                Controller.Model.IsDirty = true;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    string formula = effect.Formula;
                    if (ImGui.InputText("Pratt Formula", ref formula, 512))
                    {
                        effect.Formula = formula;
                        Controller.Model.IsDirty = true;
                    }
                    ImGui.SetItemTooltip("Examples: '25', 'STR * 2 - DEF', 'INT * 150%'");

                    if (Controller.IsFormulaValid(effect.Formula))
                    {
                        ImGui.TextColored(ImGuiExt.Colors.Success, "Valid Formula");
                    }
                    else
                    {
                        ImGui.TextColored(ImGuiExt.Colors.Error, "Invalid Formula Syntax");
                    }

                    float variancePct = effect.Variance * 100f;
                    if (ImGui.SliderFloat("Variance (+/- %)", ref variancePct, 0f, 100f, "%.0f%%"))
                    {
                        effect.Variance = Math.Clamp(variancePct / 100f, 0f, 1f);
                        Controller.Model.IsDirty = true;
                    }
                    ImGui.SetItemTooltip("Allow the formula to vary by +/- the specified percentage.\ne.g. Formula = '20', Variance = +/- 50%\nthen final value will be 10 to 30 selected randomly");
                }
                else if (effect.Kind is SkillEffectKind.ApplyStatusEffect or SkillEffectKind.RemoveStatusEffect)
                {
                    ImGui.TextDisabled("Status Effect Selection [WIP]");
                }
                else if (effect.Kind == SkillEffectKind.EmitSignal)
                {
                    ImGui.TextDisabled("Signal Selection [WIP]");
                }

                ImGui.Unindent();
                ImGui.Spacing();
            }

            ImGui.PopID();
        }

        if (indexToRemove.HasValue)
        {
            Controller.RemoveEffectAt(indexToRemove.Value);
        }

        ImGui.EndChild();
    }
}
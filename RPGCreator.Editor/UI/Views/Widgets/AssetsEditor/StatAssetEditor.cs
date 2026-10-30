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
using System.Globalization;
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;


public class StatAssetEditor(
    IEditorViewContext context,
    EditorViewDefinition definition,
    StatAssetEditorController controller)
    : ComplexEditorUiWindowBase<StatAssetEditorController>(context, definition, controller)
{
    private static readonly ScopedLogger LOGGER = Logger.ForContext<StatAssetEditor>();
    
    public override void OnActivate(object? args)
    {
        if (args is StatDefinition stat)
        {
            Controller.LoadStat(stat);
        }
        else
        {
            LOGGER.Error("Invalid stat definition provided.");
            GlobalRouter.DropView(ViewId);
        }
    }

    protected override void OnClosed()
    {
        if (Controller.Model.IsDirty)
        {
            ImModal.Show("Are you sure?", () =>
            {
                ImGui.Text("If you close without saving, your changes will be lost.");
                ImGui.Text("Continue?");
                if (ImGui.Button("Yes, close without saving"))
                {
                    ImModal.CloseCurrent();
                    Controller.Clear();
                }
                ImGui.SameLine();
                if (ImGui.Button("No, keep editing"))
                {
                    ImModal.CloseCurrent();
                    // Later on, we should see to add a 'CloseRequested' abstract method to intercept closing event
                    EditorWindowManager.Open(this, MainEditorUiWindow.WindowGroup);
                }
            });
        }
        else
        {
            Controller.Clear();
        }
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Window)
            return;

        if (ImGui.Begin(Name, ref IsOpenInternal, Controller.Model.IsDirty ? ImGuiWindowFlags.UnsavedDocument : ImGuiWindowFlags.None))
        {
            
            if (ImGui.BeginTable("##StatAssetEditor_Table", 2, ImGuiTableFlags.BordersInnerV,
                    new Vector2(0, -(ImGui.CalcButtonSize("Save").Y + ImGui.GetStyle().FramePadding.Y * 3))))
            {
                ImGui.TableSetupColumn("##info");
                ImGui.TableSetupColumn("##thresholdRules", ImGuiTableColumnFlags.WidthFixed, 600f);

                ImGui.TableNextColumn();
                ImGui.TextDisabled("Kind: ");
                ImGui.SameLine(0f);
                ImGui.Text(Controller.Model.Kind.ToString());
                ImGui.Separator();
                if(ImGui.InputText("Name", ref Controller.Model.Name, 256))
                {
                    Controller.Model.IsDirty = true;
                }
                if(ImGui.InputText("Code", ref Controller.Model.Code, 256))
                {
                    Controller.Model.IsDirty = true;
                }
                ImGui.InputTextMultiline("Description", ref Controller.Model.Description, 2048);
                if(ImGui.Button("Select Icon", new Vector2(0, 32)))
                {
                    Controller.OpenIconSelector(LocalRouter);
                }
            
                Controller.TryDrawResourceSelector();
                ImGui.SameLine();
                var iconTexture = Controller.Model.IconTexture;
                if (iconTexture.HasValue)
                {
                    ImGui.Image(iconTexture.Value, new Vector2(32, 32));
                    if (ImGui.BeginItemTooltip())
                    {
                        
                        ImGui.Image(iconTexture.Value, new Vector2(Controller.Model.IconSize.X, Controller.Model.IconSize.Y));
                        
                        ImGui.EndTooltip();
                    }
                }
                else
                {
                    ImGui.TextDisabled("No Icon");
                }

                ImGui.SeparatorText("Settings");
                ImGui.SetItemTooltip("Stat settings, input will change based on the kind of stat.");

                if (Controller.Model.Kind == StatKind.Derived)
                {
                    if(ImGui.InputText("Formula", ref Controller.Model.DerivedStatFormula, 1024))
                    {
                        Controller.Model.IsDirty = true;
                    }
                }
                else
                {
                    if(ImGui.InputDouble("Default Value", ref Controller.Model.DefaultValue, format: "%.0f"))
                    {
                        Controller.Model.IsDirty = true;
                    }
                }

                ImGui.InputDouble("Minimum Value", ref Controller.Model.MinValue, format: "%.0f");
                ImGui.BeginDisabled(Controller.Model.MaxStatId != Ulid.Empty);
                ImGui.InputDouble("Maximum Value", ref Controller.Model.MaxValue, format: "%.0f");
                ImGui.EndDisabled();
                if (Controller.Model.Kind != StatKind.Derived)
                {
                    ImGui.Text("Max Value by Stat:");
                    ImGui.SameLine();
                    if (Controller.Model.MaxStatId == Ulid.Empty)
                        ImGui.TextDisabled("None");
                    else
                        ImGui.Text(Controller.Model.MaxStatId.ToString());
                    ImGui.SameLine(0f);
                    if (ImGui.Button("Select Stat"))
                    {
                        
                        var stats = Controller.GetStats();
                        
                        ImModal.Show("Select Max Stat", () =>
                        {
                            ImGui.Text("Select a stat to set as the maximum value.");
                            ImGui.PushFont(null, 14f);
                            ImGui.TextDisabled("Certain stat can't be used as a maximum value due to risk of circular dependency.");
                            ImGui.PopFont();
                            ImGui.SetItemTooltip("Circular dependency is a case where, for example: Stat A depends on Stat B, and Stat B depends on Stat A.\nSo we have A => B => A => B => [REPEAT INDEFINITELY].");
                            ImGui.Separator();

                            if (ImGui.BeginTable("##MaxStatTable", 3))
                            {
                                ImGui.TableSetupColumn("Name");
                                ImGui.TableSetupColumn("Code");
                                ImGui.TableSetupColumn("Max");
                                ImGui.TableHeadersRow();

                                var clipper = ImGui.ImGuiListClipper();
                                clipper.Begin(stats.Count);

                                while (clipper.Step())
                                {
                                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                                    {
                                        var id = stats[i];
                                        var statResult = Controller.GetStat(id);
                                        if (statResult.IsFailure)
                                            continue;

                                        var stat = statResult.Value;
                                        bool isSelfOrCycle = id == Controller.Model.Stat.AssetId || Controller.WouldCreateMaxStatCycle(Controller.Model.Stat.AssetId, id);

                                        ImGui.TableNextRow();
                                        ImGui.BeginDisabled(isSelfOrCycle);

                                        ImGui.TableNextColumn();
                                        if (ImGui.Selectable($"{stat.Name}##{id}", id == Controller.Model.MaxStatId, ImGuiSelectableFlags.SpanAllColumns))
                                        {
                                            Controller.Model.MaxStatId = id;
                                            Controller.Model.IsDirty = true;
                                        }

                                        ImGui.TableNextColumn();
                                        ImGui.Text(stat.Code);

                                        ImGui.TableNextColumn();
                                        ImGui.Text(stat.MaxStatId == Ulid.Empty
                                            ? stat.MaxValue.ToString("F1", CultureInfo.InvariantCulture)
                                            : stat.MaxStatId.ToString());
                                        ImGui.SetItemTooltip(stat.MaxStatId == Ulid.Empty
                                            ? "This stat depends on a fixed value."
                                            : "This stat depends on another stat.");

                                        ImGui.EndDisabled();
                                    }
                                }

                                clipper.End();
                                ImGui.EndTable();
                            }
                            
                        });
                    }
                    
                    ImGui.SameLine();

                    if (ImGui.Button("Clear"))
                    {
                        Controller.Model.MaxStatId = Ulid.Empty;
                        Controller.Model.IsDirty = true;
                    }
                }

                ImGui.TableNextColumn();
                ImGui.Text("Threshold rules");
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                                    ImGui.CalcButtonSize("New").X - ImGui.GetStyle().ItemSpacing.X);
                if (ImGui.Button("New"))
                {
                    Controller.AddThresholdRule();
                    Controller.Model.IsDirty = true;
                }


                if (Controller.Model.ThresholdRules.Count > 0)
                {
                    if (ImGui.BeginTable("##ThresholdRules_Table", 3, ImGuiTableFlags.BordersInnerH))
                    {
                        ImGui.TableSetupColumn("##Trigger", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("##EffectType", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("##Remove", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcButtonSize("Remove").X);

                        var clipper = ImGui.ImGuiListClipper();

                        clipper.Begin(Controller.Model.ThresholdRules.Count);

                        while (clipper.Step())
                        {
                            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                            {
                                if(i < 0 || i >= Controller.Model.ThresholdRules.Count)
                                    continue;
                                
                                ImGui.PushID(i);
                                var thresholdRule = Controller.Model.ThresholdRules[i];
                                var value = thresholdRule.ThresholdValue;
                                
                                ImGui.TableNextColumn();
                                
                                var needsValue = thresholdRule.Trigger is not (StatThresholdTrigger.OnReachMin or StatThresholdTrigger.OnReachMax);
                                var availWidth = ImGui.GetContentRegionAvail().X;

                                ImGui.SetNextItemWidth(needsValue ? availWidth * 0.6f : -1f);
                                if(ImGui.BeginCombo("##Trigger", thresholdRule.Trigger.ToString()))
                                {

                                    foreach (var triggerType in Enum.GetValues<StatThresholdTrigger>())
                                    {
                                        if (ImGui.Selectable(triggerType.ToString(), triggerType == thresholdRule.Trigger))
                                        {
                                            thresholdRule = thresholdRule with { Trigger = triggerType };
                                            Controller.Model.ThresholdRules[i] = thresholdRule;
                                            Controller.Model.IsDirty = true;
                                        }
                                    }

                                    ImGui.EndCombo();
                                }
                                
                                if (needsValue)
                                {
                                    ImGui.SameLine();
                                    ImGui.SetNextItemWidth(-1f);
                                    if (ImGui.InputDouble("##Value", ref value, 0, 0, "%.1f"))
                                    {
                                        thresholdRule = thresholdRule with { ThresholdValue = value };
                                        Controller.Model.ThresholdRules[i] = thresholdRule;
                                        Controller.Model.IsDirty = true;
                                    }
                                }

                                ImGui.TableNextColumn();
                                
                                var needsExtraData = thresholdRule.Reaction is StatReactionKind.ApplyStatusEffect
                                    or StatReactionKind.RemoveStatusEffect
                                    or StatReactionKind.EmitSignal;
                                var reactAvailWidth = ImGui.GetContentRegionAvail().X;

                                ImGui.SetNextItemWidth(needsExtraData ? reactAvailWidth * 0.55f : -1f);
                                if (ImGui.BeginCombo("##ReactionKind", thresholdRule.Reaction.ToString()))
                                {
                                    foreach (var reactionKind in Enum.GetValues<StatReactionKind>())
                                    {
                                        if (ImGui.Selectable(reactionKind.ToString(), reactionKind == thresholdRule.Reaction))
                                        {
                                            thresholdRule = thresholdRule with { Reaction = reactionKind };
                                            Controller.Model.ThresholdRules[i] = thresholdRule;
                                            Controller.Model.IsDirty = true;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (needsExtraData)
                                {
                                    ImGui.SameLine();
                                    ImGui.SetNextItemWidth(-1f);
                                    if (thresholdRule.Reaction is StatReactionKind.ApplyStatusEffect or StatReactionKind.RemoveStatusEffect)
                                    {
                                        ImGui.Text("Status Effect [WIP]");
                                    }
                                    else if (thresholdRule.Reaction == StatReactionKind.EmitSignal)
                                    {
                                        ImGui.Text("Signal [WIP]");
                                    }
                                }

                                ImGui.TableNextColumn();
                                ImGui.Button("Remove");
                                ImGui.PopID();
                            }
                        }
                        clipper.End();
                        ImGui.EndTable();
                    }
                }

                ImGui.EndTable();
            }

            ImGui.Separator();

            if(ImGui.Button("Save"))
            {
                Controller.Model.IsDirty = false;
                Controller.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                if(Controller.Model.IsDirty)
                    ImModal.Show("Are you sure?", () =>
                    {
                        ImGui.Text("If you close without saving, your changes will be lost.");
                        ImGui.Text("Continue?");
                        if (ImGui.Button("Yes, close without saving"))
                        {
                            Controller.Model.IsDirty = false;
                            ImModal.CloseCurrent();
                            Close();
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("No, keep editing"))
                        {
                            ImModal.CloseCurrent();
                        }
                    });
                else
                    Close();
            }
        }
        ImGui.End();
    }
}
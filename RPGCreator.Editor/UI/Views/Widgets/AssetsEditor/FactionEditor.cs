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
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;

public class FactionEditor(IEditorViewContext viewContext, EditorViewDefinition definition, FactionEditorController controller) : ComplexEditorUiWindowBase<FactionEditorController>(viewContext, definition, controller)
{
    public override void OnActivate(object? args)
    {
        if (args is not FactionDefinition faction)
        {
            Logger.Once.Error("Invalid faction definition provided.");
            Close();
            return;
        }

        Controller.Setup(faction);
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Window)
        {
            Logger.Once.Error("This view is meant to be only drawn as a window.");
            return;
        }

        if (ImGui.Begin(Name))
        {
            ImGui.InputText("Name", ref Controller.Model.Name, 256);
            ImGui.InputTextMultiline("Description", ref Controller.Model.Description, 256);

            ImGui.Separator();

            ImGui.ColorEdit3("Faction Color", ref Controller.Model.ColorRgbEditable);
            ImGui.InputInt("Default Relation", ref Controller.Model.DefaultRelation);
            ImGui.SetItemTooltip("The default relation value of this faction to other factions. For example, if this faction is hostile to all other faction, the default relation value would be -100.\nMinimum: -100\nMaximum: 100");
            
            ImGui.SeparatorText("Specific Relations");
            ImGui.TextDisabled("Override the default relation for specific factions.");
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X -
                                ImGui.CalcButtonSize("Add Relation").X - ImGui.GetStyle().ItemSpacing.X);

            if (ImGui.Button("Add Relation"))
            {
                var availableFactions = Controller.GetAvailableFactions();
                ImModal.Show("Add Faction Relation", () =>
                {
                    if (availableFactions.Count == 0)
                    {
                        ImGui.TextDisabled("No other factions available to add.");
                        return;
                    }

                    if (ImGui.BeginTable("##SelectFactionTable", 2, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("Faction");
                        ImGui.TableSetupColumn("Default Relation");
                        ImGui.TableHeadersRow();

                        foreach (var factionId in availableFactions)
                        {
                            var res = Controller.GetFaction(factionId);
                            if (res.IsFailure) continue;
                            var targetFaction = res.Value;

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            if (ImGui.Selectable($"{targetFaction.Name}##{factionId}", false, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                Controller.AddOrUpdateRelation(factionId, (short)Controller.Model.DefaultRelation);
                                ImModal.CloseCurrent();
                            }

                            ImGui.TableNextColumn();
                            ImGui.Text(targetFaction.DefaultRelation.ToString());
                        }

                        ImGui.EndTable();
                    }
                });
            }

            if (Controller.Model.Relations.Count > 0)
            {
                if (ImGui.BeginTable("##FactionRelationsTable", 4, ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Faction", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Score", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Stance", ImGuiTableColumnFlags.WidthFixed, 80f);
                    ImGui.TableSetupColumn("##Remove", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcButtonSize("Remove").X);
                    ImGui.TableHeadersRow();

                    Ulid? relationToRemove = null;

                    foreach (var targetId in Controller.Model.Relations.Keys)
                    {
                        var score = (int)Controller.Model.Relations[targetId];
                        var targetFactionRes = Controller.GetFaction(targetId);
                        var targetName = targetFactionRes.IsSuccess ? targetFactionRes.Value.Name : $"Unknown ({targetId})";

                        ImGui.PushID(targetId.ToString());
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        if (targetFactionRes.IsSuccess)
                        {
                            var factionColor = ImGui.ColorConvertU32ToFloat4(targetFactionRes.Value.ColorRgba);
                            ImGui.ColorButton("##factionColor", factionColor, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder, new Vector2(14, 14));
                            ImGui.SameLine();
                        }
                        ImGui.TextEllipsis(targetName);

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(-1f);
                        if (ImGui.SliderInt("##RelationScore", ref score, FactionDefinition.HOSTILE_MAX, FactionDefinition.FRIENDLY_MAX))
                        {
                            Controller.AddOrUpdateRelation(targetId, (short)score);
                        }

                        ImGui.TableNextColumn();
                        var stance = score switch
                        {
                            <= -25 => ("Hostile", ImGuiExt.Colors.Error),
                            >= 25 => ("Friendly", ImGuiExt.Colors.Success),
                            _ => ("Neutral", ImGuiExt.Colors.Warning)
                        };
                        ImGui.TextColored(stance.Item2, stance.Item1);

                        ImGui.TableNextColumn();
                        if (ImGui.Button("Remove", ImGuiExt.ButtonType.Error))
                        {
                            relationToRemove = targetId;
                        }

                        ImGui.PopID();
                    }

                    if (relationToRemove.HasValue)
                    {
                        Controller.RemoveRelation(relationToRemove.Value);
                    }

                    ImGui.EndTable();
                }
            }
            else
            {
                ImGui.TextDisabled("No specific relations configured (using Default Relation for all factions).");
            }

            ImGui.Separator();
            if (ImGui.Button("Save Faction"))
            {
                Controller.Save();
                Close();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                Close();
            }
        }
        ImGui.End();
    }
}
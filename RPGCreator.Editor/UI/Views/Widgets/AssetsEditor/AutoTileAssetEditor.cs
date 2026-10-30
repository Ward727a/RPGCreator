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
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;

public class AutoTileAssetEditor(
    IEditorViewContext viewContext,
    EditorViewDefinition definition,
    AutoTileAssetEditorController controller)
    : ComplexEditorUiWindowBase<AutoTileAssetEditorController>(viewContext, definition, controller)
{
    public record AutoTileAssetEditorArguments(AutoTileRulesetDefinition RulesetDefinition);

    private const int NUMBER_OF_SQUARES_PER_ROW = 3;
    private const float NUMBER_OF_SQUARES_PER_ROW_HALF = 1.5f;
    private const float CELL_SIZE_IN_EDITOR = 128.0f;
    private const float CELL_SIZE_IN_PREVIEW = 24.0f;
    
    private static readonly Vector4 REQUIRE_COLOR = new(.5f, 1f, .5f, 1f);
    private static readonly Vector4 IGNORE_COLOR = new(0.5f, 0.5f, 0.5f, 1f);
    private static readonly Vector4 EXCLUDE_COLOR = new(1f, 0.5f, 0.5f, 1f);

    private bool _shouldFocus;

    public override void OnActivate(object? args)
    {
        if (args is AutoTileAssetEditorArguments arguments)
        {
            Controller.Init(arguments);
        }
    }

    public override void OnDeactivate()
    {
        Controller.Clear();
    }

    protected override void OnOpened()
    {
        _shouldFocus = true;
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (_shouldFocus)
        {
            ImGui.SetNextWindowFocus();
            _shouldFocus = false;
        }

        if (ImGui.Begin(Name, ref IsOpenInternal))
        {
            Controller.Update();
            
            if (ImGui.BeginTable("##editor_ruleset_tables", 3, ImGuiTableFlags.Resizable))
            {
                {
                    ImGui.TableSetupColumn("##editor_ruleset_tables.materials_and_info");
                    ImGui.TableSetupColumn("##editor_ruleset_tables.tools_and_editor");
                    ImGui.TableSetupColumn("##editor_ruleset_tables.rules_and_submenu");
                }

                ImGui.TableNextColumn();
                DrawMaterialsAndInfo();

                ImGui.TableNextColumn();
                DrawToolsAndEditor();

                ImGui.TableNextColumn();
                DrawRulesAndSubmenus();

                ImGui.EndTable();
            }
        }

        ImGui.End();
    }

    private void DrawMaterialsAndInfo()
    {
        if (ImGui.BeginListBox("##Materials", new Vector2(-1, -120)))
        {
            var clipper = ImGui.ImGuiListClipper();
            clipper.Begin(Controller.State.Materials.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var material = Controller.State.Materials[i];
                    if (ImGui.Selectable($"{material.MaterialId} - {material.Name}",
                            Controller.State.SelectedMaterialId == material.MaterialId))
                    {
                        Controller.SelectMaterial(material.MaterialId);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    }

                    if (ImGui.BeginItemTooltip())
                    {
                        var materialPreview = Controller.GetMaterialPreview(material.MaterialId);
                        ImGui.TextDisabled("Default Tile:");
                        ImGui.Image(materialPreview.texture, new Vector2(64, 64), materialPreview.defaultTileUv0,
                            materialPreview.defaultTileUv1);
                        ImGui.EndTooltip();
                    }
                }
            }
            
            clipper.End();

            ImGui.EndListBox();
        }

        ImGui.Separator();
        ImGui.BeginGroup();
        {
            ImGui.TextDisabled("Ruleset Name:");
            ImGui.SameLine(0f);
            ImGui.Text(Controller.State.RulesetDefinition?.Name ?? "None");

            ImGui.TextDisabled("Assigned Material:");
            ImGui.SameLine(0f);
            ImGui.Text($"{Controller.State.AssignedMaterialName} #{Controller.State.AssignedMaterialId}");

            ImGui.TextDisabled("Numbers of rules:");
            ImGui.SameLine(0f);
            ImGui.Text($"{Controller.State.RulesetDefinition?.Rules.Count ?? 0}");

            ImGui.TextDisabled("Tileset:");
            ImGui.SameLine(0f);
            ImGui.TextColored(ImGui.GetStyle().Colors[(int)ImGuiCol.TextLink],
                $"[{Controller.State.RulesetDefinition?.TargetTileset}]");
            if (ImGui.IsItemClicked())
            {
                var texture = Controller.State.TilesetTexture;
                ImModal.Show("Tileset Preview", () =>
                {
                    ImGui.Image(texture.Value, new Vector2(256));
                });
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
            ImGui.SetItemTooltip("Click to preview tileset");
        }
        ImGui.EndGroup();
    }
    
    private void DrawToolsAndEditor()
    {
        ImGui.BeginGroup();
        {
            ImGui.BeginDisabled(Controller.State.SelectedToolType == CellConditionMode.Require);
            if (ImGui.Button("Require", ImGuiExt.ButtonType.Success))
            {
                Controller.SelectToolType(CellConditionMode.Require);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.BeginDisabled(Controller.State.SelectedToolType == CellConditionMode.Exclude);
            if (ImGui.Button("Exclude", ImGuiExt.ButtonType.Error))
            {
                Controller.SelectToolType(CellConditionMode.Exclude);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.BeginDisabled(Controller.State.SelectedToolType == CellConditionMode.Ignore);
            if (ImGui.Button("Ignore"))
            {
                Controller.SelectToolType(CellConditionMode.Ignore);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            ImGui.EndDisabled();
            
            ImGui.SameLine();

            float buttonWidth = ImGui.CalcButtonSize("Save").X;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - buttonWidth);

            if (ImGui.Button("Save"))
            {
                Controller.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
            
        }
        ImGui.EndGroup();

        if (ImGui.BeginChild("##Editor", new Vector2(-1, -(64 + ImGui.GetFrameHeightWithSpacing()))))
        {
            var vectoredCellSize = new Vector2(CELL_SIZE_IN_EDITOR, CELL_SIZE_IN_EDITOR);
            
            var localPos = ImGui.GetCursorPos() + (ImGui.GetContentRegionAvail() / 2 -
                                                   vectoredCellSize * NUMBER_OF_SQUARES_PER_ROW_HALF);
            var cursorPos = ImGui.GetCursorScreenPos() + (ImGui.GetContentRegionAvail() / 2 -
                                                          vectoredCellSize * NUMBER_OF_SQUARES_PER_ROW_HALF);

            for (int y = 0; y < NUMBER_OF_SQUARES_PER_ROW; y++)
            {
                for (int x = 0; x < NUMBER_OF_SQUARES_PER_ROW; x++)
                {
                    var vectoredCellSizeMultipliedByCoord = vectoredCellSize * new Vector2(x, y);

                    Vector2 cellMin = cursorPos + vectoredCellSizeMultipliedByCoord;
                    Vector2 cellMax = cellMin + vectoredCellSize;

                    Vector4 color = IGNORE_COLOR;

                    if (Controller.HasSelectedRule)
                    {
                        switch (Controller.State.SelectedRule[x + y * NUMBER_OF_SQUARES_PER_ROW].Mode)
                        {
                            case CellConditionMode.Require:
                                color = REQUIRE_COLOR;
                                break;
                            case CellConditionMode.Ignore:
                                color = IGNORE_COLOR;
                                break;
                            case CellConditionMode.Exclude:
                                color = EXCLUDE_COLOR;
                                break;
                        }
                    }

                    if (x == 1 && y == 1)
                    {
                        if (Controller.HasSelectedRule)
                        {
                            ImGui.SetCursorScreenPos(cellMin);
                            
                            ImGui.Image(Controller.State.TilesetTexture.Value, vectoredCellSize,
                                Controller.State.TileUv.uv0,
                                Controller.State.TileUv.uv1);
                        } else
                            DrawSquare(ImGui.GetWindowDrawList(), cellMin, cellMax, Vector4.One, new Vector4(0, 0, 0, 1f));
                        
                    }
                    else
                    {
                        var buttonPosition = localPos + vectoredCellSizeMultipliedByCoord;
                        
                        DrawSquare(ImGui.GetWindowDrawList(), cellMin, cellMax, color, new Vector4(0, 0, 0, 1f), true);
                        
                        ImGui.SetCursorPos(buttonPosition);
                        ImGui.InvisibleButton($"##Tile{x}{y}", vectoredCellSize);
                        
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                            {
                                Controller.EditRule(x, y);
                            }
                            else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                            {
                                Controller.EditRule(x, y, false);
                            }

                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        }

                        if (Controller.HasSelectedRule && ImGui.BeginItemTooltip())
                        {
                            var materials = Controller.State.SelectedRule[x + y * NUMBER_OF_SQUARES_PER_ROW].TargetMask
                                .GetMaterials();
                            
                            var materialIds = materials.ToList();

                            if (materialIds.Count == 0)
                            {
                                ImGui.TextDisabled("No Materials");
                            }
                            else
                            {
                                ImGui.TextDisabled("Assigned Materials:");
                                foreach (var materialId in materialIds)
                                {
                                    ImGui.Text(Controller.State.Materials[materialId - 1].Name);
                                }
                            }

                            ImGui.EndTooltip();
                        }
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.Separator();
        if (ImGui.BeginChild("##Tiles", new Vector2(-1, 64 + ImGui.GetStyle().ItemSpacing.Y)))
        {
            if (Controller.HasSelectedRule)
            {
                ImGui.Image(Controller.State.TilesetTexture.Value, new Vector2(64, 64), Controller.State.TileUv.uv0,
                    Controller.State.TileUv.uv1);
            }
            else
            {
                ImGui.TextDisabled("No rule selected");
            }
            
            ImGui.BeginDisabled(!Controller.HasSelectedRule);
            
            ImGui.SameLine();
            if (ImGui.Button("Change tile"))
            {
                LocalRouter.ActivateView("rpgc://views/tileset_tile_picker", Controller.GetTilePickerArgs()).OnSuccess(view =>
                {
                    ChangeTileView = view;
                });
                ImGui.OpenPopup("###change_tile_rule");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
            
            ImGui.EndDisabled();
            
            
            DrawChangeTileModal();
        }

        ImGui.EndChild();
    }

    public bool flipH = false;
    public bool flipV = false;
    
    private void DrawRulesAndSubmenus()
    {
        if (ImGui.Button("Add Rule"))
        {
            Controller.CreateRule();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGui.SameLine();
        ImGui.BeginDisabled(Controller.State.SelectedRuleIndex == -1);
        if (ImGui.Button("Delete Rule", ImGuiExt.ButtonType.Error))
        {
            Controller.DeleteRule();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGui.EndDisabled();

        if (ImGui.BeginListBox("##RulesList",
                new Vector2(-1, -(ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y)*2)))
        {
            var clipper = ImGui.ImGuiListClipper();

            clipper.Begin(Controller.State.RulesetDefinition?.Rules.Count ?? 0);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    if (ImGui.Selectable($"Rule {i}", i == Controller.State.SelectedRuleIndex))
                    {
                        Controller.SelectRule(i);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    }

                    if (ImGui.BeginDragDropSource())
                    {
                        unsafe
                        {
                            var payloadIndex = i;
                            ImGui.SetDragDropPayload("Rule", &payloadIndex, sizeof(int));
                        }

                        ImGui.EndDragDropSource();
                    }

                    if (ImGui.BeginDragDropTarget())
                    {
                        unsafe
                        {
                            var payload = ImGui.AcceptDragDropPayload("Rule");
                            if (!payload.IsNull)
                            {
                                var droppedIndex = *(int*)payload.Data;

                                Controller.MoveRule(droppedIndex, i);
                            }
                        }

                        ImGui.EndDragDropTarget();
                    }

                    if (ImGui.BeginItemTooltip())
                    {
                        if (ImGui.BeginChild($"##rule{i}_tooltip",
                                new Vector2(24 * NUMBER_OF_SQUARES_PER_ROW, 24 * NUMBER_OF_SQUARES_PER_ROW)))
                        {
                            var cursorPos = ImGui.GetCursorScreenPos();
                            var drawList = ImGui.GetWindowDrawList();
                            float cellSize = 24.0f;

                            for (int x = 0; x < NUMBER_OF_SQUARES_PER_ROW; x++)
                            {
                                for (int y = 0; y < NUMBER_OF_SQUARES_PER_ROW; y++)
                                {
                                    Vector2 cellMin = cursorPos + new Vector2(x * CELL_SIZE_IN_PREVIEW,
                                        y * CELL_SIZE_IN_PREVIEW);
                                    Vector2 cellMax = cellMin + new Vector2(CELL_SIZE_IN_PREVIEW, CELL_SIZE_IN_PREVIEW);

                                    Vector4 color = Vector4.Zero;

                                    switch (Controller.State.RulesetDefinition?.Rules[i]
                                                .Get(x + y * NUMBER_OF_SQUARES_PER_ROW).Mode)
                                    {
                                        case CellConditionMode.Require:
                                            color = REQUIRE_COLOR;
                                            break;
                                        case CellConditionMode.Ignore:
                                            color = IGNORE_COLOR;
                                            break;
                                        case CellConditionMode.Exclude:
                                            color = EXCLUDE_COLOR;
                                            break;
                                    }

                                    if (x == 1 && y == 1)
                                    {
                                        DrawSquare(drawList, cellMin, cellMax, Vector4.One,
                                            new Vector4(0, 0, 0, 1f));
                                    }
                                    else
                                    {
                                        DrawSquare(drawList, cellMin, cellMax, color,
                                            new Vector4(0, 0, 0, 1f), true);
                                    }
                                }
                            }

                            ImGui.EndChild();
                        }

                        ImGui.EndTooltip();
                    }
                }
            }

            ImGui.EndListBox();
        }

        ImGui.Separator();

        ImGui.BeginGroup();
        {
            ImGui.BeginDisabled(!Controller.HasSelectedRule);

            ImGui.TextDisabled("Selected rule:");
            ImGui.SameLine(0f);
            ImGui.Text(!Controller.HasSelectedRule
                ? "None"
                : $"Rule {Controller.State.SelectedRuleIndex}");

            
            ImGui.TextDisabled("Rule Flip:");
            ImGui.SameLine(0f);
            if (ImGui.Checkbox("H##flipH", ref Controller.State.TileFlipH))
            {
                Controller.UpdateFlip();
            }
            ImGui.SetItemTooltip("Flip the rule horizontally");
            ImGui.SameLine(0f);
            if (ImGui.Checkbox("V##flipV", ref Controller.State.TileFlipV))
            {
                Controller.UpdateFlip();
            }
            ImGui.SetItemTooltip("Flip the rule vertically");
            ImGui.EndDisabled();
        }
        ImGui.EndGroup();
    }

    #region DrawHelpers

    private const float _45_DEGREE_AS_RADIAN = (float)Math.PI / 4;
    
    private void DrawSquare(ImDrawListPtr drawList, Vector2 position, Vector2 size, Vector4 inColor,
        Vector4 borderColor, bool diagonal = false)
    {
        if (diagonal)
        {
            drawList.AddStripedRect(position, size, 0, _45_DEGREE_AS_RADIAN, 2.4f, inColor, 8f);
        }
        else
        {
            drawList.AddRectFilled(position, size, ImGui.ColorConvertFloat4ToU32(inColor));
        }

        drawList.AddRect(position, size, ImGui.ColorConvertFloat4ToU32(borderColor), 0f, thickness: 3f);
    }

    #endregion
    
    #region PopupModal

    private IEditorUiBindableView? ChangeTileView;
    
    private void DrawChangeTileModal()
    {
        if (ImGui.BeginPopupModal("Change tile###change_tile_rule"))
        {
            if (ImGui.Button("Confirm"))
            {
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.BeginChild("###tile_selector", new Vector2(0, -ImGui.CalcButtonSize("Confirm").Y)))
            {
                ChangeTileView?.Draw();
            }
            ImGui.EndChild();
            ImGui.EndPopup();
        }
    }
    
    #endregion
}
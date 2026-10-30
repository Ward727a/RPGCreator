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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Types;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsManager;

public partial class AutoTileAssetManager
{
    public class MainWindow : ComplexEditorUi<AutoTileAssetManagerController.MainWindowController>
    {
        private IEditorUiWindow? _newAutoTileRulesetWindow;
        private readonly float _tableHeight;

        public MainWindow(IEditorViewContext context,
        EditorViewDefinition definition,
        AutoTileAssetManagerController.MainWindowController controller) : base(context, definition, controller)
        {
            controller.RefreshList();
            _tableHeight = -(ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeight());
        }

        private string _searchInput = string.Empty;

        public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
        {
            
            ImGui.InputTextWithHint("##search", "Search...", ref _searchInput, 256);
            ImGui.SameLine();
            ImGui.Button("Search");
            ImGui.SameLine();
            ImGui.Separator(true);
            ImGui.SameLine();
            if (ImGui.Button("New AutoTile Rulesets"))
            {
                LocalRouter.ActivateView("rpgc://views/auto_tile_ruleset/new_autotile_ruleset", Controller.MakeMakerModalArgs()).OnSuccess((view) =>
                {
                    if (view is IEditorUiWindow window)
                    {
                        _newAutoTileRulesetWindow = window;
                        window.Open();
                    }
                });
            }

            DrawCreateModal();

            if (ImGui.BeginTable("##autotiles_list", 4, ImGuiTableFlags.Resizable | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.BordersH | ImGuiTableFlags.Reorderable, new Vector2(0, _tableHeight)))
            {
                // Table setup columns
                {
                    ImGui.TableSetupColumn("#", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.NoReorder,
                        initWidthOrWeight: 20f);
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Material Name");
                    ImGui.TableSetupColumn("Default Tile", ImGuiTableColumnFlags.WidthFixed, initWidthOrWeight: 64f);
                }

                ImGui.TableHeadersRow();
                
                var clipper = ImGui.ImGuiListClipper();
                
                clipper.Begin(Controller.State.Rulesets.Count);

                while (clipper.Step())
                {
                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var rulesetDef = Controller.State.Rulesets[i];
                        ImGui.TableNextColumn();
                        var startY = ImGui.GetCursorPosY();
                        if (ImGui.Selectable($"##{rulesetDef.AssetId.ToString()}", Controller.State.SelectedRuleset?.AssetId == rulesetDef.AssetId, ImGuiSelectableFlags.SpanAllColumns,
                                new Vector2(0, 64)))
                        {
                            Controller.SelectRuleset(rulesetDef);
                        }

                        ImGui.SetCursorPosY(startY);
                        var materialInfo = Controller.GetMaterialInfo(rulesetDef.AssetId);
                        if(materialInfo.id != MaterialId.Empty)
                            ImGui.Text($"{materialInfo.id}");
                        else
                        {
                            ImGui.Text("-1");
                            ImGui.SetItemTooltip("No material assigned");
                        }
                    
                        ImGui.TableNextColumn();
                        ImGui.Text($"{rulesetDef.Name}");
                    
                        ImGui.TableNextColumn();
                        ImGui.Text($"{materialInfo.name}");
                    
                        ImGui.TableNextColumn();
                        var (texture, uv0, uv1) = Controller.GetTilesetTexture(rulesetDef);
                        if (texture.HasValue)
                        {
                            ImGui.Image(texture.Value, new Vector2(64, 64), uv0,
                                uv1);
                        }
                        else
                            ImGui.TextDisabled("[ERROR NO TEXTURE]");
                    }
                    
                }
                
                clipper.End();

                ImGui.EndTable();
                
            }
            
            ImGui.BeginDisabled(!Controller.HasSelectedRuleset);
            if (ImGui.Button("Edit"))
            {
                LocalRouter.ActivateView("rpgc://views/auto_tile_asset_editor", new AutoTileAssetEditor.AutoTileAssetEditorArguments(Controller.State.SelectedRuleset!)).OnSuccess(view =>
                {
                    if (view is IEditorUiWindow window)
                    {
                        EditorWindowManager.Open(window, MainEditorUiWindow.WindowGroup);
                    }
                });
            }
            ImGui.SameLine();
            ImGui.Button("Delete", ImGuiExt.ButtonType.Error);
            ImGui.EndDisabled();
        }

        public void DrawCreateModal()
        {
            _newAutoTileRulesetWindow?.Draw();
        }
    }
}

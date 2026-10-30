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
using System.Linq;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.Editor.UI.Views.EditorTools.Payload;
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
    public class AutoTileMakerModal(
        IEditorViewContext context,
        EditorViewDefinition definition,
        AutoTileAssetManagerController.AutoTileMakerModalController controller)
        : ComplexEditorUiWindowBase<AutoTileAssetManagerController.AutoTileMakerModalController>(context, definition,
            controller)
    {
        
        private IEditorUiWindow? _tilePicker;
        
        public override void OnActivate(object? args)
        {
            if (args is AutoTileAssetManagerController.AutoTileMakerModalArgs arg)
            {
                Controller.Init(arg);
            }
        }

        protected override void OnOpened()
        {
            ImGui.OpenPopup(ImGuiId);
        }
        
        public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
        {
            if (ImGui.BeginPopupModal(Name))
            {
                _tilePicker?.Draw();
                ImGui.InputText("Name", ref Controller.State.TilesetName, 256);

                ImGui.Separator();
                ImGui.Text($"Tileset: ");
                ImGui.SameLine();
                if (ImGui.BeginCombo("##tileset_selector",
                        Controller.State.SelectedTileset != null ? Controller.State.SelectedTileset.Name : "None"))
                {
                    var clipper = ImGui.ImGuiListClipper();
                    var ulids = Controller.GetTilesets();
                    clipper.Begin(ulids.Count);

                    while (clipper.Step())
                    {
                        for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                        {
                            var tileset = Controller.GetTilesetDefinition(ulids[i]);
                            if (tileset == null)
                                ImGui.TextDisabled($"[ERROR: {ulids[i]}]");
                            else
                            {
                                if (ImGui.Selectable(tileset.Name, Controller.State.SelectedTileset == tileset))
                                {
                                    Controller.SelectTileset(tileset);
                                }
                            }
                        }
                    }

                    clipper.End();

                    ImGui.EndCombo();
                }

                ImGui.BeginDisabled(Controller.State.SelectedTileset == null);

                ImGui.Text("Default Tile: ");
                ImGui.SameLine();
                if (Controller.State.DefaultTileIndex == TileIndex.Zero || !Controller.State.TilesetTextureRef.HasValue)
                {
                    ImGui.Text("None");
                }
                else
                {
                    ImGui.Image(Controller.State.TilesetTextureRef.Value, new Vector2(64), Controller.State.TileUv0,
                        Controller.State.TileUv1);
                }

                ImGui.SameLine();
                if (ImGui.Button("Select##tile"))
                {
                    var arg = Controller.UpdateArgs(out var success, out var fromCache);

                    if (!success)
                    {
                        ImNotify.Show("Error", "Couldn't get argument!", NotifyType.Error, 5F);
                    }
                    else
                    {
                        LocalRouter.ActivateView("rpgc://views/tileset_tile_picker", arg).OnSuccess(tilePickerWidget =>
                        {
                            Controller.SetTilePickerWidget(tilePickerWidget);
                        });
                        
                        LocalRouter.ActivateView("rpgc://views/auto_tile_ruleset/tile_picker",Controller.MakeAutoTilePickerArg()).OnSuccess(view =>
                        {

                            if (view is IEditorUiWindow window)
                            {
                                _tilePicker = window;
                                _tilePicker.Open();
                            }
                            
                        });
                    }
                }

                ImGui.EndDisabled();

                if (ImGui.Button("Create"))
                {
                    if (!Controller.CreateAutoTile())
                    {
                        ImNotify.Show("Error", "Couldn't create auto tile!", NotifyType.Error, 5F);
                        return;
                    }

                    Controller.RefreshList();

                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }
    }
}


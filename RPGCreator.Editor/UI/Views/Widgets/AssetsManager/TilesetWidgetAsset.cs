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
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;
using RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsManager;

public class TilesetWidgetAsset : ComplexEditorUi<TilesetWidgetAssetController>
{
    private readonly ScopedLogger _logger = Logger.ForContext<TilesetWidgetAsset>();
    
    public TilesetWidgetAsset(IEditorViewContext context, EditorViewDefinition definition, TilesetWidgetAssetController controller) : base(context, definition, controller)
    {
        controller.RefreshList();
    }

    private string _searchTextField = "";
    
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.14f, 0.14f, 0.14f, 0.4f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4f, 4f));
        
        if(ImGui.BeginChild("##TilesetManager", new Vector2(0, 0), ImGuiChildFlags.AlwaysUseWindowPadding))
        {
            DrawCreatorPopup();
            DrawEditorPopup();

            DrawTopBar();

            DrawList();
            
            ImGui.Separator();

            DrawBottomBar();
        }
        ImGui.EndChild();
        
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }
    

    private void DrawTopBar()
    {
        ImGui.InputTextWithHint("##SearchField", "Search", ref _searchTextField, 256);
            
        ImGui.SameLine();
        if (ImGui.Button("Search"))
        {
            Controller.RefreshList();
        }
            
        ImGui.SameLine();
        ImGui.Separator(true);
            
        ImGui.SameLine(0);
        if (ImGui.Button("New"))
        {
            if (_tilesetCreator == null)
            {
                GlobalRouter.ActivateView("rpgc://views/asset_creator/tileset").OnSuccess(tilesetCreator =>
                {
                    _tilesetCreator = tilesetCreator as TilesetCreator;
                }).OnFailure(error => { _logger.Error(error); });
            }
                
            _tilesetCreator?.OpenPopup();
        }
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("Create a new tileset.");
            ImGui.EndTooltip();
        }
            
        ImGui.SameLine();
        ImGui.Button("Import...");
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("Import a tileset from a file or a resource.");
            ImGui.EndTooltip();
        }
    }

    private void DrawList()
    {
        var bottomHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
            
        if (ImGui.BeginChild("##TilesetTableChild", new Vector2(0, -bottomHeight)))
        {
            if (ImGui.BeginTable("##TilesetTable", 2, ImGuiTableFlags.BordersInnerH))
            {
                foreach (var tilesetDefinition in Controller.State.Tilesets)
                {
                    ImGui.TableNextColumn();
                    var startY = ImGui.GetCursorPosY();

                    if (ImGui.Selectable($"##{tilesetDefinition.AssetId}", Controller.GetSelectedTilesetId() == tilesetDefinition.AssetId,
                            ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, 64)))
                    {
                        Controller.SelectTileset(tilesetDefinition);
                        Logger.Debug("Selected: {assetName}@{assetId}", tilesetDefinition.Name,
                            tilesetDefinition.AssetId);
                    }

                    ImGui.SetCursorPosY(startY);
                    ImGui.TextWrapped(tilesetDefinition.Name);

                    ImGui.TableNextColumn();
                    var image = Controller.GetThumbnail(tilesetDefinition);

                    if (image.IsFailure)
                    {
                        ImGui.Text("No thumbnail");
                    }
                    else
                    {
                        ImGui.Image(image.Value, new Vector2(64));
                        if (ImGui.BeginItemTooltip())
                        {
                            ImGui.Image(image.Value, new Vector2(128));
                            ImGui.TextWrapped(tilesetDefinition.TextureResourcePath);
                            ImGui.EndTooltip();
                        }
                    }
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndChild();
    }

    private void DrawBottomBar()
    {
        ImGui.BeginDisabled(!Controller.HasSelectedTileset());

        if (ImGui.Button("Edit"))
        {
            GlobalRouter.ActivateView("rpgc://views/tileset_editor", new TilesetEditorArg(Controller.State.SelectedTileset!)).
                OnSuccess(view =>
                {
                    if (view is IEditorUiWindow viewWindow)
                    {
                        TilesetEditor = viewWindow;
                        TilesetEditor?.Open();
                    }
                    else
                    {
                        ImNotify.Show("Error!", $"Invalid view type. Expected IEditorUiWindow got\n{view.GetType().Name}", NotifyType.Error, 5f);
                    }
                }).OnFailure(err =>
                {
                    ImNotify.Show("Error!", err, NotifyType.Error, 5f);
                });
        }
        ImGui.SameLine();
        ImGui.Button("Delete");
            
        ImGui.EndDisabled();
    }
    
    private TilesetCreator? _tilesetCreator;
    
    private void DrawCreatorPopup()
    {
        _tilesetCreator?.Draw();
    }

    private IEditorUiWindow? TilesetEditor;
    
    private void DrawEditorPopup()
    {
        TilesetEditor?.Draw();
    }
}
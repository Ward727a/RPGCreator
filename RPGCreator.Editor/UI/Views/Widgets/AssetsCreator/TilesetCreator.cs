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
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Resources;
using ImGuiExt = RPGCreator.Editor.ImGuiLib.ImGuiExt;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class TilesetCreator(IEditorViewContext context, EditorViewDefinition definition, TilesetCreatorController controller)
    : ComplexEditorUi<TilesetCreatorController>(context, definition, controller)
{
    private readonly ResourceSelector.ResourceSelectorArguments _resourceSelectorArguments = new()
    {
        AllowImportFromOs = true,
        ResourceTypes = [IResourceManager.ResourceType.Image],
        OnResourceSelected = resource =>
        {
            controller.SetTilesetImage(resource).OnFailure(err =>
            {
                Logger.Error(err);
            });
        }
    };

    private const string MODAL_ID = "###tileset_creator";

    private ResourceSelector? _resourceSelectorView;
    private bool _imageButtonPressed;
    private bool _isPopupOpen;

    public void OpenPopup()
    {
        _isPopupOpen = true;
        ImGui.OpenPopup(MODAL_ID);
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (!ImGui.IsPopupOpen(MODAL_ID) && Controller.NeedClearState)
            Controller.ClearState();

        ImGui.SetNextWindowSize(new Vector2(500, 300), ImGuiCond.Appearing);
        if (!ImGui.BeginPopupModal("RPG Creator - Tileset Creator###tileset_creator", ref _isPopupOpen,
                ImGuiWindowFlags.NoResize)) return;

        ImGui.BeginTable("TilesetCreatorTable", 2);
        {
            DrawResourceSelectorPopup();

            ImGui.TableNextColumn();
            ImGui.Text("Tileset name");

            ImGui.TableNextColumn();
            if (ImGui.InputText("##TilesetName", ref Controller.State.TilesetName, 255))
                Controller.SetClearState();

            ImGui.TableNextColumn();
            ImGui.Text("Tileset image");

            ImGui.TableNextColumn();

            _imageButtonPressed = Controller.State.TilesetImageTexture.HasValue
                ? ImGui.ImageButton("##TilesetImage", Controller.State.TilesetImageTexture.Value, new Vector2(128, 128))
                : ImGui.Button("Select image");

            if (_imageButtonPressed)
                OpenResourceImporter();

            ImGui.TableNextColumn();
            ImGui.Text("Tile width");

            ImGui.TableNextColumn();
            if (ImGui.InputInt("##TileWidth", ref Controller.State.TileWidth))
                Controller.SetClearState();

            ImGui.TableNextColumn();
            ImGui.Text("Tile height");

            ImGui.TableNextColumn();
            if (ImGui.InputInt("##TileHeight", ref Controller.State.TileHeight))
                Controller.SetClearState();
        }
        ImGui.EndTable();

        DrawFooter();
        ImGui.EndPopup();
    }

    private void DrawFooter()
    {
        const string textBtn1 = "Cancel";
        const string textBtn2 = "Create";

        var spacing = ImGui.GetStyle().ItemSpacing.X;

        var btn1Width = ImGui.CalcButtonSize(textBtn1).X;
        var btn2Width = ImGui.CalcButtonSize(textBtn2).X;

        var totalWidth = btn1Width + spacing + btn2Width;

        var rightAlignPos = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - totalWidth;
        ImGui.SetCursorPosX(rightAlignPos);

        if (ImGui.Button(textBtn2))
        {
            Controller.SubmitTileset()
                .OnFailure(err => { Logger.Error(err); });
        }

        ImGui.SameLine();

        if (ImGui.Button("Cancel", ImGuiExt.ButtonType.Error))
            _isPopupOpen = false;
    }

    private void DrawResourceSelectorPopup()
    {
        _resourceSelectorView?.Draw();
    }
    
    #region helpers

    private void OpenResourceImporter()
    {
        if (_resourceSelectorView == null)
            GlobalRouter.ActivateView("rpgc://views/resource_selector", _resourceSelectorArguments)
                .OnSuccess((view) => { _resourceSelectorView = view as ResourceSelector; });

        ImGui.OpenPopup(_resourceSelectorView?.ModalId);
    }
    
    #endregion
}
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
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsEditor;

public record TilesetEditorArg(TilesetDefinition TilesetToEdit);

public class TilesetEditor(IEditorViewContext viewContext, EditorViewDefinition definition, TilesetEditorController controller) : ComplexEditorUiWindowBase<TilesetEditorController>(viewContext, definition, controller)
{
    private ResourceSelector? _resourceSelectorView;

    public override void OnActivate(object? args)
    {
        if(args is TilesetEditorArg arg)
        {
            Controller.LoadTileset(arg.TilesetToEdit);
        }
        else
        {
            Controller.BlockOpen();
            ImNotify.Show("Error: Invalid argument", 
                "Error when trying to activate the Tileset Editor:\n" +
                "- The argument provided is not of type TilesetEditorArg.",
                NotifyType.Error,
                5f);
        }
    }

    protected override void OnOpened()
    {
        if(Controller.CanOpen)
            ImGui.OpenPopup("###tileset_editor");
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        ImGui.SetNextWindowSize(new Vector2(500, 300), ImGuiCond.Appearing);
        if (ImGui.BeginPopupModal($"Edit {Controller.State.TilesetOriginalName}###tileset_editor", ref IsOpenInternal,
                Controller.GetFlags()))
        {
            ImGui.BeginTable("TilesetCreatorTable", 2);
            {
                _resourceSelectorView?.Draw();
                ImGui.TableNextColumn();
                ImGui.Text("Tileset name");

                ImGui.TableNextColumn();
                if (ImGui.InputText("##TilesetName", ref Controller.State.TilesetName, 255))
                    Controller.SetShouldSave();
                
                ImGui.TableNextColumn();
                ImGui.Text("Tile size");
                
                ImGui.TableNextColumn();
                if (ImGui.InputInt2("##TilesetTileSize", ref Controller.State.TilesetTileSize[0])) 
                    Controller.SetShouldSave();
                
                ImGui.TableNextColumn();
                ImGui.Text("Image");
                
                ImGui.TableNextColumn();
                var _imageButtonPressed = Controller.State.TilesetTexture.HasValue
                    ? ImGui.ImageButton("##TilesetImage", Controller.State.TilesetTexture.Value, new Vector2(128, 128))
                    : ImGui.Button("Select image");

                if (_imageButtonPressed)
                    OpenResourceSelector();
                
                
                ImGui.EndTable();
            }
            
            var _canSave = Controller.CanSave();
            ImGui.BeginDisabled(!_canSave);
            if (ImGui.Button("Save"))
            {
                Controller.Save();
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndDisabled();
            if (!_canSave)
            {
                ImGui.SetItemTooltip("Name, image path and tile size must be set.\nFor tile size, width AND height must be greater than 0.");
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Cancel", ImGuiExt.ButtonType.Error))
            {
                if (!Controller.ShouldSave)
                {
                    ImGui.CloseCurrentPopup();
                }
            }
            
            ImGui.EndPopup();
        }
    }

    private void OpenResourceSelector()
    {
        if (_resourceSelectorView == null)
            GlobalRouter.ActivateView("rpgc://views/resource_selector", Controller.ResourceSelectorArguments)
                .OnSuccess((view) => { _resourceSelectorView = view as ResourceSelector; });

        ImGui.OpenPopup("###resource_selector");
    }
}
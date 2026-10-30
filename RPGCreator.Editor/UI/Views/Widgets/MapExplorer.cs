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
using Fluxor;
using Hexa.NET.ImGui;
using RPGCreator.Editor.UI.Controllers.Widgets;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Scenes;

namespace RPGCreator.Editor.UI.Views.Widgets;

public class MapExplorer(
    IEditorViewContext viewContext,
    EditorViewDefinition definition,
    MapExplorerController controller,
    ISceneManager sceneManager,
    IState<SceneState> sceneState,
    IDispatcher dispatcher)
    : ComplexEditorUiWindowBase<MapExplorerController>(viewContext, definition, controller)
{
    private IEditorUiBindableView _mapCreatorView;

    private Ulid _wantedId = Ulid.Empty;
    
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (ImGui.Begin(Name))
        {
            DrawMapCreatorPopup();
            DrawUnsavedChangeConfirm();
            if (ImGui.Button("New Map"))
            {
                if (GlobalRouter.ActivateView("rpgc://views/map_creator")
                    .OnSuccess((view) => { _mapCreatorView = view; }).IsSuccess)
                {
                    ImGui.OpenPopup("###map_creator");
                }
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Save Map"))
            {
                sceneManager.SaveSceneAsync();
                dispatcher.Dispatch(new SceneStateActions.MapSaved());
            }

            ImGui.Separator();

            foreach (var mapId in Controller.State.Maps)
            {
                Controller.GetMapDefinition(mapId).OnSuccess((map) =>
                {
                    if (ImGui.Selectable($"{map.Name}###{mapId}", sceneState.Value.CurrentMapId == mapId))
                    {
                        if(sceneState.Value.HasUnsavedChanges)
                        {
                            _wantedId = mapId;
                            ImGui.OpenPopup("###unsaved_changes");
                            return;
                        }
                        sceneManager.LoadSceneAsync(map.AssetId);
                    }
                }).OnFailure(err =>
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), $"Error: {err}");
                });
            }
        }

        ImGui.End();
    }

    private void DrawMapCreatorPopup()
    {
        
        ImGui.SetNextWindowSize(new Vector2(580, 200), ImGuiCond.Once);
        if (ImGui.BeginPopupModal("Map Creator###map_creator", ImGuiWindowFlags.NoResize))
        {
            Controller.SetNeedRefresh();
            _mapCreatorView.Draw();
            ImGui.EndPopup();
        }
        else
        {
            Controller.RefreshList();
        }
    }

    private void DrawUnsavedChangeConfirm()
    {
        if(ImGui.BeginPopupModal("Unsaved changes###unsaved_changes", ImGuiWindowFlags.NoResize))
        {
            ImGui.Text("You have unsaved changes. Do you want to discard them?");
            if(ImGui.Button("Yes, DISCARD all"))
            {
                sceneManager.LoadSceneAsync(_wantedId);
                ImGui.CloseCurrentPopup();
                _wantedId = Ulid.Empty;
            }
            ImGui.SetItemTooltip("This will discard all changes, WITHOUT SAVING them.");
            
            ImGui.SameLine();
            if(ImGui.Button("No, go back"))
            {
                ImGui.CloseCurrentPopup();
                _wantedId = Ulid.Empty;
            }
            ImGui.SetItemDefaultFocus();
            ImGui.SetItemTooltip("This will cancel the operation, without saving or discarding any changes.");
            
            ImGui.EndPopup();
        }
    }
}
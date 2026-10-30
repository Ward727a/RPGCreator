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
using RPGCreator.Editor.UI.Controllers.Widgets;
using RPGCreator.SDK.Modules.Editor.Ui;

namespace RPGCreator.Editor.UI.Views.Widgets;

public class LayerExplorer(IEditorViewContext viewContext, EditorViewDefinition definition, LayerExplorerController controller) : ComplexEditorUiWindowBase<LayerExplorerController>(viewContext, definition, controller)
{
    private IEditorUiBindableView? _layerCreatorView;

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (ImGui.Begin(Name))
        {
            DrawPopupCreate();
            var hasMapLoaded = Controller.HasMapLoaded();
            ImGui.BeginDisabled(!hasMapLoaded);
            if (ImGui.Button("New Layer"))
            {
                if (_layerCreatorView == null)
                {
                    GlobalRouter.ActivateView("rpgc://views/map_layer_creator").OnSuccess(view =>
                    {
                        _layerCreatorView = view;
                        ImGui.OpenPopup("###create_layer");
                        _is_popup_open = true;
                    });
                }
                else
                {
                    ImGui.OpenPopup("###create_layer");
                    _is_popup_open = true;
                }
            }

            if (!hasMapLoaded)
            {
                ImGui.SameLine();
                ImGui.Text("No Map Loaded");
            }

            ImGui.Separator();

            if (ImGui.BeginTable("Layer List", 1))
            {
                var layers = Controller.GetLayers();
                
                for (int i = 0; i < layers.Count; i++)
                {
                    var mapLayer = layers[i];
                    
                    var layerType = mapLayer.LayerDefinition.DataType;
                    
                    ImGui.TableNextColumn();
                    
                    ImGui.PushID(i);

                    if (i != Controller.GetActiveLayerIndex())
                    {
                        if (mapLayer.IsVisible)
                        {
                            ImGui.Button(ImIcon.EyeOn);
                        }
                        else
                        {
                            ImGui.Button(ImIcon.EyeOff);
                        }
                    }
                    else
                    {
                        if (Controller.Model._lastActiveDefaultVisibility)
                        {
                            ImGui.Button(ImIcon.EyeOn);
                        }
                        else
                        {
                            ImGui.Button(ImIcon.EyeOff);
                            ImGui.SetItemTooltip("This layer is always visible because it is selected!");
                        }
                    }
                    
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        if (i != Controller.GetActiveLayerIndex())
                        {
                            Controller.SetLayerVisibility(mapLayer, !mapLayer.IsVisible);
                        }
                        else
                        {
                            Controller.Model._lastActiveDefaultVisibility = !Controller.Model._lastActiveDefaultVisibility;
                        }
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Selectable(mapLayer.LayerDefinition.Name, i == Controller.GetActiveLayerIndex()))
                    {
                        Controller.OnLayerSelected(i, layerType);
                    }
                    
                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text(mapLayer.LayerDefinition.Name);
                        ImGui.Text($"Type: {layerType}");
                        ImGui.Text($"Z-Index: {mapLayer.LayerDefinition.ZIndex}");
                        ImGui.EndTooltip();
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        ImGui.MenuItem("Delete");
                        ImGui.SetItemTooltip("This doesn't do anything [WIP]");
                        ImGui.EndPopup();
                    }
                    
                    ImGui.PopID();
                    
                }

                ImGui.EndTable();
            }

            ImGui.EndDisabled();
        }
        ImGui.End();

    }

    private bool _is_popup_open;
    
    private void DrawPopupCreate()
    {
        ImGui.SetNextWindowSize(new Vector2(400, 400), ImGuiCond.Appearing);
        ImGui.SetNextWindowPos(new Vector2(ImGui.GetMainViewport().Size.X / 2 - 200, ImGui.GetMainViewport().Size.Y / 2 - 200), ImGuiCond.Appearing);
        if (ImGui.BeginPopupModal("Create Layer###create_layer", ref _is_popup_open, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            _layerCreatorView?.Draw();
            
            ImGui.EndPopup();
        }
    }
}
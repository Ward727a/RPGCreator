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

using Hexa.NET.ImGui;
using RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Views.Widgets.AssetsCreator;

public class MapLayerCreator : ComplexEditorUi<MapLayerCreatorController>
{
    public MapLayerCreator(IEditorViewContext context,
        EditorViewDefinition definition,
        MapLayerCreatorController controller) : base(context, definition, controller)
    {
        var selectedLayer = Store.GetOrDefault("selected_layer_urn", URN.Empty);
        Controller.SelectLayerType(selectedLayer);
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        ImGui.Text("Map Layer Creator");

        if (ImGui.BeginCombo("Layer type###layer_type", Controller.GetSelectedLayerTypeName()))
        {
            foreach (var (urn, (displayName, type)) in Controller.GetLayersTypes())
            {
                if (ImGui.Selectable(displayName, Controller.IsLayerTypeSelected(urn)))
                {
                    Controller.SelectLayerType(urn);
                    Store.AddOrEdit("selected_layer_urn", urn);
                }
            }
            
            ImGui.EndCombo();
        }

        ImGui.InputText("Layer name###layer_name", ref Controller.State.LayerName, 256);
        
        if (ImGui.Button("Create layer"))
        {
            Controller.CreateLayer();
        }
    }
}
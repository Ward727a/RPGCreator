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

using System.Collections.Generic;
using Fluxor;
using Hexa.NET.ImGui;
using RPGCreator.Editor.Tools.ToolLogics;
using RPGCreator.SDK.Assets.AutoTiles;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Views.EditorTools.Payload;

public class AutoTilePayload(
    IEditorViewContext context,
    EditorViewDefinition definition,
    AutoTilePayloadController controller)
    : ComplexEditorUi<AutoTilePayloadController>(context, definition, controller)
{
    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (drawAs != EDrawAs.Content)
        {
            return;
        }
        
        foreach (var (name, materialId) in Controller.State.Materials)
        {
            if (ImGui.Selectable($"#{materialId} {name}", Controller.State.SelectedMaterialId == materialId))
            {
                Controller.SelectMaterial(materialId);
            }
        }
    }
}

[EditorUiController]
public class AutoTilePayloadController
{
    public readonly AutoTilePayloadModel State;
    private readonly IDispatcher _dispatcher;

    public AutoTilePayloadController(AutoTilePayloadModel state, IMaterialRegistry materialRegistry, IState<AutoTileLayerToolState> toolState, IDispatcher dispatcher)
    {
        State = state;
        State.Materials = [.. materialRegistry.GetMaterials()];
        State.ToolState = toolState;
        _dispatcher = dispatcher;
    }

    public void SelectMaterial(MaterialId materialId)
    {
        _dispatcher.Dispatch(new AutoTileLayerToolActions.SetMaterial(materialId));
    }
}

[EditorUiModel]
public class AutoTilePayloadModel
{
    public IState<AutoTileLayerToolState> ToolState;
    public List<(string name, MaterialId materialId)> Materials = [];
    public MaterialId SelectedMaterialId => ToolState.Value.MaterialId;
}
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
using RPGCreator.Editor.UI.Controllers.EditorTools;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Views.EditorTools;

public class PayloadTool(
    IEditorViewContext viewContext,
    EditorViewDefinition definition,
    PayloadToolController controller)
    : ComplexEditorUiWindowBase<PayloadToolController>(viewContext, definition, controller)
{
    private readonly StringName _tilesetTypePayloadBindId = new("tileset_type_payload");
    private IEditorUiBindableView? _tilesetTypePayloadView;
    private URN _lastPayloadViewId = URN.Empty;

    protected override void OnChildBound(BoundChild child, StringName bindId)
    {
        if (bindId == _tilesetTypePayloadBindId)
        {
            Controller.AddPayloadUi(child.ViewId, child.Payload);
        }
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (ImGui.Begin(Name))
        {
            var payloadViewId = Controller.GetPayloadUi();
            if (payloadViewId != URN.Empty)
            {
                if (_lastPayloadViewId != payloadViewId)
                    _tilesetTypePayloadView = null;

                if (_tilesetTypePayloadView == null)
                    _tilesetTypePayloadView = GlobalRouter.ActivateView(payloadViewId).Match(view => view, err =>
                    {
                        Logger.Error(err);
                        return null;
                    });

                _tilesetTypePayloadView?.Draw(EDrawAs.Content);
            }
            else
            {
                ImGui.Text("No payload for this layer type");
                ImGui.TextWrapped($"[DEBUG] LayerType: {Controller.GetLayerType()}");
            }
        }

        ImGui.End();
    }
}
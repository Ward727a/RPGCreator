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
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Views.EditorTools;

public class MapDrawTool : ComplexEditorUiWindowBase<MapDrawToolController>
{
    public MapDrawTool(IEditorViewContext context, EditorViewDefinition definition, MapDrawToolController controller) : base(context, definition, controller)
    {
        controller.Init(Store);
    }
    
    private readonly StringName _tilesetTypePayloadBindId = new("tileset_type_payload");

    private const string TILESET_TYPE_PARAMETER = "tileset_type";

    private record ChildItem(URN ViewId, URN TilesetTypeId)
    {
        public Ulid Id { get; } = Ulid.NewUlid();
    };

    private Dictionary<URN, List<ChildItem>> _childItems;
    private Dictionary<Ulid, ChildItem> _childItemMap;
    
    protected override void OnChildBound(BoundChild child, StringName bindId)
    {
        if (bindId == _tilesetTypePayloadBindId)
        {
            if (child.Payload.TryGet<URN>(TILESET_TYPE_PARAMETER, out var tilesetTypeUrn))
            {
                var childItem = new ChildItem(child.ViewId, tilesetTypeUrn);
                if (!_childItems.TryGetValue(tilesetTypeUrn, out var list))
                {
                    list = new List<ChildItem>();
                    _childItems[tilesetTypeUrn] = list;
                }
                list.Add(childItem);
                _childItemMap[childItem.Id] = childItem;
            }
        }
    }

    public override void Draw(EDrawAs drawAs = EDrawAs.Unknown)
    {
        if (ImGui.Begin(Name, ref IsOpenInternal, ImGuiWindowFlags.NoCollapse))
        {
            
            if (ImGui.BeginCombo("Selected Tool", Controller.GetTool(Controller.State.SelectedTool)?.DisplayName ?? "none"))
            {
                foreach (var tool in Controller.GetTools())
                {
                    if (ImGui.Selectable(tool.DisplayName))
                    {
                        Controller.SelectTool(tool.ToolUrn);
                    }
                }
                
                ImGui.EndCombo();
            }
        }
        ImGui.End();
    }
}
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

using System.Collections.ObjectModel;
using System.Numerics;
using RPGCreator.SDK;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace _BaseModule.Tools;


public class SimplePen : ToolLogic
{
    public override URN ToolUrn { get; protected set; } = ToolUrnModule.ToUrnModule("rpgc").ToUrn("simple_pen");
    public override string DisplayName => "Simple Pen";
    public override string Description => "A simple pen tool for drawing on map layers.";
    public override string Icon => "mdi-pencil";
    public override PipedPath Category { get; } = MapCategory.Extend("Drawing");
    
    private IMapService? _mapService;
    
    private IntParameter SizeParameter { get; } = new IntParameter(
        "Size",
        "Defines the drawing size of the pen.",
        10,
        1,
        100,
        1
        );

    private BoolParameter ShowPreviewParameter { get; } = new BoolParameter(
        "Show Preview",
        "Whether to show a preview of the pen's drawing area.",
        true
    );

    public override ObservableCollection<IToolParameter> GetParameters()
    {
        return [
            SizeParameter,
            ShowPreviewParameter
        ];
    }

    public override void UseAt(Vector2? absolutePosition = null)
    {
        if (absolutePosition == null && Payload != null)
        {
            return;
        }

        _mapService ??= RuntimeServices.MapService;

        if (!_mapService.HasSelectedLayer) return;
        var layer = _mapService.GetSelectedLayer();
        
    }
}
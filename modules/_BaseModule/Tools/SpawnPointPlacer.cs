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
using _BaseModule.AssetDefinitions.SpawnPoint;
using Avalonia.Controls;
using RPGCreator.SDK;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace _BaseModule.Tools;

public class SpawnPointPlacer : ToolLogic
{
    public static URN Urn = ToolUrnModule.ToUrnModule("rpgc").ToUrn("spawn_point_placer");
    public override URN ToolUrn { get; protected set; } = Urn;
    public override string DisplayName => "Spawn Point Placer";
    public override string Description => "A tool for placing spawn points on the map.\n" +
                                          "For now, it's limited to one spawn point per map, that will be used to spawn the player character.";
    public override string Icon => "gameIcon-spawn-node";
    public override PipedPath Category => EntityCategory.Extend("Spawn Points");
    public override EPayloadType PayloadType => EPayloadType.Custom;
    private IMapService? _mapService;
    
    public override ObservableCollection<IToolParameter> GetParameters()
    {
        return [];
    }

    public override void UseAt(Vector2? absolutePosition = null, MouseButton button = MouseButton.Left)
    {
        if (absolutePosition == null)
        {
            return;
        }

        Payload = new SpawnPointData();
        
        Vector2 clickPos = AbsolutePositionToMapPosition(absolutePosition!.Value);
        
        _mapService ??= RuntimeServices.MapService;

        if (!_mapService.HasSelectedLayer) return;
        
        var layer = _mapService.GetSelectedLayer();
        if (layer.CanPaintObject(Payload) && Payload != null)
        {
            var paintTarget = layer.GetPaintTarget();
            if (paintTarget != null)
            {
                if (button == MouseButton.Right)
                {
                    paintTarget.EraseAt(clickPos);
                    return;
                }
                paintTarget.PaintAt(clickPos, Payload);
                Logger.Debug("Added spawn point at position: {position}", clickPos);
            }
        }
    }

    public override object? GetCustomPayloadUiControl()
    {
        return new TextBlock()
        {
            Text = "This tool has no need to select anything."
        };
    }
}
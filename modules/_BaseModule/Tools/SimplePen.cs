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
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
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
    public override EPayloadType PayloadType => EPayloadType.AutoTile | EPayloadType.SimpleTile;

    public override URN HelpKey => "rpgc".ToUrnNamespace().ToUrnModule("docs").ToUrn("tool_simple_pen");

    private IMapService? _mapService;
    private int _currentLayerIndex;
    private object? _lastObjectDraw;
    private bool _wasLastObjectSuccess;
    private IPaintTarget? _paintTarget;
    
    private IntParameter SizeParameter { get; } = new IntParameter(
        "Size",
        "Defines the drawing size of the pen.",
        1,
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
        return
        [
            SizeParameter,
            ShowPreviewParameter
        ];
    }

    public override void UseAt(Vector2? absolutePosition = null, MouseButton button = MouseButton.Left)
    {
        if (absolutePosition == null || Payload == null || button != MouseButton.Left)
        {
            if (button == MouseButton.Right && absolutePosition != null)
            {
                TryEraseAt(absolutePosition.Value);
            }
            return;
        }
        
        Vector2 clickPos = AbsolutePositionToMapPosition(absolutePosition!.Value);
        
        _mapService ??= RuntimeServices.MapService;

        if (!_mapService.HasSelectedLayer) return;
        var layerIndex = GlobalStates.MapState.CurrentLayerIndex;
        
        if(CheckIfCanSkipUseReload(clickPos, layerIndex))
        {
            _wasLastObjectSuccess = true;
            return;
        }
        
        var layer = _mapService.GetSelectedLayer();

        _lastObjectDraw = Payload;

        if (layer.CanPaintObject(Payload) && Payload != null)
        {
            _paintTarget = layer.GetPaintTarget();
            
            if(_paintTarget != null)
                DoCommandForSize(clickPos, commandWithPayload: _paintTarget.PaintAt);
            else
                Logger.Warning("Selected layer can paint the object but returned a null paint target.");
        }
        else
        {
            Logger.Warning("Selected layer cannot paint the current object or object is null.");
        }
    }

    private void TryEraseAt(Vector2? absolutePosition)
    {
        if (absolutePosition == null)
        {
            Logger.Error("Attempted to erase with SimplePen but no position was provided.");
            return;
        }
        
        Vector2 clickPos = AbsolutePositionToMapPosition(absolutePosition.Value);
        
        _mapService ??= RuntimeServices.MapService;
        if (!_mapService.HasSelectedLayer) return;
        var layerIndex = GlobalStates.MapState.CurrentLayerIndex;
        
        if(CheckIfCanSkipEraseReload(clickPos, layerIndex))
        {
            Logger.Debug("Erased at position without needing to reload paint target.");
            return;
        }
        
        var layer = _mapService.GetSelectedLayer();
        
        _paintTarget = layer.GetPaintTarget();
        if(_paintTarget != null)
            DoCommandForSize(clickPos, _paintTarget.EraseAt);
        else
            Logger.Warning("No paint target available for erasing at the specified position.");
    }
    
    private void DoCommandForSize(Vector2 clickPos, Action<Vector2>? command = null, Action<Vector2, object>? commandWithPayload = null)
    {
        int size = SizeParameter.GetValueAs<int>(out var success);
        
        if(!success)
        {
            Logger.Error("Failed to get size parameter value for SimplePen. Defaulting to size 1.");
            size = 1;
        }
        
        if (size <= 1)
        {
            if(command != null)
                command(clickPos);
            else if (commandWithPayload != null)
                commandWithPayload(clickPos, Payload!);
            return;
        }

        int halfSize = size / 2;
        for (int x = -halfSize; x <= halfSize; x++)
        {
            for (int y = -halfSize; y <= halfSize; y++)
            {
                Vector2 offset = MultiplyVector2ByMapGridSize(new Vector2(x, y));
                
                Vector2 offsetPos = new Vector2(clickPos.X + offset.X, clickPos.Y + offset.Y);
                
                if(command != null)
                    command(offsetPos);
                else if (commandWithPayload != null)
                    commandWithPayload(offsetPos, Payload!);
            }
        }
    }
    
    private Vector2 MultiplyVector2ByMapGridSize(Vector2 position)
    {
        var mapDef = GlobalStates.MapState.CurrentMapDef;
        if (mapDef == null)
        {
            Logger.Error("Attempted to multiply position by map grid size but no current map definition is available.");
            return position;
        }
        
        return new Vector2(position.X * mapDef.GridParameter.CellWidth, position.Y * mapDef.GridParameter.CellHeight);
    }

    public override void MoveInsideViewport(Vector2 absolutePosition, Vector2 deltaPosition)
    {
        Vector2 clickPos = AbsolutePositionToMapPosition(absolutePosition);

        if (_paintTarget == null || _paintTarget.MapDef == null ||
            _paintTarget.MapDef != GlobalStates.MapState.CurrentMapDef)
        {
            var layer = _mapService?.GetSelectedLayer();
            if (layer == null)
                return;

            if (layer.CanPaintObject(Payload) && Payload != null)
            {
                _paintTarget = layer.GetPaintTarget();
            }
            else
            {
                Logger.Warning("Selected layer cannot paint the current object or object is null.");
                return;
            }
        }
        if(_paintTarget != null && Payload != null)
            _paintTarget.PreviewAt(clickPos, Payload);
        else
            Logger.Warning("Cannot preview because paint target or payload is null.");
    }

    private bool CheckIfCanSkipUseReload(Vector2 clickPos, int layerIndex)
    {
        if (layerIndex != _currentLayerIndex)
        {
            _currentLayerIndex = layerIndex;
            return false; // Layer changed, cannot skip
        }

        if (_lastObjectDraw != Payload || !_wasLastObjectSuccess || Payload == null)
        {
            _lastObjectDraw = Payload;
            _wasLastObjectSuccess = false;
            return false; // Object to paint changed, cannot skip
        }

        if (_paintTarget == null)
        {
            return false; // We don't have a paint action, cannot skip
        }

        if (_paintTarget.MapDef == null || _paintTarget.MapDef != GlobalStates.MapState.CurrentMapDef)
        {
            return false; // Map changed, cannot skip
        }

        DoCommandForSize(clickPos, commandWithPayload: _paintTarget.PaintAt);
        return true; // Successfully repainted without needing to reload
    }
    
    private bool CheckIfCanSkipEraseReload(Vector2 clickPos, int layerIndex)
    {
        if (layerIndex != _currentLayerIndex)
        {
            _currentLayerIndex = layerIndex;
            return false; // Layer changed, cannot skip
        }

        if (_paintTarget == null)
        {
            return false; // We don't have a paint action, cannot skip
        }

        if (_paintTarget.MapDef == null || _paintTarget.MapDef != GlobalStates.MapState.CurrentMapDef)
        {
            return false; // Map changed, cannot skip
        }

        DoCommandForSize(clickPos, _paintTarget.EraseAt);
        return true; // Successfully erased without needing to reload
    }

    public override void OnDeactivate()
    {
        _lastObjectDraw = null;
        _wasLastObjectSuccess = false;
        _paintTarget = null;
        _mapService = null;
    }
}
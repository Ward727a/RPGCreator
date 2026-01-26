using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Types.Editor.Interfaces;
using RPGCreator.Core.Types.Editor.Visual;
using RPGCreator.Core.Types.Editor.Visual.PaintTargets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.Layers;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using Serilog;

namespace RPGCreator.Core.Types.Editor.Context;

public static class MapEditorContext
{
    
    private static IPaintTarget? _activePaintTargetCache;

    
    public static void Initialize()
    {
        EngineStates.BrushState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IBrushState.CurrentMode))
            {
                RefreshPaintTarget();
            }
        };
    }

    public static IPaintTarget? GetActivePaintTarget()
    {
        if (_activePaintTargetCache == null)
        {
            RefreshPaintTarget();
        }
        return _activePaintTargetCache;
    }

    private static void RefreshPaintTarget()
    {
        _activePaintTargetCache = null;

        var selectedLayer = RuntimeServices.LayerService.GetSelectedLayer();
        var map = RuntimeServices.MapService.CurrentLoadedMapDefinition;

        if (EngineStates.BrushState.CurrentMode == BrushMode.Tiling && selectedLayer is TileLayerDefinition tileLayerDefinition)
        {
            _activePaintTargetCache = new TileLayerTarget(tileLayerDefinition, map, 32, 32);
        }
        else if (EngineStates.BrushState.CurrentMode == BrushMode.Tiling && selectedLayer is AutoLayerDefinition autoLayerDefinition)
        {
            _activePaintTargetCache = new IntGridLayerTarget(autoLayerDefinition, map);
        }
        else if (EngineStates.BrushState.CurrentMode == BrushMode.Entities && selectedLayer is EntitiesLayerDefinition entityLayerDefinition)
        {
            _activePaintTargetCache = new EntityLayerTarget(entityLayerDefinition, map, 32, 32);
        }
        
        Log.Debug("Paint Target Rebuilt");
    }
    
    public static object? SelectedObjectToPaint => EngineStates.BrushState.CurrentObjectToPaint;
    
    #region Drawing State
    public static ITileDef? SelectedTile => EngineStates.BrushState.CurrentObjectToPaint as ITileDef;
    public static IntGridData? SelectedIntGridData => EngineStates.BrushState.CurrentObjectToPaint as IntGridData;
    #endregion

    #region Placement State
    public static EditorEntityVisual? SelectedEntity => EngineStates.BrushState.CurrentObjectToPaint as EditorEntityVisual;
    #endregion
    
}

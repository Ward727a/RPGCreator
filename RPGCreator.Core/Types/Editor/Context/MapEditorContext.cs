using RPGCreator.Core.Types.Editor.Visual.PaintTargets;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.RuntimeService;
using Serilog;

namespace RPGCreator.Core.Types.Editor.Context;

public static class MapEditorContext
{
    private static IPaintTarget? _activePaintTargetCache;
    
    public static void Initialize()
    {
        GlobalStates.BrushState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IBrushState.CurrentMode))
            {
                RefreshPaintTarget();
            }
        };
        RuntimeServices.OnceServiceReady((ILayerService LayerService) => LayerService.OnLayerSelected += (layerIndex) =>
        {
            RefreshPaintTarget();
        });
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

        if (GlobalStates.BrushState.CurrentMode == BrushMode.Tiling && selectedLayer is TileLayerDefinition tileLayerDefinition)
        {
            _activePaintTargetCache = new TileLayerTarget(tileLayerDefinition, map, 32, 32);
        }
        else if (GlobalStates.BrushState.CurrentMode == BrushMode.Tiling && selectedLayer is AutoLayerDefinition autoLayerDefinition)
        {
            _activePaintTargetCache = new IntGridLayerTarget(autoLayerDefinition, map);
        }
        else if (GlobalStates.BrushState.CurrentMode == BrushMode.Entities && selectedLayer is EntityLayerDefinition entityLayerDefinition)
        {
            _activePaintTargetCache = new EntityLayerTarget(entityLayerDefinition, map, 32, 32);
        }
        
        Log.Debug("Paint Target Rebuilt");
    }
    
    public static object? SelectedObjectToPaint => GlobalStates.BrushState.CurrentObjectToPaint;
    
    #region Drawing State
    public static ITileDef? SelectedTile => GlobalStates.BrushState.CurrentObjectToPaint as ITileDef;
    public static IntGridData? SelectedIntGridData => GlobalStates.BrushState.CurrentObjectToPaint as IntGridData;
    #endregion

    #region Placement State
    public static EntitySpawner? SelectedEntity => GlobalStates.BrushState.CurrentObjectToPaint as EntitySpawner;
    #endregion
    
}

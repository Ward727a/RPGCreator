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
    private static IMapDef? _map;
    private static BaseLayerDef? _selectedLayer;
    private static MapInstance? _mapInstance;

    public static MapInstance? MapInstance => _mapInstance;
    
    public static void Initialize()
    {
        EngineStates.BrushState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IBrushState.CurrentMode))
            {
                RefreshPaintTarget();
            }
        };
        
        EngineStates.EditorState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IEditorState.CurrentLayer))
            {
                _selectedLayer = EngineStates.EditorState.CurrentLayer;
                Guard.IsNotNull(_selectedLayer);
            } else if (e.PropertyName == nameof(IEditorState.CurrentMap))
            {
                _map = EngineStates.EditorState.CurrentMap;
                Guard.IsNotNull(_map);
                _mapInstance = EngineServices.GameFactory.CreateInstance<MapInstance>(_map);
                Guard.IsNotNull(_mapInstance);
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

        Guard.IsNotNull(_selectedLayer);
        Guard.IsNotNull(_map);

        if (EngineStates.BrushState.CurrentMode == BrushMode.Tiling && _selectedLayer is TileLayerDefinition tileLayerDefinition)
        {
            _activePaintTargetCache = new TileLayerTarget(tileLayerDefinition, _map, 32, 32);
        }
        else if (EngineStates.BrushState.CurrentMode == BrushMode.Tiling && _selectedLayer is AutoLayerDefinition autoLayerDefinition)
        {
            _activePaintTargetCache = new IntGridLayerTarget(autoLayerDefinition, _map);
        }
        else if (EngineStates.BrushState.CurrentMode == BrushMode.Entities && _selectedLayer is EntitiesLayerDefinition entityLayerDefinition)
        {
            _activePaintTargetCache = new EntityLayerTarget(entityLayerDefinition, _map, 32, 32);
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

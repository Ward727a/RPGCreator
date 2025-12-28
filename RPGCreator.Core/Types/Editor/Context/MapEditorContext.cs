using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Editor.Interfaces;
using RPGCreator.Core.Types.Editor.Visual;
using RPGCreator.Core.Types.Editor.Visual.PaintTargets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using Serilog;

namespace RPGCreator.Core.Types.Editor.Context;

public partial class MapEditorContext : ObservableObject
{

    public event Action? MapChanged;
    
    private IPaintTarget? _activePaintTargetCache;
    
    [ObservableProperty]
    private object? _selectedObjectToPaint;


    [ObservableProperty]
    private EditorMode _currentMode = EditorMode.Tiling;
    partial void OnCurrentModeChanged(EditorMode value) => RefreshPaintTarget();

    [ObservableProperty]
    private object? _selectedLayer;
    partial void OnSelectedLayerChanged(object? value) => RefreshPaintTarget();

    [ObservableProperty]
    private MapDefinition? _map;
    partial void OnMapChanged(MapDefinition? value)
    {
        RefreshPaintTarget();
        MapChanged?.Invoke();
        MapInstance? instance = null;
        if(value != null)
            instance = EngineCore.Instance.Managers.Assets.MapFactory.Create(value);
        MapInstance = instance;
        EngineCore.Instance.Data.OnEditedMapChanged(instance);
    }

    public MapInstance? MapInstance { get; set; }

    public IPaintTarget? GetActivePaintTarget()
    {
        if (_activePaintTargetCache == null)
        {
            RefreshPaintTarget();
        }
        return _activePaintTargetCache;
    }

    private void RefreshPaintTarget()
    {
        _activePaintTargetCache = null;

        if (Map == null) return;

        if (CurrentMode == EditorMode.Tiling && SelectedLayer is TileLayerDefinition tileLayerDefinition)
        {
            _activePaintTargetCache = new TileLayerTarget(tileLayerDefinition, Map, 32, 32);
        }
        else if (CurrentMode == EditorMode.Tiling && SelectedLayer is AutoLayerDefinition autoLayerDefinition)
        {
            _activePaintTargetCache = new IntGridLayerTarget(autoLayerDefinition, Map);
        }
        else if (CurrentMode == EditorMode.Entities && SelectedLayer is EntitiesLayerDefinition entityLayerDefinition)
        {
            _activePaintTargetCache = new EntityLayerTarget(entityLayerDefinition, Map, 32, 32);
        }
        
        Log.Debug("Paint Target Rebuilt");
    }
    
    #region Drawing State
    public bool IsDrawing { get; set; } = false;
    public Point LastDrawAt { get; set; } = new(-1, -1);
    public IBrush? ActiveBrush { get; set; }
    public ITileDef? SelectedTile => _selectedObjectToPaint as ITileDef;
    public IntGridData? SelectedIntGridData => _selectedObjectToPaint as IntGridData;
    #endregion

    #region Placement State
    public bool IsPlacing { get; set; } = false;
    public Point LastPlacementAt { get; set; } = new(-1, -1);
    public EditorEntityVisual? SelectedEntity => _selectedObjectToPaint as EditorEntityVisual;
    #endregion
    
}

public enum EditorMode
{
    Tiling,
    Entities
}
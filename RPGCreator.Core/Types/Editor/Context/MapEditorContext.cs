using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Editor.Interfaces;
using RPGCreator.Core.Types.Editor.Visual;
using RPGCreator.Core.Types.Editor.Visual.PaintTargets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using Serilog;

namespace RPGCreator.Core.Types.Editor.Context;

public partial class MapEditorContext : ObservableObject
{
    public MapDefinition Map { get; }
    
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
    private MapDefinition? _currentMapDef;
    partial void OnCurrentMapDefChanged(MapDefinition? value) => RefreshPaintTarget();


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

        if (CurrentMapDef == null) return;

        if (CurrentMode == EditorMode.Tiling && SelectedLayer is TileLayerDefinition tileLayerDefinition)
        {
            _activePaintTargetCache = new TileLayerTarget(tileLayerDefinition, CurrentMapDef, 32, 32);
        }
        else if (CurrentMode == EditorMode.Entities && SelectedLayer is EntitiesLayerDefinition entityLayerDefinition)
        {
            _activePaintTargetCache = new EntityLayerTarget(entityLayerDefinition, CurrentMapDef, 32, 32);
        }
        
        Log.Debug("Paint Target Rebuilt");
    }
    
    #region Drawing State
    public bool IsDrawing { get; set; } = false;
    public Point LastDrawAt { get; set; } = new(-1, -1);
    public IBrush? ActiveBrush { get; set; }
    public ITileDef? SelectedTile => _selectedObjectToPaint as ITileDef;
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
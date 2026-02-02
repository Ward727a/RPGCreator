using System;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.RTP.Services;

public class LayerService : ObservableObject, ILayerService
{
    private readonly ScopedLogger _logger = Logger.ForContext<LayerService>(); 
    private bool _hasSelectedLayer;
    
    public Action<int>? OnLayerSelected { get; set; }

    public bool HasSelectedLayer
    {
        get => _hasSelectedLayer;
        private set => SetProperty(ref _hasSelectedLayer, value);
    }
    
    private bool _canSelectLayer;
    public bool CanSelectLayer 
    {
        get => _canSelectLayer;
        private set => SetProperty(ref _canSelectLayer, value);
    }
    
    private int _currentLayerIndex;
    public int CurrentLayerIndex
    {
        get => _currentLayerIndex;
        private set => SetProperty(ref _currentLayerIndex, value);
    }
    
    private int _layerCount;
    public int LayerCount
    {
        get => _layerCount;
        private set => SetProperty(ref _layerCount, value);
    }

    public LayerService()
    {
        RuntimeServices.MapService.OnMapLoaded += OnMapLoaded;
        RuntimeServices.MapService.OnMapUnloaded += OnMapUnloaded;
    }

    public void SelectLayer(int layerIndex)
    {
        ValidateLayerAvailability();
        if (!CanSelectLayerIndex(layerIndex))
        {
            layerIndex = ClampLayerIndex(layerIndex);
            _logger.Warning("The provided layer index was out of bounds. It has been clamped to {layerIndex}.", args: layerIndex);
        }
        
        CurrentLayerIndex = layerIndex;
        HasSelectedLayer = true;
        OnLayerSelected?.Invoke(layerIndex);
    }

    public bool TryAddLayer(BaseLayerDef layerDef)
    {
        if (!RuntimeServices.MapService.HasLoadedMap)
            return false;
        var mapDef = RuntimeServices.MapService.CurrentLoadedMapDefinition;
        if (mapDef == null)
            return false;
        
        return mapDef.AddLayer(layerDef);
    }

    public bool TryRemoveLayer(int layerIndex)
    {
        if (!RuntimeServices.MapService.HasLoadedMap)
            return false;
        var mapDef = RuntimeServices.MapService.CurrentLoadedMapDefinition;
        if (mapDef == null)
            return false;
        
        if (!CanSelectLayerIndex(layerIndex))
            return false;
        
        var layerDef = GetLayerDefAt(layerIndex);
        return mapDef.RemoveLayer(layerDef);
    }

    public LayerData GetLayerData(int layerIndex)
    {
        ValidateLayerAvailability();
        if (!CanSelectLayerIndex(layerIndex))
        {
            layerIndex = ClampLayerIndex(layerIndex);
            _logger.Warning("The provided layer index was out of bounds. It has been clamped to {layerIndex}.", args: layerIndex);
        }
        
        var layerDef = GetLayerDefAt(layerIndex);
        return CreateLayerData(layerDef);
    }

    public BaseLayerDef GetSelectedLayer()
    {
        ValidateLayerAvailability();
        return GetLayerDefAt(CurrentLayerIndex);
    }

    #region Helpers

    private void ValidateLayerAvailability()
    {
        if(!RuntimeServices.MapService.HasLoadedMap)
            throw new InvalidOperationException("No map is currently loaded.");
        
        if(LayerCount == 0)
            throw new InvalidOperationException("The current loaded map has no layers.");
        
        if(!CanSelectLayer)
            throw new InvalidOperationException("The layer service cannot select layers at this time.");
    }
    
    private bool CanSelectLayerIndex(int layerIndex)
    {
        return layerIndex >= 0 && layerIndex < LayerCount;
    }
    
    private int ClampLayerIndex(int layerIndex)
    {
        if (layerIndex < 0)
            return 0;
        if (layerIndex >= LayerCount)
            return LayerCount - 1;
        return layerIndex;
    }
    
    private LayerData CreateLayerData(BaseLayerDef layerDef)
    {
        return new LayerData(
            layerDef.ZIndex,
            layerDef.Name,
            layerDef.VisibleByDefault
        );
    }
    
    private BaseLayerDef GetLayerDefAt(int layerIndex)
    {
        var mapDef = RuntimeServices.MapService.CurrentLoadedMapDefinition;
        if (mapDef == null)
            throw new InvalidOperationException("No map is currently loaded.");
        
        return mapDef.TileLayers[layerIndex];
    }

    #endregion

    #region Events

    private void OnMapUnloaded()
    {
        var mapDef = RuntimeServices.MapService.CurrentLoadedMapDefinition;
        if (mapDef != null)
        {
            mapDef.TileLayerAdded -= OnLayerAdded;
            mapDef.TileLayerRemoved -= OnLayerRemoved;
        }
        else
        {
            _logger.Error("Map unloaded event triggered, but no map definition was found, events could not be unsubscribed!!");
        }
        LayerCount = 0;
        CanSelectLayer = false;
        HasSelectedLayer = false;
        CurrentLayerIndex = -1;
    }

    private void OnMapLoaded(Ulid mapId)
    {
        var mapDef = RuntimeServices.MapService.CurrentLoadedMapDefinition;
        if (mapDef != null)
        {
            LayerCount = mapDef.TileLayers.Count;
            CanSelectLayer = LayerCount > 0;
            CurrentLayerIndex = CanSelectLayer ? 0 : -1;
            HasSelectedLayer = CanSelectLayer;
            mapDef.TileLayerAdded += OnLayerAdded;
            mapDef.TileLayerRemoved += OnLayerRemoved;
        }
        else
        {
            LayerCount = 0;
            CanSelectLayer = false;
            CurrentLayerIndex = -1;
        }
    }
    
    private void OnLayerAdded(BaseLayerDef layerDef)
    {
        LayerCount++;
        CanSelectLayer = LayerCount > 0;
    }
    private void OnLayerRemoved(BaseLayerDef layerIndex)
    {
        LayerCount--;
        CanSelectLayer = LayerCount > 0;
        if (CurrentLayerIndex >= LayerCount)
        {
            CurrentLayerIndex = LayerCount - 1;
        }
    }

    #endregion

    public void Dispose()
    {
        RuntimeServices.MapService.OnMapLoaded -= OnMapLoaded;
        RuntimeServices.MapService.OnMapUnloaded -= OnMapUnloaded;
        GC.SuppressFinalize(this);
    }
}
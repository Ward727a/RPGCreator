using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using RPGCreator.EditorLib.Mediator.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.MapLayers;
using RPGCreator.SDK.Assets.Maps;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Services.RuntimeService;

namespace RPGCreator.RTP.Services;

public class LayerService : ObservableObject, ILayerService,
    INotificationHandler<MapMessage.MapLayerAddedMessage>,
    INotificationHandler<MapMessage.MapLayerRemovedMessage>,
    INotificationHandler<MapMessage.MapLoadedMessage>,
    INotificationHandler<MapMessage.MapUnloadedMessage>
{
    private readonly MapMdrService _mapMdrService;
    private readonly IMapService _mapService;
    private readonly ScopedLogger _logger = Logger.ForContext<LayerService>();

    public Action<int>? OnLayerSelected { get; set; }

    public bool HasSelectedLayer
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public bool CanSelectLayer
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public int CurrentLayerIndex
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public int LayerCount
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public LayerService(MapMdrService mapMdrService, IMapService mapService)
    {
        _mapMdrService = mapMdrService;
        _mapService = mapService;
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

    public async Task<bool> TryAddLayer(IMapLayerDefinition layerDef)
    {
        if (!_mapService.HasLoadedMap)
            return false;
        var mapDef = _mapService.CurrentLoadedMapDefinition;
        if (mapDef == null)
            return false;

        await _mapMdrService.AddLayerAsync(mapDef, layerDef);
        return true;
    }

    public async Task<bool> TryRemoveLayer(int layerIndex)
    {
        if (!_mapService.HasLoadedMap)
            return false;
        var mapDef = _mapService.CurrentLoadedMapDefinition;
        if (mapDef == null)
            return false;
        
        if (!CanSelectLayerIndex(layerIndex))
            return false;
        
        var layerDef = GetLayerDefAt(layerIndex);
        await _mapMdrService.RemoveLayerAsync(mapDef, layerDef);
        return true;
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

    public IMapLayerDefinition GetSelectedLayer()
    {
        ValidateLayerAvailability();
        return GetLayerDefAt(CurrentLayerIndex);
    }

    #region Helpers

    private void ValidateLayerAvailability()
    {
        if(!_mapService.HasLoadedMap)
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
    
    private LayerData CreateLayerData(IMapLayerDefinition layerDef)
    {
        return new LayerData(
            layerDef.ZIndex,
            layerDef.Name,
            layerDef.Visible
        );
    }
    
    private IMapLayerDefinition GetLayerDefAt(int layerIndex)
    {
        var mapDef = _mapService.CurrentLoadedMapDefinition;
        if (mapDef == null)
            throw new InvalidOperationException("No map is currently loaded.");
        
        return mapDef.Layers[layerIndex];
    }

    #endregion

    #region Events

    private void OnMapUnloaded()
    {
        var mapDef = _mapService.CurrentLoadedMapDefinition;
        if (mapDef != null)
        {
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

    private void MapLoaded(Ulid mapId)
    {
        var mapDef = _mapService.CurrentLoadedMapDefinition;
        if (mapDef != null)
        {
            LayerCount = mapDef.Layers.Count;
            CanSelectLayer = LayerCount > 0;
            CurrentLayerIndex = CanSelectLayer ? 0 : -1;
            HasSelectedLayer = CanSelectLayer;
        }
        else
        {
            LayerCount = 0;
            CanSelectLayer = false;
            CurrentLayerIndex = -1;
        }
    }
    
    private void OnLayerAdded()
    {
        LayerCount++;
        CanSelectLayer = LayerCount > 0;
    }
    private void OnLayerRemoved()
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
        GC.SuppressFinalize(this);
    }

    public Task Handle(MapMessage.MapLayerAddedMessage notification, CancellationToken cancellationToken)
    {
        OnLayerAdded();
        return Task.CompletedTask;
    }

    public Task Handle(MapMessage.MapLayerRemovedMessage notification, CancellationToken cancellationToken)
    {
        OnLayerRemoved();
        return Task.CompletedTask;
    }

    public Task Handle(MapMessage.MapLoadedMessage notification, CancellationToken cancellationToken)
    {
        MapLoaded(notification.MapId);
        return Task.CompletedTask;
    }

    public Task Handle(MapMessage.MapUnloadedMessage notification, CancellationToken cancellationToken)
    {
        OnMapUnloaded();
        return Task.CompletedTask;
    }
}
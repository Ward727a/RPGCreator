using System;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.RTP.Services;

public partial class MapService : ObservableObject, IMapService
{
    public Action<Ulid>? OnMapLoaded { get; }
    public Action? OnMapUnloaded { get; }
    public bool HasLoadedMap { get; private set;  }
    public bool IsMapDirty { get; private set;  }
    public MapData CurrentLoadedMapData { get; private set;  }
    public IMapDef? CurrentLoadedMapDefinition { get; private set;  }
    
    public bool LoadMap(Ulid mapId)
    {
        
        OnMapLoaded?.Invoke(mapId);
        return true;
    }

    public void UnloadMap()
    {
        OnMapUnloaded?.Invoke();
    }

    public void PlaceObjectAt(float x, float y, object objectToPlace)
    {
        throw new NotImplementedException();
    }

    public void PlaceAssetAt(float x, float y, Ulid assetId)
    {
        throw new NotImplementedException();
    }

    public void PlaceTileAt(int x, int y, TileData tileData)
    {
        throw new NotImplementedException();
    }

    public bool TryGetObjectAt(float x, float y, out object foundObject)
    {
        throw new NotImplementedException();
    }

    public bool TryGetTileAt(int x, int y, out TileData tileData)
    {
        throw new NotImplementedException();
    }
}
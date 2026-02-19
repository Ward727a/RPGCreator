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

using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.SDK.GlobalState;

public class MapState : BaseState, IMapState
{
    public bool HasCurrentMap 
    { 
        get; 
        set => SetProperty(ref field, value); 
    }

    public bool IsMapDirty 
    { 
        get; 
        set => SetProperty(ref field, value); 
    }

    public Ulid CurrentMapId 
    { 
        get; 
        set => SetProperty(ref field, value); 
    }

    public MapData? CurrentMapData 
    { 
        get; 
        set => SetProperty(ref field, value); 
    }

    public IMapDef? CurrentMapDef
    {
        get;
        set
        {
            SetProperty(ref field, value);
            CurrentMapId = value?.Unique ?? Ulid.Empty;
        }
    }

    public bool HasSelectedLayer
    {
        get
        {
            if(CurrentMapDef == null) return false;
            return CurrentLayerIndex >= 0 && CurrentLayerIndex < CurrentMapDef.TileLayers.Count;
        }
    }
    
    public bool CanSelectLayer 
    { 
        get
        {
            if(CurrentMapDef == null) return false;
            return CurrentMapDef.TileLayers.Count > 0;
        }
    }
    
    public int CurrentLayerIndex 
    { 
        get; 
        set => SetProperty(ref field, value); 
    }
    
    public int LayerCount {
        get
        {
            if(CurrentMapDef == null) return 0;
            return CurrentMapDef.TileLayers.Count;
        }
    }

    public override void Reset()
    {
        HasCurrentMap = false;
        IsMapDirty = false;
        CurrentMapId = Ulid.Empty;
        CurrentMapData = null;
        CurrentMapDef = null;
        CurrentLayerIndex = 0;
    }
}
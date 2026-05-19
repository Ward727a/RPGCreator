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

using Fluxor;
using RPGCreator.SDK.States.Actions;

namespace RPGCreator.SDK.States.Reducers;

public static class MapReducers
{
    [ReducerMethod]
    public static MapState ReduceOpenMap(MapState state, MapActions.OpenMapAction action)
    {
        if (action.MapDef.Unique == state.MapId)
            return state;

        var hasSelectedLayer = action.MapDef.Layers.Count > 0;
        
        return new MapState(MapDef: action.MapDef, SelectedLayerIndex: hasSelectedLayer ? 0 : int.MinValue);
    }

    [ReducerMethod]
    public static MapState ReduceCloseMap(MapState state, MapActions.CloseMapAction action)
        => new(MapDef: null, SelectedLayerIndex: int.MinValue);

    [ReducerMethod]
    public static MapState ReduceSelectLayer(MapState state, MapActions.SelectLayerAction action)
    {
        if (state.SelectedLayerIndex == action.LayerIndex)
            return state;

        return state with { SelectedLayerIndex = action.LayerIndex };
    }

    [ReducerMethod]
    public static MapState ReduceDeselectLayer(MapState state, MapActions.DeselectLayerAction action)
        => state with { SelectedLayerIndex = int.MinValue };
}
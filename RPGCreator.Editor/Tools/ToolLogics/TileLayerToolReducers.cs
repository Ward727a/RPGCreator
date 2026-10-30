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
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.Tools.ToolLogics;

public class TileLayerToolReducers
{
    [ReducerMethod]
    public static TileLayerToolState ReduceSetTilesetAction(TileLayerToolState state, TileLayerToolActions.SetTilesetAction action)
    {
        var start = action.NewStart ?? TileIndex.Zero;
        var end = action.NewEnd ?? TileIndex.Zero;
        if (state.SelectedTilesetIndex == action.SelectedTilesetIndex && state.TilesByRow == action.TilesByRow && state.StartTileIndex == start && state.EndTileIndex == end) return state;
        
        // ReSharper disable once WithExpressionModifiesAllMembers
        return state with { SelectedTilesetIndex = action.SelectedTilesetIndex, TilesByRow = action.TilesByRow, StartTileIndex = start, EndTileIndex = end};
    }
    
    [ReducerMethod]
    public static TileLayerToolState ReduceSetSelectedTileIndexAction(TileLayerToolState state, TileLayerToolActions.SetSelectedTileIndexAction action)
    {
        if (state.StartTileIndex == action.Start && state.EndTileIndex == action.End) return state;
        
        return state with { StartTileIndex = action.Start, EndTileIndex = action.End };
    }
    
    [ReducerMethod]
    public static TileLayerToolState ReduceSetTilesByRowAction(TileLayerToolState state, TileLayerToolActions.SetTilesByRowAction action)
    {
        if (state.TilesByRow == action.TilesByRow) return state;
    
        return state with { TilesByRow = action.TilesByRow };
    }
}
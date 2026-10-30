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

namespace RPGCreator.SDK.Scenes;

public static class SceneStateReducers
{
    [ReducerMethod]
    public static SceneState ReduceMapLoadStarted(SceneState state, SceneStateActions.MapLoadStarted action)
    {
        return state with { IsLoading = true, HasUnsavedChanges = false };
    }
    
    [ReducerMethod]
    public static SceneState ReduceMapLoadCompleted(SceneState state, SceneStateActions.MapLoadCompleted action)
    {
        return state with { 
            CurrentMapId = action.MapId, 
            MapSize = action.MapSize,
            IsLoading = false,
            ActiveLayerIndex = action.NewLayerIndex,
            ActiveLayerType = action.FirstLayerType
        };
    }
    
    [ReducerMethod]
    public static SceneState ReduceMapModified(SceneState state, SceneStateActions.MapModified action)
    {
        if (state.HasUnsavedChanges) return state; 
        
        return state with { HasUnsavedChanges = true };
    }
    
    [ReducerMethod]
    public static SceneState ReduceMapSaved(SceneState state, SceneStateActions.MapSaved action)
    {
        if(!state.HasUnsavedChanges) return state;
        
        return state with { HasUnsavedChanges = false };
    }
    
    [ReducerMethod]
    public static SceneState ReduceActiveLayerChanged(SceneState state, SceneStateActions.ActiveLayerChanged action)
    {
        if (action.NewLayerIndex == state.ActiveLayerIndex) return state;
        
        return state with { ActiveLayerIndex = action.NewLayerIndex, ActiveLayerType = action.NewLayerType };
    }
}
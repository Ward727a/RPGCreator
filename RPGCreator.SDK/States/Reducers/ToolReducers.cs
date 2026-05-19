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

using System.Collections.Immutable;
using Fluxor;
using RPGCreator.SDK.States.Actions;

namespace RPGCreator.SDK.States.Reducers;

public static class ToolReducers
{
    [ReducerMethod]
    public static ToolState ReduceChangeTool(ToolState state, ToolActions.ChangeToolAction action)
    {
        return state with
        {
            ActiveTool = action.NewTool,
            ParameterValue = action.NewTool.GetDefaultParameters().ToImmutableDictionary(p => p.DisplayName, p => p.DefaultValue),
            Payload = null
        };
    }

    [ReducerMethod]
    public static ToolState ReduceChangeToolPayload(ToolState state, ToolActions.ChangeToolPayloadAction action)
    {
        return state with { Payload = action.NewPayload };
    }
    
    [ReducerMethod]
    public static ToolState ReduceClearToolPayload(ToolState state, ToolActions.ClearToolPayloadAction action)
    {
        return state with { Payload = null };
    }
    
    [ReducerMethod]
    public static ToolState ReduceUpdateToolParameterValue(ToolState state, ToolActions.UpdateToolParameterValueAction action)
    {
        if (!state.ParameterValue.ContainsKey(action.ParameterName))
        {
            return state;
        }
        
        return state with { ParameterValue = state.ParameterValue.SetItem(action.ParameterName, action.NewValue) };
    }
}
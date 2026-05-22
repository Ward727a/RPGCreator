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

public static class GameSessionReducers
{
    [ReducerMethod]
    public static GameSessionState ReduceStartNewSession(GameSessionState state, GameSessionActions.StartNewSessionAction action)
    {
        return new GameSessionState(IsSessionActive: true, CurrentPlayerId: -1, CurrentMapId: Ulid.Empty);
    }

    [ReducerMethod]
    public static GameSessionState ReduceSetPlayerEntityId(GameSessionState state, GameSessionActions.SetPlayerEntityIdAction action)
    {
        return state with { CurrentPlayerId = action.EntityId };
    }

    [ReducerMethod]
    public static GameSessionState ReduceSetMapId(GameSessionState state, GameSessionActions.SetMapIdAction action)
    {
        return state with { CurrentMapId = action.MapId };
    }

    [ReducerMethod]
    public static GameSessionState ReduceStopSession(GameSessionState state, GameSessionActions.StopSessionAction action)
    {
        return new GameSessionState(IsSessionActive: false, CurrentPlayerId: -1, CurrentMapId: Ulid.Empty);
    }
}
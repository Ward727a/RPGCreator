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

using System;
using System.Collections.Generic;
using Fluxor;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.Tools;

namespace RPGCreator.Editor.Tools.ToolLogics;

[ToolLogic("rpgc://layer_type/actor_layer")]
public class ActorLayerToolLogic(IState<ActorLayerToolState> state, IDispatcher dispatcher) : IToolLogic
{
    private Ulid SelectedActorId => state.Value.SelectedActorId;

    public IReadOnlyList<Point> Apply(IMapLayer targetLayer, Point gridPos, bool forPhantom = false)
    {
        if (targetLayer is not ActorLayer actorLayer || SelectedActorId == Ulid.Empty)
            return [];

        actorLayer.Add(gridPos, new ActorSpawnData(
            InstanceId: Ulid.NewUlid(),
            ActorDefinitionId: SelectedActorId,
            GridPosition: gridPos,
            FacingDirection: state.Value.FacingDirection
        ));

        return [gridPos];
    }

    public void Erase(IMapLayer targetLayer, Point gridPos, bool forPhantom = false)
    {
        if (targetLayer is not ActorLayer actorLayer)
            return;

        actorLayer.Remove(gridPos);
    }

    public void Pick(IMapLayer targetLayer, Point gridPos)
    {
        if (targetLayer is not ActorLayer actorLayer)
            return;

        var spawn = actorLayer.Get(gridPos);
        if (spawn == null)
            return;

        dispatcher.Dispatch(new ActorLayerToolActions.SetActor(spawn.Value.ActorDefinitionId));
        dispatcher.Dispatch(new ActorLayerToolActions.SetFacingDirection(spawn.Value.FacingDirection));
    }

    public void UpdateState()
    {
    }
}
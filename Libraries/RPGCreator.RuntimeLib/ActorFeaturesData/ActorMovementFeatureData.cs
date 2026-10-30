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

using Arch.Core;
using RPGCreator.RuntimeLib.ECS.Components.Actor;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.RuntimeLib.ActorFeaturesData;

[TypeDiscriminator("rpgc", "sdk", "actor_features", "movement")]
[ActorFeature("Movement", "Locomotion", "Allows the actor to move and run across the map.")]
public class ActorMovementFeatureData : IActorFeatureData
{

    /// <summary>
    /// Walking speed in pixels per second.
    /// </summary>
    public float WalkSpeed { get; set; } = 96f;

    /// <summary>
    /// Running speed in pixels per second.
    /// </summary>
    public float RunSpeed { get; set; } = 160f;

    public bool CanRun { get; set; } = true;
    public bool AllowDiagonalMovement { get; set; } = true;
    public ActorDirection InitialFacingDirection { get; set; } = ActorDirection.Down;

    public void ApplyToEntity(World world, Entity entity)
    {
        world.Add(entity, new ActorMovementComponent
        {
            WalkSpeed = WalkSpeed,
            RunSpeed = RunSpeed,
            CanRun = CanRun,
            AllowDiagonalMovement = AllowDiagonalMovement,
            FacingDirection = InitialFacingDirection
        });
    }

    public void CollectDependencies(in List<Ulid> dependencies)
    {
    }
}
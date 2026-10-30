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
using Microsoft.Xna.Framework;
using RPGCreator.RuntimeLib.ECS.Components.Actor;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.RuntimeLib.ActorFeaturesData;

[TypeDiscriminator("rpgc", "sdk", "actor_features", "collider")]
[ActorFeature("Collider", "Collision", "Allows the actor to collide with other actors and objects.")]
public class ActorColliderFeatureData : IActorFeatureData
{
    public int OffsetX { get; set; } = 4;
    public int OffsetY { get; set; } = 16;
    public int Width { get; set; } = 24;
    public int Height { get; set; } = 16;
    public bool IsSolid { get; set; } = true;
    
    public void ApplyToEntity(World world, Entity entity)
    {
        world.Add(entity, new ActorColliderComponent()
        {
            LocalBounds = new Rectangle(OffsetX, OffsetY, Width, Height),
            IsSolid = IsSolid
        });
    }

    public void CollectDependencies(in List<Ulid> dependencies)
    {
    }
}
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

using RPGCreator.SDK.Services.Map;

namespace RPGCreator.SDK.ECS.Components;

/// <summary>
/// Attached to the layer entity representing an ActorLayer in the ECS World.
/// </summary>
public struct ActorLayerComponent(ActorLayer layer)
{
    public ActorLayer Layer { get; init; } = layer;
}

/// <summary>
/// Attached to each individual spawned actor entity on the map, linking it back to its ActorDefinition and spawn instance.
/// </summary>
public struct ActorInstanceComponent
{
    public Ulid InstanceId;
    public Ulid ActorDefinitionId;
    public ActorLayer ParentLayer;
}
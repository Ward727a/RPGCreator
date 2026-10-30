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

using Arch.Buffer;
using Arch.Core;
using Arch.System;
using RPGCreator.SDK.Scenes;

namespace RPGCreator.SDK.ECS;

/// <summary>
/// A derived class of BaseSystem that put all required constructor arguments so you don't forget them.
/// </summary>
/// <param name="world">The world instance where the system will operate.</param>
/// <param name="commandBuffer">The command buffer used for mutable operations.</param>
/// <param name="sceneRender">The scene render used for rendering operations.</param>
public abstract class RpgcBaseSystem(World world, CommandBuffer commandBuffer, ISceneRender sceneRender) : BaseSystem<World, double>(world)
{
    protected ISceneRender SceneRender { get; } = sceneRender;
}
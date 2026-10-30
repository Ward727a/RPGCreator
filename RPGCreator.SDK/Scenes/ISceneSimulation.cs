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

namespace RPGCreator.SDK.Scenes;

public interface ISceneSimulation
{
    /// <summary>
    /// Defines the various phases of the system execution pipeline, ensuring a structured and ordered simulation flow.
    /// </summary>
    /// <remarks>
    /// Each phase is designed to handle specific types of operations, please refer to the documentation for detailed guidance on what each phase is intended for.
    /// </remarks>
    public enum SystemPhase
    {
        /// <summary>
        /// Input phase, used for reading hardware states and translating them into intents.<br/>
        /// <b>Good for:</b>
        /// <ul>
        ///     <li>Reading Keyboard, Mouse, and Gamepad states</li>
        ///     <li>Creating Intent components (e.g., MoveIntent, AttackIntent)</li>
        /// </ul><br/>
        /// <b>Bad for:</b>
        /// <ul>
        ///     <li>Moving entities directly (use Logic instead)</li>
        ///     <li>Game logic or rendering</li>
        /// </ul>
        /// </summary>
        Input,

        /// <summary>
        /// Logic phase, the brain of the game. Processes intents, AI, and rules.<br/>
        /// <b>Good for:</b>
        /// <ul>
        ///     <li>AI behaviors and Pathfinding</li>
        ///     <li>Status effects (Poison damage ticks, Regen)</li>
        ///     <li>Converting 'MoveIntent' into 'Velocity'</li>
        ///     <li>Spawning or destroying entities</li>
        /// </ul><br/>
        /// <b>Bad for:</b>
        /// <ul>
        ///     <li>Resolving collisions or moving coordinates directly</li>
        ///     <li>Drawing anything</li>
        /// </ul>
        /// </summary>
        Logic,

        /// <summary>
        /// Physics phase, strictly applies movement and resolves overlap/collisions.<br/>
        /// <b>Good for:</b>
        /// <ul>
        ///     <li>Adding Velocity to Position</li>
        ///     <li>Checking hitboxes (AABB) and Z-Elevation</li>
        ///     <li>Pushing entities out of walls or each other</li>
        /// </ul><br/>
        /// <b>Bad for:</b>
        /// <ul>
        ///     <li>Reading inputs or making AI choices</li>
        ///     <li>Rendering</li>
        /// </ul>
        /// </summary>
        Physics,

        /// <summary>
        /// Post-physics phase, reacts to the final, resolved positions of entities.<br/>
        /// <b>Good for:</b>
        /// <ul>
        ///     <li>Camera tracking (must happen after physics to avoid jitter)</li>
        ///     <li>Triggering area events (e.g., stepping on a teleport pad)</li>
        ///     <li>Updating Spatial Hash grids or QuadTrees</li>
        /// </ul><br/>
        /// <b>Bad for:</b>
        /// <ul>
        ///     <li>Modifying velocity or positions again</li>
        ///     <li>Rendering</li>
        /// </ul>
        /// </summary>
        PostPhysics,

        /// <summary>
        /// Rendering phase, prepares and sends data to the RenderOrchestrator.<br/>
        /// <b>Good for:</b>
        /// <ul>
        ///     <li>Updating sprite animations (changing frames based on time)</li>
        ///     <li>Sending draw calls (TileSystem, SpriteSystem) to the Orchestrator</li>
        ///     <li>Updating UI elements attached to entities (Health bars)</li>
        /// </ul><br/>
        /// <b>Bad for:</b>
        /// <ul>
        ///     <li>Mutating the core game state (Position, Health, Logic)</li>
        ///     <li>Destroying entities</li>
        /// </ul>
        /// <remarks>
        /// Every system in this phase must be read-only, and should NEVER modify any entity components.
        /// </remarks>
        /// </summary>
        Rendering,
    }

    World? CurrentWorld { get; }
    CommandBuffer? SharedCommandBuffer { get; }

    void RegisterSystem<TSystem>(SystemPhase phase) where TSystem : ISystem<double>;
    void UnregisterSystem<TSystem>(SystemPhase phase) where TSystem : ISystem<double>;
    
    public World CreateWorld(ISceneRender sceneRender);
    public void AttachWorldComponent<T>(T component = default) where T : struct;
    void DestroyWorld();

    void Update(double deltaTime);
    public void ExtractRenderData(double deltaTime);

}
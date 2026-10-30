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
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.SDK.Scenes;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.RuntimeLib.Scenes;

[EngineService(typeof(ISceneSimulation), ServiceLifetime.Transient)]
public class SceneSimulation : ISceneSimulation
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<Type>[] _registeredSystems;
    
    private Entity _worldEntity = Entity.Null;

    private Group<double>? _inputGroup;
    private Group<double>? _logicGroup;
    private Group<double>? _physicsGroup;
    private Group<double>? _postPhysicsGroup;
    private Group<double>? _renderGroup;

    /// <summary>
    /// Gets the current instance of the <see cref="World"/> being managed by the scene simulation.
    /// </summary>
    /// <remarks>
    /// This property represents the active world instance used for managing entities and systems within
    /// the scene simulation. It will be <c>null</c> when no world has been created or after the world
    /// has been destroyed using the <see cref="DestroyWorld"/> method.
    /// </remarks>
    public World? CurrentWorld { get; private set; }

    /// <summary>
    /// Gets the shared instance of the <see cref="CommandBuffer"/> used to record and execute commands
    /// on the active <see cref="World"/> during the scene simulation.
    /// </summary>
    /// <remarks>
    /// This property is initialized when a new world is created through the <see cref="CreateWorld"/> method
    /// and disposed when the world is destroyed using the <see cref="DestroyWorld"/> method. The shared
    /// command buffer facilitates the system's ability to schedule actions and manipulate entities
    /// and components consistently across simulation phases. It will be <c>null</c> when no world is active.
    /// </remarks>
    public CommandBuffer? SharedCommandBuffer { get; private set; }

    public SceneSimulation(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _registeredSystems = new List<Type>[5];
    
        for (var i = 0; i < 5; i++)
        {
            _registeredSystems[i] = [];
        }
    }

    /// <summary>
    /// Registers a system to a specified simulation phase.
    /// This method associates a system factory with the given phase, enabling its creation and execution during the simulation lifecycle.
    /// </summary>
    /// <typeparam name="TSystem">The type of the system to register, which must implement the <see cref="ISystem{T}"/> interface with a float type parameter.</typeparam>
    /// <param name="phase">The phase during which the system will be executed.</param>
    /// <param name="systemFactory">A factory function to create instances of the system, given a world context.</param>
    public void RegisterSystem<TSystem>(ISceneSimulation.SystemPhase phase)
        where TSystem : ISystem<double>
    {
        var type = typeof(TSystem);
        if (!_registeredSystems[(int)phase].Contains(type))
        {
            _registeredSystems[(int)phase].Add(type);
        }
    }
    /// <summary>
    /// Unregisters a system from a specified simulation phase.
    /// This method removes the association of a system type from the given phase, preventing its execution during the simulation lifecycle.
    /// </summary>
    /// <typeparam name="TSystem">The type of the system to unregister, which must implement the <see cref="ISystem{T}"/> interface with a float type parameter.</typeparam>
    /// <param name="phase">The phase from which the system will be unregistered.</param>
    public void UnregisterSystem<TSystem>(ISceneSimulation.SystemPhase phase)
        where TSystem : ISystem<double>
    {
        _registeredSystems[(int)phase].Remove(typeof(TSystem));
    }

    /// <summary>
    /// Initializes and creates a new game world.
    /// This method sets up the necessary groups and phases for the simulation,
    /// including Input, Logic, Physics, PostPhysics, and Rendering.
    /// </summary>
    /// <remarks>
    /// If a world already exists, it is destroyed before creating a new one.
    /// The method ensures proper organization of the simulation's systems into groups.
    /// </remarks>
    public World CreateWorld(ISceneRender sceneRender)
    {
        DestroyWorld();

        CurrentWorld = World.Create();
        SharedCommandBuffer = new CommandBuffer();

        _worldEntity = CurrentWorld.Create<WorldEntityFlag>();

        _inputGroup = CreateGroup("Input", ISceneSimulation.SystemPhase.Input, sceneRender);
        _logicGroup = CreateGroup("Logic", ISceneSimulation.SystemPhase.Logic, sceneRender);
        _physicsGroup = CreateGroup("Physics", ISceneSimulation.SystemPhase.Physics, sceneRender);
        _postPhysicsGroup = CreateGroup("PostPhysics", ISceneSimulation.SystemPhase.PostPhysics, sceneRender);
        _renderGroup = CreateGroup("Rendering", ISceneSimulation.SystemPhase.Rendering, sceneRender);
        
        return CurrentWorld;
    }

    public void AttachWorldComponent<T>(T component = default) where T : struct
    {
        if (CurrentWorld == null || !CurrentWorld.IsAlive(_worldEntity))
            return;

        if (!CurrentWorld.Has<T>(_worldEntity))
        {
            CurrentWorld.Add(_worldEntity, component);
        }
        else
        {
            CurrentWorld.Set(_worldEntity, component);
        }
    }

    /// <summary>
    /// Destroys the currently active game world and releases all associated resources.
    /// This includes disposing of all system groups and removing the world instance from memory.
    /// </summary>
    /// <remarks>
    /// If no active world exists, this method performs no action.
    /// System groups such as Input, Logic, Physics, PostPhysics, and Rendering are properly disposed of before the world is destroyed.
    /// After execution, the CurrentWorld property is set to null.
    /// </remarks>
    public void DestroyWorld()
    {
        if (CurrentWorld == null) return;

        _inputGroup?.Dispose();
        _logicGroup?.Dispose();
        _physicsGroup?.Dispose();
        _postPhysicsGroup?.Dispose();
        _renderGroup?.Dispose();

        SharedCommandBuffer?.Dispose();
        SharedCommandBuffer = null;

        CurrentWorld.Destroy(_worldEntity);
        _worldEntity = Entity.Null;
        
        World.Destroy(CurrentWorld);
        CurrentWorld = null;
    }

    /// <summary>
    /// Updates all system groups within the simulation for the current game world.
    /// This method processes the Input, Logic, Physics, PostPhysics, and Rendering phases
    /// sequentially using the provided delta time.
    /// </summary>
    /// <remarks>
    /// If no game world has been created, the method exits without performing any updates.
    /// Each system group is updated in the order they were initialized during the world creation.
    /// </remarks>
    /// <param name="deltaTime">
    /// The time elapsed since the last update, typically used to normalize simulation behavior.
    /// </param>
    public void Update(double deltaTime)
    {
        if (CurrentWorld == null) return;
        
        if(!CurrentWorld.IsAlive(_worldEntity))
            throw new Exception("World entity is not alive! Has it been destroyed?");

        SharedCommandBuffer!.Playback(CurrentWorld);

        UpdateGroup(_inputGroup!, deltaTime);
        SharedCommandBuffer!.Playback(CurrentWorld);

        UpdateGroup(_logicGroup!, deltaTime);
        SharedCommandBuffer!.Playback(CurrentWorld);

        UpdateGroup(_physicsGroup!, deltaTime);
        SharedCommandBuffer!.Playback(CurrentWorld);

        UpdateGroup(_postPhysicsGroup!, deltaTime);
    }
    
    public void ExtractRenderData(double deltaTime)
    {
        if (CurrentWorld == null || _renderGroup == null) return;

        UpdateGroup(_renderGroup, deltaTime);
    }

    #region Helpers

    private Group<double> CreateGroup(string name, ISceneSimulation.SystemPhase phase, ISceneRender sceneRender)
    {
        var systemTypes = _registeredSystems[(int)phase];
        var systems = new ISystem<double>[systemTypes.Count];

        for (int i = 0; i < systemTypes.Count; i++)
        {
            systems[i] = (ISystem<double>)ActivatorUtilities.CreateInstance(
                _serviceProvider, 
                systemTypes[i], 
                CurrentWorld!,
                SharedCommandBuffer!,
                sceneRender
            );
        }
        
        var group = new Group<double>(name, systems);
        group.Initialize();

        return group;
    }

    private void UpdateGroup(Group<double> group, in double deltaTime)
    {
        group.BeforeUpdate(deltaTime);
        group.Update(deltaTime);
        group.AfterUpdate(deltaTime);
    }

    #endregion
}
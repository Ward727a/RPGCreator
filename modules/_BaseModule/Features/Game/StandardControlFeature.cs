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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using _BaseModule.Features.Entity;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Modules.Features.Game;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Game;

[GameFeature]
public class StandardControlFeature : BaseGameFeature
{
    public override int FeaturePriority { get; } = 100;
    public override string FeatureName => "Standard Player Control";
    public override string FeatureDescription => "Allows the controlled entity to be moved by the player with the keyboard.\n" +
                                                 "All controls can be reconfigured in the input settings in the editor parameters.";
    public override URN FeatureUrn => new("rpgc", FeatureUrnModule, "standard_control_feature");

    private int _controlledEntityRuntimeId = -1;
    private bool _controlledEntityHasMovement;
    private IEcsWorld? _currentEcsWorld;
    
    public override void OnSetup()
    {
        RuntimeServices.OnceServiceReady((IPlayerController pc) =>
        {
            pc.PropertyChanged += OnPlayerControllerPropertyChanged;
        });
        RuntimeServices.OnceServiceReady((IGameSession gs) =>
        {
            gs.EcsWorldChanged += OnEcsWorldChanged;
        });

        EngineServices.OnceServiceReady((IInputsService IS) =>
        {
            IS.RegisterAction("forward", HandleMovementForward, true);
            IS.RegisterAction("backward", HandleMovementBackward, true);
            IS.RegisterAction("left", HandleMovementLeft, true);
            IS.RegisterAction("right", HandleMovementRight, true);
            IS.SetAxisBinding("horizontal",  KeyboardKeys.D, KeyboardKeys.Q);
            IS.SetAxisBinding("vertical", KeyboardKeys.S, KeyboardKeys.Z);
        });
    }

    private void OnEcsWorldChanged(IEcsWorld? obj)
    {
        obj.SystemManager.AddSystem(new StandardControlSystem());
    }

    private bool CheckEntityHasMovement()
    {
        if (_currentEcsWorld == null)
            return false;

        return _currentEcsWorld.ComponentManager.HasComponent<MovementComponent>(_controlledEntityRuntimeId);
    }
    
    private void OnPlayerControllerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RuntimeServices.PlayerController.PossessedEntityId)) return;
        
        var controlledEntity = RuntimeServices.PlayerController.PossessedEntityId;
        _controlledEntityRuntimeId = controlledEntity;
        
        _controlledEntityHasMovement = CheckEntityHasMovement();
    }
    
    private ref MovementComponent GetMovementComponent()
    {
        if (_currentEcsWorld == null || _controlledEntityRuntimeId == -1 || !_controlledEntityHasMovement)
            return ref Unsafe.NullRef<MovementComponent>();

        return ref _currentEcsWorld.ComponentManager.GetComponent<MovementComponent>(_controlledEntityRuntimeId);
    }

    private void HandleMovementForward()
    {
        HandleMovement(y: 1);
    }
    
    private void HandleMovementBackward()
    {
        HandleMovement(y: -1);
    }

    private void HandleMovementLeft()
    {
        HandleMovement(x: -1);
    }
    
    private void HandleMovementRight()
    {
        HandleMovement(x: 1);
    }

    private void HandleMovement(float? x = null, float? y = null)
    {
        ref var moveComp = ref GetMovementComponent();

        if (Unsafe.IsNullRef(ref moveComp))
            return;
        if(x.HasValue)
            moveComp.Direction.X = x.Value;
        if(y.HasValue)
            moveComp.Direction.Y = y.Value;
    }

    public override void OnStopGame()
    {
        _controlledEntityRuntimeId = -1;
        _controlledEntityHasMovement = false;

        RuntimeServices.OnceServiceReady((IPlayerController pc) =>
        {
            pc.PropertyChanged -= OnPlayerControllerPropertyChanged;
        });
        RuntimeServices.OnceServiceReady((IGameSession gs) =>
        {
            gs.EcsWorldChanged -= OnEcsWorldChanged;
        });
        RuntimeServices.OnceServiceReady((IInputsService IS) =>
        {
            IS.UnregisterAction("forward");
            IS.UnregisterAction("backward");
            IS.UnregisterAction("left");
            IS.UnregisterAction("right");
        });
    }
}

public class StandardControlSystem : ISystem
{
    public override int Priority => 200;
    public override bool IsDrawingSystem => false;
    
    ComponentManager _componentManager;
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
    }

    public override void Update(TimeSpan deltaTime)
    {
        float x = EngineServices.InputsService.GetAxis("horizontal");
        float y = EngineServices.InputsService.GetAxis("vertical");
        
        foreach (var entityId in _componentManager.Query<MovementComponent, PlayerTagComponent>())
        {
            ref var movementComponent = ref _componentManager.GetComponent<MovementComponent>(entityId);
            movementComponent.Direction.X = x;
            movementComponent.Direction.Y = y;
        }
    }
}
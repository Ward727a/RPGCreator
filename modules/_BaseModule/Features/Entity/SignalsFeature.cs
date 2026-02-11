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

using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Entity;

/// <summary>
/// The signal system should not be confused with the EventBus.<br/>
/// The signal system is for "local" event, thing that happened to a specific entity, and that other entities can react to.<br/>
/// The EventBus is for "global" events, that are not tied to a specific entity, can be listened by everything.
/// </summary>
public class SignalsFeature : BaseEntityFeature
{
    public override string FeatureName => "Signals";
    public override string FeatureDescription => "Allows the entity to emit and react to signals. Signals are a powerful way to create interactions between entities without tight coupling. They can be used for a wide variety of purposes, such as triggering events, communicating between entities, and creating complex behaviors.";
    public override URN FeatureUrn { get; } = FeatureUrnModule.ToUrnModule("rpgc").ToUrn("signals");

    public override void OnWorldSetup(IEcsWorld world)
    {
        world.SystemManager.AddSystem(new SimpleEventSystem());
        world.SystemManager.AddSystem(new SignalsSystem());
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new SimpleEventStorageComponent());
        entity.AddComponent(new SignalsComponent());
    }
}


public struct SignalsComponent : IComponent
{
    /// <summary>
    /// Signals that are currently waiting to be processed.<br/>
    /// Those will be used and cleared at the end of the current frame, after ALL systems have been executed by the SignalsSystem.
    /// </summary>
    public Bitmask256 PendingSignals;
    
    public void EmitSignal(int signalIndex)
    {
        PendingSignals.Set(signalIndex, true);
    }

    public void EmitSignal(URN signalUrn)
    {
        PendingSignals.Set(RegistryServices.SignalRegistry.GetSignalMask(signalUrn), true);
    }
}

public struct SimpleEventStorageComponent() : IComponent
{
    public List<SimpleEventRuntimeInstance> AttachedEvents = new List<SimpleEventRuntimeInstance>();
    public CustomData LocalVariables { get; set; } = new CustomData();
}

public struct SimpleEventRuntimeInstance
{
    public Ulid AssetId;
    public Bitmask256 InterestsMask;
}

public class SimpleEventSystem : ISystem
{
    public override int Priority => 2147482000; // Should be executed at the very end of the frame, after all other systems have been executed, but before the SignalsSystem, to ensure that all signals emitted during the frame are processed in the same frame.
    public override bool IsDrawingSystem => false;
    
    private ComponentManager _componentManager = null!;
    private ISimpleEventExecutor _executor => RuntimeServices.SimpleEventExecutor;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
    }

    public override void Update(TimeSpan deltaTime)
    {
        foreach (var entityId in _componentManager.QueryDirty<SignalsComponent>())
        {
            if (!_componentManager.HasComponent<SimpleEventStorageComponent>(entityId)) continue;
            
            ref var signals = ref _componentManager.GetComponent<SignalsComponent>(entityId);
            ref var storage = ref _componentManager.GetComponent<SimpleEventStorageComponent>(entityId);

            foreach (var events in storage.AttachedEvents)
            {
                if (signals.PendingSignals.HasAny(events.InterestsMask))
                {
                    if (storage.LocalVariables == null)
                    {
                        storage.LocalVariables = new CustomData().Set("entityId", entityId);
                    }

                    _executor.Execute(events.AssetId, storage.LocalVariables);
                }
            }
        }
    }
}

public class SignalsSystem : ISystem
{
    public override int Priority => 2147483000; // Should be executed at the very end of the frame, after all other systems have been executed, to ensure that all signals emitted during the frame are processed.
    public override bool IsDrawingSystem => false;
    private ComponentManager _componentManager = null!;
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
    }

    public override void Update(TimeSpan deltaTime)
    {
        foreach (var entityId in _componentManager.QueryDirty<SignalsComponent>())
        {
            ref var signalsComponent = ref _componentManager.GetComponent<SignalsComponent>(entityId);
            
            signalsComponent.PendingSignals.Clear();
        }
        _componentManager.ClearDirty<SignalsComponent>();
    }
    
    public void EmitSignal(int entityId, int signalIndex)
    {
        if (!_componentManager.HasComponent<SignalsComponent>(entityId)) return;
        
        ref var signalsComponent = ref _componentManager.GetComponent<SignalsComponent>(entityId);
        signalsComponent.EmitSignal(signalIndex);
        
    }
}
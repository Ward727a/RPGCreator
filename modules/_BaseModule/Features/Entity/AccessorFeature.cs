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
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Entity;

public interface IAccessor
{
    public bool IsBinded { get; }
    public IEcsWorld BindedToWorld { get; }
    public int EntityId { get; set; }
    public void Update(IEcsWorld world);
}

[EntityFeature]
public class AccessorFeature : BaseEntityFeature
{
    public static UrnNamespace AccessorNamespace = "rpgc_runtime".ToUrnNamespace();
    public override string FeatureName => "Accessor Feature";
    public override string FeatureDescription => "A feature that provides access to certain properties of an entity.";
    public override URN FeatureUrn => FeatureUrnModule.ToUrnModule("rpgc").ToUrn("accessor");

    public override void OnWorldSetup(IEcsWorld world)
    {
        world.SystemManager.AddSystem(new AccessorSystem());
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new AccessorComponent());
    }
}

public struct AccessorComponent : IComponent
{
    public Bitmask256 InterestedSignals { get; set; }
    
    /// <summary>
    /// int => Signal urn of the property or method being accessed.
    /// IAccessor => The accessor instance that provides access to the property or method.
    /// </summary>
    public Dictionary<int, List<IAccessor>>? ActiveBindings { get; set; }

    public void RefreshSignal(int signalIdx, IEcsWorld world)
    {
        if (ActiveBindings == null) return;

        if (!ActiveBindings.TryGetValue(signalIdx, out var activeAccessors)) return;
        
        foreach (var accessor in activeAccessors)
        {
            accessor.Update(world);
        }
    }
    
    public void AddBinding(int signalIdx, IAccessor accessor)
    {
        if (ActiveBindings == null)
        {
            ActiveBindings = new Dictionary<int, List<IAccessor>>();
        }

        if (!ActiveBindings.TryGetValue(signalIdx, out var accessors))
        {
            accessors = new List<IAccessor>();
            ActiveBindings[signalIdx] = accessors;
        }

        accessors.Add(accessor);
    }
}

public class AccessorSystem : ISystem
{
    public override int Priority => 2147481000;
    public override bool IsDrawingSystem => false;
    private IEcsWorld? World => RuntimeServices.GameSession.ActiveEcsWorld;
    private ComponentManager _componentManager = null!;
    
    private readonly Dictionary<int /*signalIdx*/, Bitmask256 /*signalBitmask*/> _signalBitmasks = new();
    
    private readonly Queue<(int entityId, int signalIdx, IAccessor accessor)> _addQueue = new();
    private readonly Queue<(int entityId, int signalIdx, IAccessor accessor)> _removeQueue = new();
    private readonly Queue<(int entityId, int signalIdx)> _clearQueue = new();
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
    }

    public override void Update(TimeSpan deltaTime)
    {
        
        while (_addQueue.TryDequeue(out var addItem))
        {
            if (!_componentManager.HasComponent<AccessorComponent>(addItem.entityId))
            {
                _componentManager.AddComponent(addItem.entityId, new AccessorComponent());
            }

            ref var accessorComp = ref _componentManager.GetComponent<AccessorComponent>(addItem.entityId);
            addItem.accessor.EntityId = addItem.entityId;
            accessorComp.AddBinding(addItem.signalIdx, addItem.accessor);
            var bitmask = accessorComp.InterestedSignals;
            bitmask.Set(addItem.signalIdx, true);
            accessorComp.InterestedSignals = bitmask;

            if (!_signalBitmasks.ContainsKey(addItem.signalIdx))
            {
                var signalMask = new Bitmask256();
                signalMask.Set(addItem.signalIdx, true);
                _signalBitmasks[addItem.signalIdx] = signalMask;
            }
        }

        while (_removeQueue.TryDequeue(out var removeItem))
        {
            if (!_componentManager.HasComponent<AccessorComponent>(removeItem.entityId)) continue;
            
            ref var accessorComp = ref _componentManager.GetComponent<AccessorComponent>(removeItem.entityId);
            if (accessorComp.ActiveBindings == null) continue;
            if (!accessorComp.ActiveBindings.TryGetValue(removeItem.signalIdx, out var accessors)) continue;
            accessors.Remove(removeItem.accessor);
            if(accessorComp.ActiveBindings[removeItem.signalIdx].Count == 0)
            {
                accessorComp.InterestedSignals.Set(removeItem.signalIdx, false);
            }
        }
        
        while (_clearQueue.TryDequeue(out var clearItem))
        {
            if (!_componentManager.HasComponent<AccessorComponent>(clearItem.entityId)) continue;
            
            ref var accessorComp = ref _componentManager.GetComponent<AccessorComponent>(clearItem.entityId);
            if (accessorComp.ActiveBindings == null) continue;
            accessorComp.ActiveBindings.Remove(clearItem.signalIdx);
            accessorComp.InterestedSignals.Set(clearItem.signalIdx, false);
        }
        
        foreach (var entityId in _componentManager.QueryDirty<SignalsComponent>())
        {
            if (!_componentManager.HasComponent<AccessorComponent>(entityId)) continue;
            
            ref var signalComp = ref _componentManager.GetComponent<SignalsComponent>(entityId);
            ref var accessorComp = ref _componentManager.GetComponent<AccessorComponent>(entityId);

            if (accessorComp.ActiveBindings == null) continue;
            if (!signalComp.PendingSignals.HasAny(accessorComp.InterestedSignals)) continue;
            
            foreach (var signalIdx in accessorComp.ActiveBindings.Keys)
            {
                if(!_signalBitmasks.TryGetValue(signalIdx, out var bitmask)) continue;

                if (signalComp.PendingSignals.HasAny(bitmask))
                {
                    RefreshAccessors(entityId, signalIdx);
                }
            }
        }
    }
    
    public void RefreshAccessors(int entityId, int signalIdx)
    {
        if(World == null) return;
        if (!_componentManager.HasComponent<AccessorComponent>(entityId)) return;
        
        var accessorComp = _componentManager.GetComponent<AccessorComponent>(entityId);
        accessorComp.RefreshSignal(signalIdx, World);
    }
    
    public void AddAccessorBinding(int entityId, int signalIdx, IAccessor accessor)
    {
        _addQueue.Enqueue((entityId, signalIdx, accessor));
    }
    
    public void ClearAccessorBindings(int entityId, int signalIdx)
    {
        _clearQueue.Enqueue((entityId, signalIdx));
    }
    
    public void RemoveAccessorBinding(int entityId, int signalIdx, IAccessor accessor)
    {
        _removeQueue.Enqueue((entityId, signalIdx, accessor));
    }
}
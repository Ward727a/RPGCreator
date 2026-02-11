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

using _BaseModule.Features.Entity;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace _BaseModule.ECS_Accessor;

public partial class StatObservableAccessor : BaseObservableAccessor
{
    public sealed override int EntityId { get; set; }
    private ComponentManager ComponentManager;
    
    public int StatIdx { get; set; }

    [ObservableProperty] private double _statActualValue = -1;
    [ObservableProperty] private double _statMaxValue = -1;
    [ObservableProperty] private double _statBaseValue = -1;
    [ObservableProperty] private double _statMinValue = -1;

    public StatObservableAccessor(int entityId, IEcsWorld world, URN statUrn)
    {
        EntityId = entityId;
        ComponentManager = world.ComponentManager;
        world.SystemManager.GetSystem<StatSystem>(out var system);
        if (system == null)
        {
            StatIdx = -1;
            Logger.Warning("StatSystem not found in world. StatObservableAccessor will not function properly.");
            return;
        }
        Logger.Debug("Binding StatObservableAccessor to entity {0} and stat {1}", args: [entityId, statUrn]);
        StatIdx = system.GetStatIndex(statUrn);
    }
    
    public override void Update(IEcsWorld world)
    {
        if(StatIdx == -1) return;
        if(ComponentManager == null || world.ComponentManager != ComponentManager)
            ComponentManager = world.ComponentManager;

        if(!ComponentManager.HasComponent<StatComponent>(EntityId)) return;

        ref var statComponent = ref ComponentManager.GetComponent<StatComponent>(EntityId);
        
        if (StatIdx < 0 || StatIdx >= statComponent.Stats.Length)
            return;

        ref var stat = ref statComponent.Stats[StatIdx];
        
        StatActualValue = stat.ActualValue;
        StatMaxValue = stat.FinalValue;
        StatBaseValue = stat.BaseValue;
        StatMinValue = stat.MinValue;
        
        Logger.Debug("Updated StatObservableAccessor for entity {0} and stat index {1}: ActualValue={2}, MaxValue={3}, BaseValue={4}, MinValue={5}",
            args: [EntityId, StatIdx, StatActualValue, StatMaxValue, StatBaseValue, StatMinValue]);
    }

    public override void BindToEntity(IEcsWorld world)
    {
        if (IsBinded) return;
        world.SystemManager.GetSystem<AccessorSystem>(out var system);

        if (system == null) return;
        
        var signalIdx = RegistryServices.SignalRegistry.GetSignalMask(ISignalRegistry.SignalModuleUrn.ToUrnModule("rpgc").ToUrn("stat_changed"));
        
        system.AddAccessorBinding(EntityId, signalIdx, this);
        IsBinded = true;
        BindedToWorld = world;
        Logger.Debug("Bound StatObservableAccessor to entity {0} with signal index {1}", args: [EntityId, signalIdx]);
    }
}
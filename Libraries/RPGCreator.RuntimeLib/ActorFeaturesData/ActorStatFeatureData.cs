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

[TypeDiscriminator("rpgc", "sdk", "actor_features", "stats")]
[ActorFeature("Stats", "Combat & RPG", "Gives the actor base attributes and resources (HP, MP, Strength, etc.).")]
public class ActorStatsFeatureData : IActorFeatureData
{
    public Dictionary<Ulid, double> BaseStats { get; set; } = [];

    public void ApplyToEntity(World world, Entity entity)
    {
        var runtimeStats = new Dictionary<Ulid, RuntimeStatValue>(BaseStats.Count);
        foreach (var (statId, value) in BaseStats)
        {
            runtimeStats[statId] = new RuntimeStatValue
            {
                BaseValue = value,
                CurrentValue = value,
                BonusFlat = 0,
                BonusPercent = 0
            };
        }

        world.Add(entity, new ActorStatsComponent
        {
            Stats = runtimeStats,
            IsDirty = true
        });
    }

    public void CollectDependencies(in List<Ulid> dependencies)
    {
        dependencies.AddRange(BaseStats.Keys);
    }
}
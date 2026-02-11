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

using System.Globalization;
using _BaseModule.ECS_Accessor;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Entity;

/// <summary>
/// Tags the entity as a player-controlled entity.<br/>
/// Note: This feature does not implement any control logic by itself; it simply serves as a marker for other systems to identify player characters.<br/>
/// Note2: The added component is empty, and will be optimized by the ecs to use as little memory as possible.
/// </summary>
[EntityFeature]
public class PlayerTagFeature : BaseEntityFeature
{
    public static URN Urn = FeatureUrnModule.ToUrnModule("rpgc").ToUrn("player_tag_feature");
    
    public override string FeatureName => "Player Tag";
    public override string FeatureDescription => "Tags the entity as a player-controlled entity.\n" +
                                              "Useful for identifying player characters in various systems (this does nothing by itself).";
    public override URN FeatureUrn => Urn;
    
    public bool ShouldBePlayerControlled
    {
        get => GetConfig(true);
        set => SetConfig(value);
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        if (!ShouldBePlayerControlled) return;
        entity.AddComponent(new PlayerTagComponent());
        entity.ExecuteOnceCreated((int entityId) =>
        {
            RuntimeServices.CameraService.LinkToEntity(entityId);
            RuntimeServices.GameSession.CurrentPlayerId = entityId;
            Logger.Debug("PlayerTagFeature injected on entity {0}. Camera linked and player ID set.", args: [entityId]);
            var HealthBinding = new StatObservableAccessor(
                RuntimeServices.GameSession.CurrentPlayerId,
                RuntimeServices.GameSession.ActiveEcsWorld,
                "rpgc".ToUrnNamespace().ToUrnModule("stat").ToUrn("Health"));

            HealthBinding.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(StatObservableAccessor.StatActualValue) ||
                    args.PropertyName == nameof(StatObservableAccessor.StatMaxValue))
                {
                    Logger.Debug("Health stat changed: ActualValue={ActualValue}, MaxValue={MaxValue}", args:
                    [
                        HealthBinding.StatActualValue.ToString(CultureInfo.InvariantCulture),
                        HealthBinding.StatMaxValue.ToString(CultureInfo.InvariantCulture)
                    ]);
                }
            };
                
            Logger.Debug("Health stat binding made: ActualValue={ActualValue}, MaxValue={MaxValue}", args:
            [
                HealthBinding.StatActualValue.ToString(CultureInfo.InvariantCulture),
                HealthBinding.StatMaxValue.ToString(CultureInfo.InvariantCulture)
            ]);
            HealthBinding.BindToEntity(RuntimeServices.GameSession.ActiveEcsWorld);
        });
    }
}

public struct PlayerTagComponent : IComponent
{
}
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

using System.Numerics;
using _BaseModule.Enums;
using _BaseModule.Features.Entity;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Features;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;
using Size = RPGCreator.SDK.Types.Size;

namespace _BaseModule.MacroFeatures;

public enum CollisionOriginType
{
    [Description("Above the head", "The collision will be just above the entity's head.")]
    AboveHead,
    [Description("At the head", "The collision will be at the entity's head.")]
    AtHead,
    [Description("At the center", "The collision will be at the entity's center.")]
    AtCenter,
    [Description("At the foot", "The collision will be at the entity's foot.")]
    AtFoot,
    [Description("Below the foot", "The collision will be just below the entity's foot.")]
    BelowFoot,
}

[EntityMacroFeature(MaxInstancesPerCharacter = 1)]
public class LivingBeingMacroFeature : BaseMacroEntityFeature
{
    public static URN Urn = new("rpgc", MacroFeatureUrnModule, "living_being");
    public override string FeatureName => "Living Being";
    public override string FeatureDescription => "Defines the entity as a living being, enabling health, stamina, and other vital systems.";
    public override URN FeatureUrn => new URN("rpgc", MacroFeatureUrnModule, "living_being");

    [EntityFeatureProperty("Movement Type", "Defines the type of movement allowed for the entity.\n" +
                                            "Property is global with all entity.", IsShared = true, Category = "Movement")]
    public MovementType MovementType
    {
        get => GetSubSharedConfigValue(MovementFeature.Urn, MovementType.FourDir);
        set => SetSubSharedConfigValue(MovementFeature.Urn, value);
    }

    [EntityFeatureProperty("Player Controlled", "If enabled, this entity can be controlled by the player.", Category = "Control")]
    public bool ShouldBePlayerControlled
    {
        get => GetSubConfigValue(PlayerTagFeature.Urn, true);
        set => SetSubConfigValue(PlayerTagFeature.Urn, value);
    }

    [EntityFeatureProperty("Speed", "Defines the movement speed of the entity.", MinValue = 0, Category = "Movement")]
    public int Speed
    {
        get => GetSubConfigValue(MovementFeature.Urn, 5);
        set => SetSubConfigValue(MovementFeature.Urn, value);
    }

    [EntityFeatureProperty("Size", "The size of the entity.", Category = "Appearance", MinValue = 1)]
    public Size Size
    {
        get => GetSubConfigValue(BoundsFeature.Urn, new Size(32, 32));
        set => SetSubConfigValue(BoundsFeature.Urn, value);
    }
    
    [EntityFeatureProperty("Collision Type", "Defines the type of collision for the entity.", Category = "Collision")]
    public EColliderType CollisionShapeType
    {
        get => GetSubConfigValue(CollisionFeature.Urn, EColliderType.Square);
        set => SetSubConfigValue(CollisionFeature.Urn, value);
    }
    
    [EntityFeatureProperty("Collision Size", "Defines the size of the collision box.", Category = "Collision")]
    public Size CollisionSize
    {
        get => GetSubConfigValue(CollisionFeature.Urn, Size);
        set => SetSubConfigValue(CollisionFeature.Urn, value);
    }

    [EntityFeatureProperty("Collision Origin", "Defines the origin of the collision box.", Category = "Collision")]
    public CollisionOriginType CollisionOriginType
    {
        get => GetConfig(CollisionOriginType.AtCenter);
        set => SetConfig(value);
    }
    
    public override void OnSetup()
    {
        var fm = EngineServices.FeaturesManager;
        
        fm.OnceEntityFeaturesRegistered(SpriteFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(BoundsFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(CollisionFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(MovementFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(AnimationFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(PlayerTagFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(AccessorFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(StatsFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(StatsModifierFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(SignalsFeature.Urn, RegisterSubFeature);
    }

    public override void OnAddedToUi(IEntityDefinition definition, object itemCtx)
    {
        base.OnAddedToUi(definition, itemCtx);
        // We update the collision origin type here to make sure it's set correctly for the current entity.
        UpdateCollisionOriginType();
        
        Configuration.DataChanged += ConfigurationChanged;
        GetRequiredFeatureData(CollisionFeature.Urn).DataChanged += ConfigurationChanged;
        GetRequiredFeatureData(BoundsFeature.Urn).DataChanged += ConfigurationChanged;
    }
    
    public override void OnRemovedFromUi(IEntityDefinition definition, object itemCtx)
    {
        base.OnRemovedFromUi(definition, itemCtx);
        Configuration.DataChanged -= ConfigurationChanged;
        GetRequiredFeatureData(CollisionFeature.Urn).DataChanged -= ConfigurationChanged;
        GetRequiredFeatureData(BoundsFeature.Urn).DataChanged -= ConfigurationChanged;
    }

    private void ConfigurationChanged(string propName)
    {
        // If any of these 3 variables change, we need to update the collision origin.
        if (propName is nameof(CollisionOriginType) or nameof(CollisionFeature.CollisionSize) or nameof(BoundsFeature.Size))
        {
            UpdateCollisionOriginType();
        }
    }

    /// <summary>
    /// Update the collision origin based on the selected type.<br/>
    /// It should update the collision box position found in the collision feature.
    /// </summary>
    private void UpdateCollisionOriginType()
    {
        var entitySize = Size;
        var collisionSize = CollisionSize;

        void SetCollisionOriginPosition(float x, float y)
        {
            var offset = new Vector2(x, y);
            SetSubConfigValue(CollisionFeature.Urn, offset, nameof(CollisionFeature.CollisionOrigin));
        }
        
        switch (CollisionOriginType)
        {
            case CollisionOriginType.AboveHead:
                SetCollisionOriginPosition(0, -CollisionSize.Height);
                break;
            case CollisionOriginType.AtHead:
                SetCollisionOriginPosition(0, 0);
                break;
            case CollisionOriginType.AtCenter:
                SetCollisionOriginPosition(0, (entitySize.CenterHeight) - (collisionSize.CenterHeight));
                break;
            case CollisionOriginType.AtFoot:
                SetCollisionOriginPosition(0, entitySize.Height - collisionSize.Height);
                break;
            case CollisionOriginType.BelowFoot:
                SetCollisionOriginPosition(0, entitySize.Height);
                break;
        }
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        // Convert this to use BlobManager later.
        entity.AddComponent(new CharStateComponent
        {
            AnimationsMapping = entityDefinition.AnimationsMapping
        });
    }

}
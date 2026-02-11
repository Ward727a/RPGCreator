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
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.MacroFeatures;

[EntityMacroFeature(MaxInstancesPerCharacter = 1)]
public class LivingBeingMacroFeature : BaseMacroEntityFeature
{
    public override string FeatureName => "Living Being";
    public override string FeatureDescription => "Defines the entity as a living being, enabling health, stamina, and other vital systems.";
    public override URN FeatureUrn => new URN("rpgc", MacroFeatureUrnModule, "living_being");

    [EntityFeatureProperty("Movement Type", "Defines the type of movement allowed for the entity.\n" +
                                            "Property is global with all other same feature.", IsShared = true, Category = "Movement")]
    public MovementType MovementType
    {
        get => GetSharedConfigurationValue(MovementFeature.Urn, MovementType.FourDir);
        set => SetSharedConfigurationValue(MovementFeature.Urn, value);
    }
    

    [EntityFeatureProperty("Player Controlled", "If enabled, this entity can be controlled by the player.", Category = "Control")]
    public bool ShouldBePlayerControlled
    {
        get => GetConfigurationValue(PlayerTagFeature.Urn, true);
        set => GetConfigurationValue(PlayerTagFeature.Urn, value);
    }


    [EntityFeatureProperty("Speed", "Defines the movement speed of the entity.", MinValue = 0, Category = "Movement")]
    public int Speed
    {
        get => GetConfigurationValue(MovementFeature.Urn, 5);
        set => SetConfigurationValue(MovementFeature.Urn, value);
    }
    
    public override void OnSetup()
    {
        var fm = EngineServices.FeaturesManager;
        
        fm.OnceEntityFeaturesRegistered(MovementFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(AnimationFeature.Urn, RegisterSubFeature);
        fm.OnceEntityFeaturesRegistered(PlayerTagFeature.Urn, RegisterSubFeature);
        // More to come...
    }
}
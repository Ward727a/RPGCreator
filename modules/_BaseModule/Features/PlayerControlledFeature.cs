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

using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features;

[EntityFeature(MaxInstancesPerCharacter = 1)]
public class PlayerControlledFeature : BaseEntityFeature
{
    public override string FeatureName => "Player Controlled Feature";
    public override string FeatureDescription => "Allows the entity to be controlled by the player.";
    public override URN FeatureUrn => new("rpgc", FeatureUrnModule, "PlayerControlledFeature");

    public Ulid FeatureRuntimeId { get; private set; } = Ulid.Empty;
    
    // Here we do not mark it as "IsShared" because we do the sync manually.
    [EntityFeatureProperty(
        "Is Controlled By Player",
        "Determines whether the entity is controlled by the player by default.")]
    public bool IsControlledByDefault
    {
        get => GetConfig(false);
        set
        {
            SetConfig(value);
            if(value && FeatureRuntimeId != Ulid.Empty)
                SetShared(FeatureRuntimeId, "ControlledByPlayer");
        }
    }
    
    public override void OnSetup()
    {
    
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
    }

    public override void OnInject(BufferedEntity entity)
    {
    }

    public override void OnDestroy(BufferedEntity entity)
    {
    }

    public override IEntityFeature Clone()
    {
        var clone = base.Clone();
        if(clone is PlayerControlledFeature pcFeature)
        {
            pcFeature.FeatureRuntimeId = Ulid.NewUlid();
            pcFeature.SetupEvent();
        }
        return clone;
    }
    
    private void SetupEvent()
    {
        SharedMemoryConfiguration.OnDataChanged += (SharedMemoryConfigurationOnOnDataChanged);
    }

    public override void Dispose()
    {
        SharedMemoryConfiguration.OnDataChanged -= SharedMemoryConfigurationOnOnDataChanged;
        GC.SuppressFinalize(this);
    }

    public override void Reset()
    {
        FeatureRuntimeId = Ulid.NewUlid();
    }

    #region EventsHandler

    private void SharedMemoryConfigurationOnOnDataChanged(string key)
    {
        if (key != "ControlledByPlayer") return;
        var isControlled = Ulid.Parse(GetShared("", key)) == FeatureRuntimeId;
        SetConfig(isControlled, nameof(IsControlledByDefault));
    }
    
    #endregion
}
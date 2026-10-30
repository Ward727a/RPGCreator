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

using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.ActorFeatures;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Registry;

[EngineService(typeof(IActorFeaturesRegistry))]
public class ActorFeaturesRegistry(ITypesRegistry typesRegistry, IServiceProvider serviceProvider) : IActorFeaturesRegistry
{
    public IEnumerable<(Type featureType, ActorFeatureAttribute attribute)> Features => FeatureRegistry.Values;
    
    public Dictionary<URN, (Type featureType, ActorFeatureAttribute attribute)> FeatureRegistry { get; } = new();
    
    public void RegisterFeature(Type featureType, ActorFeatureAttribute attribute, bool overrideIfExist = false)
    {
        if (typesRegistry.HasType(featureType))
        {
            typesRegistry.GetKey(featureType).OnSuccess((urn) =>
            {
                if(FeatureRegistry.ContainsKey(urn) && !overrideIfExist)
                {
                    Logger.Error("Feature {urn} already registered", urn);
                    return;
                }
                FeatureRegistry[urn] = (featureType, attribute);
            }).OnFailure(err =>
            {
                Logger.Error("Couldn't register feature {type} due to an error while trying to get is URN discriminator: {err}", featureType, err);
            });
        }
        else
        {
            Logger.Error("Couldn't register feature {type} because it doesn't appear to be registered inside the types registry.", featureType);
        }
    }

    public void UnregisterFeature(URN featureUrn)
    {
        if(FeatureRegistry.Remove(featureUrn))
            Logger.Info("Feature {urn} unregistered", featureUrn);
        else
            Logger.Error("Couldn't unregister feature {urn} because it doesn't appear to be registered.", featureUrn);
    }

    public (Type featureType, ActorFeatureAttribute attribute)? GetFeature(URN featureUrn)
    {
        if(FeatureRegistry.TryGetValue(featureUrn, out var feature))
            return feature;
        Logger.Error("Couldn't get feature {urn} because it doesn't appear to be registered.", featureUrn);
        return null;
    }
    
    public bool IsFeatureRegistered(URN featureUrn) => FeatureRegistry.ContainsKey(featureUrn);
    
    public IActorFeatureData? CreateFeatureInstance(URN featureUrn)
    {
        var feature = GetFeature(featureUrn);
        if (feature == null)
            return null;

        return ActivatorUtilities.CreateInstance(serviceProvider, feature.Value.featureType) as IActorFeatureData;
    }
}
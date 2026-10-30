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

using RPGCreator.SDK.Assets.Actors;

namespace RPGCreator.SDK.ActorFeatures;

public interface IActorFeatureDrawer
{
    /// <summary>
    /// Draws the ImGui inspector for the given feature.
    /// Returns true if any property of the feature was modified (to mark the editor as dirty).
    /// </summary>
    bool Draw(IActorFeatureData feature);
}

public interface IActorFeatureDrawer<in TFeature> : IActorFeatureDrawer
    where TFeature : class, IActorFeatureData
{
    bool Draw(TFeature feature);

    bool IActorFeatureDrawer.Draw(IActorFeatureData feature) => Draw((TFeature)feature);
}

public interface IActorFeatureDrawerRegistry
{
    void RegisterDrawer(Type featureType, Type drawerType);
    void UnregisterDrawer(Type featureType);

    /// <summary>
    /// Draws the feature using its registered custom drawer, or falls back to the generic reflection drawer if none is registered.
    /// Returns true if any value was modified.
    /// </summary>
    bool DrawFeature(IActorFeatureData feature);
}
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

using RPGCreator.SDK.Services.Assets;

namespace RPGCreator.SDK.Assets.Actors;

public class DirectionalAnimationSet
{
    /// <summary>
    /// Maps each direction to a SpriteAnimationDefinition AssetId.
    /// For non-directional props (like a chest or a tree), only ActorDirection.Down needs to be set.
    /// </summary>
    [AssetReference]
    public Dictionary<ActorDirection, Ulid> Clips { get; init; } = [];

    /// <summary>
    /// Resolves the SpriteAnimationDefinition AssetId for the requested direction.
    /// Automatically handles Left/Right horizontal mirroring and single-direction fallback (Down).
    /// </summary>
    public (Ulid AnimationAssetId, bool FlipHorizontally) Resolve(ActorDirection direction)
    {
        if (Clips.TryGetValue(direction, out var id) && id != Ulid.Empty)
            return (id, false);

        // Automatic Left <-> Right mirror fallback
        if (direction == ActorDirection.Right && Clips.TryGetValue(ActorDirection.Left, out id) && id != Ulid.Empty)
            return (id, true);

        if (direction == ActorDirection.Left && Clips.TryGetValue(ActorDirection.Right, out id) && id != Ulid.Empty)
            return (id, true);

        // Fallback to Down for 1-direction animations (e.g. chests, death, props)
        if (Clips.TryGetValue(ActorDirection.Down, out id) && id != Ulid.Empty)
            return (id, false);

        return (Ulid.Empty, false);
    }
}
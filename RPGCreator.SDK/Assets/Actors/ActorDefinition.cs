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
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Actors;


[TypeDiscriminator("rpgc", "sdk", "definitions", "actor")]
[AssetType("actors", "ractor")]
public class ActorDefinition : IAssetDto
{
    public Ulid AssetId { get; init; } = Ulid.NewUlid();
    public List<Ulid> Dependencies { get; init; } = [];

    /// <summary>
    /// Display name of the actor (e.g. "Hero", "Goblin Scout", "Wooden Chest").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category used to group and filter actors in the Editor UI.
    /// </summary>
    public ActorCategory Category { get; set; } = ActorCategory.Character;

    /// <summary>
    /// Icon or Portrait image resource ID (used in the Asset Manager, dialogues, or UI).
    /// </summary>
    public Ulid IconResource { get; set; } = Ulid.Empty;

    /// <summary>
    /// Maps an Animation Slot URN (e.g. "rpgc://anim_slots/idle", "rpgc://anim_slots/walk", "rpgc://anim_slots/open")
    /// to its directional animation clips.
    /// </summary>
    [AssetContainer]
    public Dictionary<URN, DirectionalAnimationSet> Animations { get; set; } = [];

    /// <summary>
    /// Modular features defining what this actor has and can do
    /// (e.g. StatsFeature, FactionFeature, MovementFeature, PlayerControllerFeature, ChestFeature).
    /// </summary>
    public List<IActorFeatureData> Features { get; set; } = [];

    /// <summary>
    /// Attempts to retrieve a feature of type <typeparamref name="T"/> attached to this actor.
    /// </summary>
    public bool TryGetFeature<T>(out T? feature) where T : class, IActorFeatureData
    {
        foreach (var item in Features)
        {
            if (item is T matched)
            {
                feature = matched;
                return true;
            }
        }

        feature = null;
        return false;
    }

    /// <summary>
    /// Returns true if a feature of type <typeparamref name="T"/> is attached to this actor.
    /// </summary>
    public bool HasFeature<T>() where T : class, IActorFeatureData
        => TryGetFeature<T>(out _);
}
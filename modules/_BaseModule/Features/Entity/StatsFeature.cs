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

using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Entity;

public class StatsFeature : BaseEntityFeature
{
    public static readonly URN StatsTag = new URN("rpgc", TagsUrnModule, "stats");
    
    public override string FeatureName => "Stats Feature";
    public override string FeatureDescription => "Adds basic stats to the entity, such as health, mana, and stamina.\n" +
                                                 "This will also enable a custom assets menu for stats management.";
    public override URN FeatureUrn => new URN("rpgc", FeatureUrnModule, "stats");

    private URN MakePath(string statName) => new URN("rpgc", "stats", statName);
    
    public override void OnSetup()
    {
        // First we register each path for each stat, if they don't already exist.
        EngineServices.OnceServiceReady((IGlobalPathData pathsData) =>
        {
            // If the paths for stats don't exist yet,
            // This either means that the feature is being set up for the very first time,
            // or that the paths have been deleted (which shouldn't happen, but just in case).
            if(!pathsData.HasTag(StatsTag))
            {
                pathsData.RegisterPaths(new List<(URN pathToValue, Ulid idValue)>([
                    (MakePath("health"), Ulid.NewUlid()),
                    (MakePath("mana"), Ulid.NewUlid()),
                    (MakePath("stamina"), Ulid.NewUlid())
                ]), StatsTag);
            }

        });
    }
}

public struct StatComponent : IComponent
{
    /// <summary>
    /// Reference to the stat definition, which contains the stat's name, description, icon, etc.
    /// </summary>
    public Ulid StatDefId;
    
    /// <summary>
    /// The current value of the stat. This can be modified by the game logic, such as when the entity takes damage or uses a skill that consumes mana.
    /// </summary>
    public double CurrentValue;
}
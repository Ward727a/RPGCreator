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


using System.Text.Json.Serialization;
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Definitions.Blueprints;


[EngineClass("rpgc", "scripts", "blueprints", "blueprint_def", DisplayName = "Blueprint Definition")]
public partial class BlueprintData() : BaseAssetDef, IHasTag
{
    [JsonIgnore]
    public bool IsDirty { get; private set; } = true;
    
    public StringName TagType = StringName.Empty;
    public Ulid Id => Unique;
    public List<ConnectionData> Connections { get; private set; } = new List<ConnectionData>();
    public List<NodeData> Nodes { get; private set; } = new List<NodeData>();
    public List<BlueprintParameters> Parameters { get; private set; } = new List<BlueprintParameters>();

    [JsonIgnore]
    public string FileCsName => $"{Id}.cs";
    
    [JsonIgnore]
    public string FileJsonName => $"{Name}_{Id}.json";
    
    public static BlueprintData Create()
    {
        return new BlueprintData()
        {
            Unique = Ulid.NewUlid(),
        };
    }

    public static BlueprintData Create(Ulid id)
    {
        return new BlueprintData()
        {
            Unique = id,
        };
    }

    /// <summary>
    /// Marks the blueprint as dirty.<br/>
    /// This notifies the <see cref="IBlueprintRegistry"/> that the blueprint has been modified, and should be recompiled.
    /// </summary>
    public void MarkDirty()
    {
        IsDirty = true;
    }
    
    /// <summary>
    /// Marks the blueprint as clean.<br/>
    /// <b>This should be used ONLY by the <see cref="IBlueprintRegistry"/> when compiling blueprints.</b><br/>
    /// Use it ONLY if you know what you're doing.
    /// </summary>
    public void MarkClean()
    {
        IsDirty = false;
    }

    public HashSet<StringName> Tags { get; } = [];

    public Result HasTag(StringName tag)
    {
        return Tags.Contains(tag) ? Result.Success() : Result.Failure($"The blueprint does not have the tag '{tag}'.");
    }
}

public interface IHasTag
{
    HashSet<StringName> Tags { get; }
    public Result HasTag(StringName tag)
    {
        return Tags.Contains(tag) ? Result.Success() : Result.Failure($"The object does not have the tag '{tag}'.");
    }

    public Result AddTag(StringName tag)
    {
        return Tags.Add(tag) ? Result.Success() : Result.Failure($"Failed to add tag '{tag}' to the object. Maybe it already exists?");
    }

    public Result RemoveTag(StringName tag)
    {
        return Tags.Remove(tag) ? Result.Success() : Result.Failure($"Failed to remove tag '{tag}' from the object. Maybe it doesn't exist?");
    }
}
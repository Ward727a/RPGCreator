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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.MetaData;

[EngineClass("rpgc", "metadata", "project", DisplayName = "Project Metadata")]
public partial class ProjectMetaData : BaseMetaData
{
    public readonly struct AssetsStruct()
    {
        public readonly string DbName { get; } = "AssetsManager.db";
        public readonly string RootFolder { get; } = "Content";
    }
    
    public override string DbKey => "metadata_project";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    [JsonPropertyName("Path")]
    public string JsonFilePath { get; set; } = "";
    [JsonIgnore]
    public string Directory => Path.GetDirectoryName(JsonFilePath) ?? "";
    public Version Version { get; set; } = new Version(0, 0, 1);
    public Version EditorVersion { get; set; } = RpgEnv.Versions.EngineVersion;
    public bool IsArchived { get; set; } = false;
    public bool IsFavorite { get; set; } = false;
    public string? Copyright { get; set; } = "";
    public List<string> Authors { get; set; } = [];
    public AssetsStruct Assets { get; set; } = new();
    public List<string> Modules { get; set; } = [];
    public Ulid MainMapId { get; set; } = Ulid.Empty;

    public ProjectMetaData()
    {
    }

    public ProjectMetaData(string name, string description, string jsonFilePath) : this()
    {
        Name = name;
        Description = description;
        JsonFilePath = jsonFilePath;
    }
    
    public static ProjectMetaData Create()
    {
        return new ProjectMetaData("New Project", "A new project", "")
        {
            Unique = Ulid.NewUlid()
        };
    }

    public static ProjectMetaData Create(string path)
    {
        return new ProjectMetaData("New Project", "A new project", path)
        {
            Unique = Ulid.NewUlid()
        };
    }
}
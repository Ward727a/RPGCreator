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
using RPGCreator.SDK.Assets.MetaData;
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Projects;

[EngineClass("rpgc", "sdk", "project_link")]
public sealed partial class ProjectLink() : IEngineObject
{
    [JsonPropertyName("Id")]
    public Ulid ProjectId { get; set; } = Ulid.Empty;
    [JsonPropertyName("ProjectPath")]
    public string ProjectLastKnownPath { get; set; } = string.Empty;
    [JsonPropertyName("LastOpened")]
    public DateTime ProjectLastOpened { get; set; } = DateTime.MinValue;

    public ProjectLink(ProjectMetaData project) : this()
    {
        ProjectId = project.Unique;
        ProjectLastKnownPath = project.JsonFilePath;
        ProjectLastOpened = DateTime.UtcNow;
    }

    [JsonConstructor]
    // ReSharper disable InconsistentNaming
    public ProjectLink(Ulid ProjectId, string ProjectLastKnownPath, DateTime ProjectLastOpened) : this()
    // ReSharper restore InconsistentNaming
    {
        this.ProjectId = ProjectId;
        this.ProjectLastKnownPath = ProjectLastKnownPath;
        this.ProjectLastOpened = ProjectLastOpened;
    }
}
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

using System.Collections.Immutable;
using Fluxor;
using RPGCreator.SDK.Assets.MetaData;

namespace RPGCreator.SDK.States;

public record ProjectState(
    ProjectMetaData? ProjectMetaData
)
{
    public string Name => ProjectMetaData?.Name ?? "Unnamed Project";
    public string Description => ProjectMetaData?.Description ?? "No description";
    public string ConfigFilePath => ProjectMetaData?.JsonFilePath ?? "";
    public string Directory => ProjectMetaData?.Directory ?? "";
    public Version Version => ProjectMetaData?.Version ?? new Version(0, 0, 1);
    public Version EditorVersion => ProjectMetaData?.EditorVersion ?? new Version(0, 0, 1);
    public bool IsArchived => ProjectMetaData?.IsArchived ?? false;
    public bool IsFavorite => ProjectMetaData?.IsFavorite ?? false;
    public string? Copyright => ProjectMetaData?.Copyright;
    
    public IImmutableList<string> Authors => ProjectMetaData?.Authors.ToImmutableList() ?? ImmutableList<string>.Empty;
    public IImmutableList<string> Modules => ProjectMetaData?.Modules.ToImmutableList() ?? ImmutableList<string>.Empty;
    
    public Ulid MainMapId => ProjectMetaData?.MainMapId ?? Ulid.Empty;
}

public class FeatureProjectState : Feature<ProjectState>
{
    public override string GetName() => "ProjectState";

    protected override ProjectState GetInitialState() => new(null);
}
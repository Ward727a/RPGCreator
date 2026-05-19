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

using RPGCreator.SDK.Assets.MetaData;

namespace RPGCreator.SDK.States.Actions;

public static class ProjectActions
{
    public record OpenProjectAction(ProjectMetaData ProjectMetaData);
    public record CloseProjectAction();
    
    public record ChangeNameAction(string NewName);
    public record ChangeDescriptionAction(string NewDescription);
    public record ChangeVersionAction(Version NewVersion);
    public record ChangeEditorVersionAction(Version NewEditorVersion);
    public record ChangeCopyrightAction(string? NewCopyright);
    
    public record AddAuthorAction(string NewAuthor);
    public record RemoveAuthorAction(string Author);
    
    public record AddModuleAction(string NewModule);
    public record RemoveModuleAction(string Module);
    
    public record SetMainMapAction(Ulid NewMainMapId);
    
    public record SetFavoriteAction(bool IsFavorite);
    public record SetArchivedAction(bool IsArchived);
}
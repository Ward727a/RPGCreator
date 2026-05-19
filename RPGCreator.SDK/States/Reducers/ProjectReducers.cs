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

using Fluxor;
using RPGCreator.SDK.States.Actions;

namespace RPGCreator.SDK.States.Reducers;

public static class ProjectReducers
{
    [ReducerMethod]
    public static ProjectState ReduceOpenProject(ProjectState currentState, ProjectActions.OpenProjectAction action)
        => currentState with { ProjectMetaData = action.ProjectMetaData };

    [ReducerMethod]
    public static ProjectState ReduceCloseProject(ProjectState currentState, ProjectActions.CloseProjectAction action)
        => currentState with { ProjectMetaData = null };

    [ReducerMethod]
    public static ProjectState ReduceChangeName(ProjectState currentState, ProjectActions.ChangeNameAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Name = action.NewName;

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceChangeDescription(ProjectState currentState,
        ProjectActions.ChangeDescriptionAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Description = action.NewDescription;

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceChangeVersion(ProjectState currentState, ProjectActions.ChangeVersionAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Version = action.NewVersion;

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceChangeEditorVersion(ProjectState currentState,
        ProjectActions.ChangeEditorVersionAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.EditorVersion = action.NewEditorVersion;

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceChangeCopyright(ProjectState currentState,
        ProjectActions.ChangeCopyrightAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Copyright = action.NewCopyright;

        return currentState with { ProjectMetaData = updatedMetadata };
    }
    
    [ReducerMethod]
    public static ProjectState ReduceAddAuthor(ProjectState currentState, ProjectActions.AddAuthorAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Authors = updatedMetadata.Authors.Append(action.NewAuthor).ToList();

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceRemoveAuthor(ProjectState currentState, ProjectActions.RemoveAuthorAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Authors = updatedMetadata.Authors.Where(a => a != action.Author).ToList();

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceAddModule(ProjectState currentState, ProjectActions.AddModuleAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Modules = updatedMetadata.Modules.Append(action.NewModule).ToList();

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceRemoveModule(ProjectState currentState, ProjectActions.RemoveModuleAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.Modules = updatedMetadata.Modules.Where(m => m != action.Module).ToList();

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceSetMainMap(ProjectState currentState, ProjectActions.SetMainMapAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.MainMapId = action.NewMainMapId;

        return currentState with { ProjectMetaData = updatedMetadata };
    }

    [ReducerMethod]
    public static ProjectState ReduceSetFavorite(ProjectState currentState, ProjectActions.SetFavoriteAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.IsFavorite = action.IsFavorite;

        return currentState with { ProjectMetaData = updatedMetadata };
    }
    
    [ReducerMethod]
    public static ProjectState ReduceSetArchived(ProjectState currentState, ProjectActions.SetArchivedAction action)
    {
        if (currentState.ProjectMetaData == null)
            return currentState;

        var updatedMetadata = currentState.ProjectMetaData.Clone();

        updatedMetadata.IsArchived = action.IsArchived;

        return currentState with { ProjectMetaData = updatedMetadata };
    }
}
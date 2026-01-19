#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Project.EventsArgs;

namespace RPGCreator.Core.Types.Project
{
    public class ProjectEvent
    {

        event EventHandler<ProjectLoadingArgs>? Loading;
        public virtual void OnLoading(ProjectLoadingArgs e)
        {
            Loading?.Invoke(this, e);
        }
        event EventHandler<ProjectLoadedArgs>? Loaded;
        public virtual void OnLoaded(ProjectLoadedArgs e)
        {
            Loaded?.Invoke(this, e);
        }

        event EventHandler<ProjectDeletingArgs>? Deleting;
        public virtual void OnDeleting(ProjectDeletingArgs e)
        {
            Deleting?.Invoke(this, e);
        }
        event EventHandler<ProjectDeletedArgs>? Deleted;
        public virtual void OnDeleted(ProjectDeletedArgs e)
        {
            Deleted?.Invoke(this, e);
        }

        event EventHandler<ProjectSavingArgs>? Saving;
        public virtual void OnSaving(ProjectSavingArgs e)
        {
            Saving?.Invoke(this, e);
        }

        event EventHandler<ProjectSavedArgs>? Saved;
        public virtual void OnSaved(ProjectSavedArgs e)
        {
            Saved?.Invoke(this, e);
        }

        event EventHandler<ProjectLoadingMapArgs>? LoadingMap;
        public virtual void OnLoadingMap(ProjectLoadingMapArgs e)
        {
            LoadingMap?.Invoke(this, e);
        }

        event EventHandler<ProjectLoadedMapArgs>? LoadedMap;
        public virtual void OnLoadedMap(ProjectLoadedMapArgs e)
        {
            LoadedMap?.Invoke(this, e);
        }

        event EventHandler<ProjectDeletingArgs>? DeletingMap;
        public virtual void OnDeletingMap(ProjectDeletingArgs e)
        {
            DeletingMap?.Invoke(this, e);
        }

        event EventHandler<ProjectDeletedArgs>? DeletedMap;
        public virtual void OnDeletedMap(ProjectDeletedArgs e)
        {
            DeletedMap?.Invoke(this, e);
        }

        event EventHandler<ProjectSavingMapArgs>? SavingMap;
        public virtual void OnSavingMap(ProjectSavingMapArgs e)
        {
            SavingMap?.Invoke(this, e);
        }

        event EventHandler<ProjectSavingMapArgs>? SavedMap;
        public virtual void OnSavedMap(ProjectSavingMapArgs e)
        {
            SavedMap?.Invoke(this, e);
        }

        event EventHandler<ProjectCreatingMapArgs>? CreatingMap;
        public virtual void OnCreatingMap(ProjectCreatingMapArgs e)
        {
            CreatingMap?.Invoke(this, e);
        }

        event EventHandler<ProjectSavingMapArgs>? CreatedMap;
        public virtual void OnCreatedMap(ProjectSavingMapArgs e)
        {
            CreatedMap?.Invoke(this, e);
        }

        public event EventHandler? MapsListChanged;
        public virtual void OnMapsListChanged()
        {
            MapsListChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}

#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
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
using RPGCreator.Core.Managers.ProjectsManager.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.ProjectsManager
{
    public class ProjectsManagerEvent
    {

        public event EventHandler<ProjectsManagerLoadingProjectArgs>? LoadingProject;
        public virtual void OnLoadingProject(ProjectsManagerLoadingProjectArgs e)
        {
            LoadingProject?.Invoke(this, e);
        }

        public event EventHandler<ProjectsManagerLoadedProjectArgs>? LoadedProject;
        public virtual void OnLoadedProject(ProjectsManagerLoadedProjectArgs e)
        {
            LoadedProject?.Invoke(this, e);
        }

        public event EventHandler<ProjectsManagerUnloadingProjectArgs>? UnloadingProject;
        public virtual void OnUnloadingProject(ProjectsManagerUnloadingProjectArgs e)
        {
            UnloadingProject?.Invoke(this, e);
        }

        public event EventHandler<ProjectsManagerUnloadedProjectArgs>? UnloadedProject;
        public virtual void OnUnloadedProject(ProjectsManagerUnloadedProjectArgs e)
        {
            UnloadedProject?.Invoke(this, e);
        }

        public event EventHandler<ProjectsManagerSavingProjectArgs>? SavingProject;
        public virtual void OnSavingProject(ProjectsManagerSavingProjectArgs e)
        {
            SavingProject?.Invoke(this, e);
        }

        public event EventHandler<ProjectsManagerSavedProjectArgs>? SavedProject;
        public virtual void OnSavedProject(ProjectsManagerSavedProjectArgs e)
        {
            SavedProject?.Invoke(this, e);
        }

        public event EventHandler<ProjectsManagerDeletingProjectArgs>? DeletingProject;
        public virtual void OnDeletingProject(ProjectsManagerDeletingProjectArgs e)
        {
            DeletingProject?.Invoke(this, e);
        }

        public event EventHandler<ProjectsManagerDeletedProjectArgs>? DeletedProject;
        public virtual void OnDeletedProject(ProjectsManagerDeletedProjectArgs e)
        {
            DeletedProject?.Invoke(this, e);
        }

    }
}

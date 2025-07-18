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
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.Core.Managers.ProjectsManager.Events;
using RPGCreator.Core.Type.Project;
using System.Collections.ObjectModel;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Managers.ProjectsManager
{
    public class ProjectsManager
    {

        public readonly ProjectsManagerEvent Events;

        public ProjectsManager()
        {
            Events = new ProjectsManagerEvent();
        }

        public void LoadProject(Ulid projectId)
        {
            if (EngineCore.Instance.Data.EditedProject != null)
            {
                UnloadProject();
            }

            var PreArgs = new ProjectsManagerLoadingProjectArgs(projectId);

            Events.OnLoadingProject(PreArgs);

            ProjectsConf projectsConf = EngineCore.Instance.Configs.GetConfig<ProjectsConf>("ProjectsConf");

            if (!projectsConf.TryGetProject(projectId, out var project))
            {
                Events.OnLoadedProject(PreArgs.ToPost(null).SetError(true, "Project not found"));
                return;
            }

            project.Load();

            EngineCore.Instance.Data.EditedProject = project;


            Events.OnLoadedProject(PreArgs.ToPost(EngineCore.Instance.Data.EditedProject));
        }

        public void UnloadProject()
        {

            var PreArgs = new ProjectsManagerUnloadingProjectArgs();

            Events.OnUnloadingProject(PreArgs);

            if(PreArgs.Cancel)
                return;

            if (EngineCore.Instance.Data.EditedProject == null)
            {
                Events.OnUnloadedProject(PreArgs.ToPost().SetError(true, "No project loaded"));
                return;
            }
            // TODO: Unload the project here

            EngineCore.Instance.Data.EditedProject.Unload();
            EngineCore.Instance.Data.EditedProject = null;

            Events.OnUnloadedProject(PreArgs.ToPost());

            //EngineCore.Instance.Events.OnEngineUnloadProject(new());
        }

        public BaseProject? CreateProject(string project_name, string project_path)
        {
            if (string.IsNullOrWhiteSpace(project_name))
            {
                throw new ArgumentException("Project name cannot be null or empty.", nameof(project_name));
            }
            if (string.IsNullOrWhiteSpace(project_path))
            {
                throw new ArgumentException("Project path cannot be null or empty.", nameof(project_path));
            }

            var newProject = new BaseProject(project_name)
            {
                Path = project_path
            };
            
            ProjectsConf.Instance.SaveProject(newProject);

            return newProject;
        }

        public List<BaseProjectLink> GetProjectsList()
        {
            return ProjectsConf.Instance.ProjectLinks;
        }

    }
}

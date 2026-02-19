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

using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.Core.Types.Project;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.SDK;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Types.Interfaces;
using Serilog;

namespace RPGCreator.Core.Managers.ProjectsManager
{
    public class ProjectsManager : IProjectsManager
    {
        public ProjectsManager()
        {
        }

        public IBaseProject? CreateProject(string project_name, string project_path)
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
            
            // Create a new asset pack for the project
            var assetsPack = new BaseAssetsPack(Path.Combine(newProject.Path, "assets_pack.pack"));
            assetsPack.Name = $"{project_name} Assets Pack";
            assetsPack.Description = $"Default assets pack for the project {project_name}";
            assetsPack.Save();
            
            newProject.AssetsPackPath.Add(assetsPack.DbFilePath);
            
            ProjectsConf.Instance.SaveProject(newProject);

            return newProject;
        }

        public event Action<IBaseProject>? OnProjectOpened;

        public List<BaseProjectLink> GetAllProjects()
        {
            return ProjectsConf.Instance.ProjectLinks;
        }
        
        public bool TryGetProject(string configPath, [NotNullWhen(true)] out IBaseProject? project)
        {
            project = null;
            if (File.Exists(configPath))
            {
                EngineServices.SerializerService.Deserialize<BaseProject>(File.ReadAllText(configPath), out var _projectObject, out System.Type? objectType);

                if (objectType == null)
                    return false;
            
                project = _projectObject;
        
                return true;
            }
            return false;
        }

        public void OpenProject(IBaseProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project), "Project cannot be null.");
            }

            GlobalStates.ProjectState.CurrentProject = project;
            
            foreach (string packPath in project.AssetsPackPath)
            {
                try
                {
                    EngineCore.Instance.Managers.Assets.AddPack(packPath);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to load assets pack at {packPath}: {Message}", packPath, ex.Message);
                    continue; // Skip this pack and continue with the next one
                }
            }

            EngineServices.GlobalPathData = project.GlobalPathData;
            
            OnProjectOpened?.Invoke(project);
        }

        public void CloseCurrentProject()
        {
            GlobalStates.ProjectState.CurrentProject = null;
        }
        
        public IBaseProject? GetCurrentProject()
        {
            return GlobalStates.ProjectState.CurrentProject;
        }
    }
}

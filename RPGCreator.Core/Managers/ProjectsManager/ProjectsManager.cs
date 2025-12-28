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
using RPGCreator.Core.Types.Project;
using System.Collections.ObjectModel;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.Core.Managers.ProjectsManager
{
    public class ProjectsManager : IProjectsManager
    {

        public readonly ProjectsManagerEvent Events;

        public ProjectsManager()
        {
            Events = new ProjectsManagerEvent();
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

        public List<IBaseProjectLink> GetAllProjects()
        {
            return ProjectsConf.Instance.ProjectLinks;
        }
        
        public bool TryGetProject(string configPath, out IBaseProject? project)
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

    }
}

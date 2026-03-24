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
using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.Core.Types.Project;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Projects;
using Serilog;

namespace RPGCreator.Core.Managers.ProjectsManager
{
    public class ProjectsManager : IProjectsManager
    {
        public event Action<IBaseProject>? OnProjectOpened;
        
        private ProjectConfig _config;
        
        public ProjectsManager()
        {
            if (EngineServices.Config.TryFrom<ProjectConfig>("projects", true, out var config))
            {
                _config = config;
            }
            else
            {
                _config = new ProjectConfig();
                EngineServices.Config.CreateConfig("projects", _config, true);
            }
            
        }

        public IBaseProject CreateProject(string projectName, string projectPath, string description = "")
        {
            Guard.IsNotNullOrWhiteSpace(projectName);
            Guard.IsNotNullOrWhiteSpace(projectPath);
            
            var newProject = new BaseProject(projectName)
            {
                Path = projectPath,
                Description = description
            };
            
            // Create a new asset pack for the project
            var assetsPack = new BaseAssetsPack(Path.Combine(newProject.Path, "assets_pack.pack"));
            assetsPack.Name = $"{projectName} Assets Pack";
            assetsPack.Description = $"Default assets pack for the project {projectName}";
            assetsPack.Save();
            
            newProject.AssetsPackPath.Add(assetsPack.DbFilePath);
            
            _config.AddOrUpdateProject(newProject);
            // Here we force save, as we don't want to wait for the save loop.
            _config.SaveConfig();

            return newProject;
        }


        public List<BaseProjectLink> GetAllProjects()
        {
            if (EngineServices.Config.TryFrom("projects", true, out var config))
            {
                return config.Get("links", new List<BaseProjectLink>());
            }
            
            EngineServices.Config.CreateConfig("projects", new ProjectConfig(), true);
            return [];
        }
        
        public bool TryGetProject(string configPath, [NotNullWhen(true)] out IBaseProject? project)
        {
            project = null;
            if (File.Exists(configPath))
            {
                EngineServices.Serializer.DeserializeFrom<BaseProject>(configPath, out var projectObject);

                if (projectObject == null)
                    return false;
            
                project = projectObject;
        
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
                }
            }

            EngineServices.PathRegistry = project.PathRegistry;
            
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

        public bool SaveProject(IBaseProject project)
        {
            return _config.AddOrUpdateProject(project);
        }

        [SerializingType("projectConfig")]
        private class ProjectConfig : BaseConfig
        {
            public List<BaseProjectLink> ProjectLinks = new List<BaseProjectLink>();

            private void AddProject(IBaseProject project)
            {
                if (project == null)
                {
                    throw new ArgumentNullException(nameof(project), "Project cannot be null.");
                }

                var link = BaseProjectLink.CreateLinkFromProject(project);
            
                ProjectLinks.Add(link);
            }

            public bool AddOrUpdateProject(IBaseProject project)
            {
                IsDirty = true;
                var link = ProjectLinks.Find(link => link.ProjectID == project.Id);
                
                if(link == null)
                {
                    AddProject(project);
                    link = ProjectLinks.Last();
                }
                
                string projectFilePath = link.ProjectConfigPath;
                
                if (string.IsNullOrEmpty(projectFilePath))
                {
                    Logger.Error("Project config path is not set. Cannot save project.");
                    return false;
                }
                
                var projectDir = Path.GetDirectoryName(projectFilePath);

                if (projectDir == null)
                {
                    Logger.Error("Failed to get directory name for project config path: {ProjectConfigPath}", projectFilePath);
                    return false;
                }
                
                if (!Directory.Exists(projectDir))
                {
                    Directory.CreateDirectory(projectDir);
                }
                
                EngineServices.Serializer.SerializeTo(project, projectFilePath);
                return true;
            }
            
            protected override void _OnLoadedConfig()
            {
                ProjectLinks = Get("links", new List<BaseProjectLink>());
            }

            protected override void _OnSavedConfig()
            {
                Set("links", ProjectLinks);
            }
        }
        
    }
}

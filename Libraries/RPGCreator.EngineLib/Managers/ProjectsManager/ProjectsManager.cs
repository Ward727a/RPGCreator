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
using RPGCreator.EngineLib.Types.Project;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.MetaData;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Services.EngineService;

namespace RPGCreator.EngineLib.Managers.ProjectsManager
{
    public class ProjectsManager : IProjectsManager
    {
        public event Action<IBaseProject>? OnProjectOpened;
        
        private ProjectConfig _config;
        
        public ProjectsManager()
        {
            if (EngineServices.Config.TryFrom<BaseConfig>("projects", true, out var config))
            {
                _config = new(config);
            }
            else
            {
                _config = new ProjectConfig();
                EngineServices.Config.CreateConfig("projects", _config.Config, true);
            }
            
        }

        public IBaseProject CreateProject(string projectName, string projectPath, string description = "")
        {
            Guard.IsNotNullOrWhiteSpace(projectName);
            Guard.IsNotNullOrWhiteSpace(projectPath);

            var projectMeta = ProjectMetaData.Create(projectPath);
            projectMeta.Name = projectName;
            projectMeta.Description = description;
            
            _config.AddOrUpdateProject(projectMeta);
            // Here we force save, as we don't want to wait for the save loop.
            _config.SaveConfig();

            return new Project(projectMeta);
        }


        public List<ProjectLink> GetAllProjects()
        {
            if (EngineServices.Config.TryFrom("projects", true, out var config))
            {
                return config.Get("links", new List<ProjectLink>());
            }
            
            EngineServices.Config.CreateConfig("projects", new ProjectConfig().Config, true);
            return [];
        }
        
        public bool TryGetProject(string configPath, [NotNullWhen(true)] out IBaseProject? project)
        {
            project = null;
            if (File.Exists(configPath))
            {
                EngineServices.Serializer.DeserializeFrom<ProjectMetaData>(configPath, out var projectObject);

                if (projectObject == null)
                    return false;
            
                project = new Project(projectObject);
        
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
            
            EngineServices.AssetsManager.RefreshAssets();

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
            return _config.AddOrUpdateProject(project.MetaData);
        }

        private class ProjectConfig
        {
            public readonly BaseConfig Config;
            public List<ProjectLink> ProjectLinks = new List<ProjectLink>();

            public ProjectConfig()
            {
                Config = new BaseConfig();
                Config.ConfigLoaded += _OnLoadedConfig;
                Config.ConfigSaved += _OnSavedConfig;
            }
            
            public ProjectConfig(BaseConfig config)
            {
                Config = config;
                config.ConfigLoaded += _OnLoadedConfig;
                config.ConfigSaved += _OnSavedConfig;
            }

            private ProjectLink AddProject(ProjectMetaData project)
            {
                if (project == null)
                {
                    throw new ArgumentNullException(nameof(project), "Project cannot be null.");
                }

                var link = new ProjectLink(project);
            
                ProjectLinks.Add(link);
                return link;
            }

            public bool AddOrUpdateProject(ProjectMetaData project)
            {
                Config.MarkDirty();
                var link = ProjectLinks.Find(link => link.ProjectId == project.Unique);
                
                if(link == default)
                {
                    link = AddProject(project);
                }
                
                string projectFilePath = link.ProjectLastKnownPath;
                
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
            
            protected  void _OnLoadedConfig()
            {
                ProjectLinks = Config.Get("links", new List<ProjectLink>());
            }

            protected  void _OnSavedConfig()
            {
                Config.Set("links", ProjectLinks);
            }

            public void SaveConfig()
            {
                Config.SaveConfig();
            }
        }
        
    }
}

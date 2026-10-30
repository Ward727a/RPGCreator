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
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Assets.MetaData;
using RPGCreator.SDK.IO.Workspace;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Projects;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.States;
using RPGCreator.SDK.States.Actions;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.EngineLib.Managers.Project
{
    
    [EngineService(typeof(IProjectsManager), ServiceLifetime.Singleton, EngineModeFlags.All)]
    public class ProjectsManager : IProjectsManager
    {
        private readonly IDispatcher _dispatcher;
        private readonly IState<ProjectState> _projectState;
        private readonly IProjectRegistryManager _projectRegistry;
        private readonly ISerializerService _serializer;
        private readonly IAssetManager _assetsManager;
        private readonly IResourceManager _resourceManager;
        private readonly IMaterialRegistry _materialRegistry;

        public event Action<IBaseProject>? OnProjectOpened;

        public ProjectsManager(IDispatcher dispatcher, IState<ProjectState> projectState,
            IProjectRegistryManager projectRegistry, ISerializerService serializer, IAssetManager assetsManager, IResourceManager resourceManager,
            IMaterialRegistry materialRegistry)
        {
            _dispatcher = dispatcher;
            _projectState = projectState;
            _projectRegistry = projectRegistry;
            _serializer = serializer;
            _assetsManager = assetsManager;
            _resourceManager = resourceManager;
            _materialRegistry = materialRegistry;

            _projectRegistry.LoadAsync();
        }

        public IBaseProject CreateProject(string projectName, string projectPath, string description = "")
        {
            Guard.IsNotNullOrWhiteSpace(projectName);
            Guard.IsNotNullOrWhiteSpace(projectPath);

            var projectMeta = ProjectMetaData.Create(projectPath);
            projectMeta.Name = projectName;
            projectMeta.Description = description;

            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);
            }
            
            _serializer.SerializeTo(projectMeta, Path.Combine(projectPath, "project.config.json"));
            
            _projectRegistry.Projects.KnownProjects[projectMeta.Unique] = new ProjectLink(projectMeta);
            _projectRegistry.SaveAsync();
            
            return new Types.Project.Project(projectMeta);
        }

        public void DeleteProject(IBaseProject project)
        {
            Guard.IsNotNull(project, nameof(project));

            if (_projectRegistry.Projects.KnownProjects.ContainsKey(project.MetaData.Unique))
            {
                _projectRegistry.Projects.KnownProjects.Remove(project.MetaData.Unique);
                _projectRegistry.SaveAsync();
            }
        }


        public List<ProjectLink> GetAllProjects()
        {
            return [.. _projectRegistry.Projects.KnownProjects.Values];
        }
        
        public bool TryGetProject(string configPath, [NotNullWhen(true)] out IBaseProject? project)
        {
            project = null;
            if (!configPath.EndsWith(".config.json"))
            {
                configPath = Path.Combine(configPath, "project.config.json");
            }
            if (File.Exists(configPath))
            {
                _serializer.DeserializeFrom<ProjectMetaData>(configPath, out var projectObject);

                if (projectObject == null)
                    return false;
            
                project = new Types.Project.Project(projectObject);
        
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

            _dispatcher.Dispatch(new ProjectActions.OpenProjectAction(project.MetaData));
            
            _resourceManager.LoadAllResourcesFromDisk();
            _assetsManager.RefreshAssets();
            _materialRegistry.Load();
        }

        public void CloseCurrentProject()
        {
            _dispatcher.Dispatch(new ProjectActions.CloseProjectAction());
        }
        
        public ProjectMetaData? GetCurrentProject()
        {
            return _projectState.Value.ProjectMetaData;
        }

        public bool SaveProject(IBaseProject project)
        {
            _projectRegistry.Projects.KnownProjects[project.MetaData.Unique] = new ProjectLink(project.MetaData);
            _projectRegistry.SaveAsync();
            return true;
        }
    }
}

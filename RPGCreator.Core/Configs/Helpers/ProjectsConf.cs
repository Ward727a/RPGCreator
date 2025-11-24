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
#endregion
using RPGCreator.Core.Types.Project;
using RPGCreator.Core.Types.Internal;
using static RPGCreator.Core.Configs.EngineConfigs;

/*
 *
 * ProjectsConf
 * ============
 * This class is used to manage the projects configuration.
 *
 * DevNote:
 * Right now I'm trying to rework the old system that was used to save projects to use the project link system.
 * It will allow to have a better management, without having to load all the projects at once, and also to not have to save all the projects in 2 different places.
 * [Ward, 17/07/2025] (Done)
 * 
 */

namespace RPGCreator.Core.Configs.Helpers
{
    public class ProjectsConf : ConfHelper
    {
        public override string ConfigName { get; set; } = "ProjectsConf";
        public static ProjectsConf Instance { get; private set; }

        public List<BaseProjectLink> ProjectLinks { get; private set; } = [];

        public ProjectsConf() : base()
        {
            Instance = this;
        }
        
        public void AddProject(BaseProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project), "Project cannot be null.");
            }

            var link = BaseProjectLink.CreateLinkFromProject(project);
            
            ProjectLinks.Add(link);
        }
        
        public bool TryGetProject(Ulid projectId, out BaseProject? project)
        {
            project = null;
            var link = ProjectLinks.FirstOrDefault(l => l.ProjectID == projectId);
            if (link != null && link.TryGetProject(out project))
            {
                return true;
            }
            #if DEBUG
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            #endif
            return false;
        }

        public void SaveProject(BaseProject project, bool force = false)
        {
            EngineSerializer.Instance.Serialize(project, out string projectData);

            var link = ProjectLinks.Find(link => link.ProjectID == project.Id);
            if(link == null)
            {
                AddProject(project);
                link = ProjectLinks.Last();
            }
            
            if(ConfigPath == null)
            {
                throw new InvalidOperationException("ConfigPath is not set. Cannot save project.");
            }
            
            EngineSerializer.Instance.Serialize(this, out string configData);
            
            // Save the configuration data to the config file
            File.WriteAllText(ConfigPath, configData);
            
            // Then save the project data to the project config file
            string projectConfigPath = link.ProjectConfigPath;
            
            if (string.IsNullOrEmpty(projectConfigPath))
            {
                throw new InvalidOperationException("Project config path is not set. Cannot save project.");
            }
            if (!Directory.Exists(Path.GetDirectoryName(projectConfigPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(projectConfigPath)!);
            }
            File.WriteAllText(projectConfigPath, projectData);
            
            Console.WriteLine($"Saved project '{project.Name}' to config at {ConfigPath}.");
        }

        public override SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(ProjectsConf));
            info.AddValue("projectLinks", ProjectLinks);
            return info;
        }

        public override void SetObjectData(Serializer.DeserializationInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
            }

            info.TryGetList("projectLinks", out List<BaseProjectLink> projectLinks);

            ProjectLinks = projectLinks;
        }
    }
}

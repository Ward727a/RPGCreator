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
using RPGCreator.Core.Type.Project;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RPGCreator.Core.Type.Internal;
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
 * [Ward, 17/07/2025]
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
            EngineSerializer.Instance.Serialize(project, out string projectData, false);

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
            
            EngineSerializer.Instance.Serialize(this, out string configData, false);
            
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
            
            // OLD CODE - For Reference Only
//             var projectElement = Root.Elements("project").FirstOrDefault(x => x.Element("name")?.Value == project.Name);
//
//             if (projectElement != null)
//             {
//
//                 if (projectElement.Elements().Any(e => e.Name == "do-not-load"))
//                 {
//                     return;
//                 }
//
//                 string? project_config_path = projectElement.Element("path")?.Value ?? null;
//
//                 if (string.IsNullOrEmpty(project_config_path) || !File.Exists(project_config_path))
//                 {
//                     projectElement.Add(new XElement("do-not-load", "Reason: Project config path is not valid!"));
//                     return;
//                 }
//
//                 XDocument projectDoc;
//                 XElement memProjectElem = projectElement;
//
//                 if(!File.Exists(project_config_path))
//                 {
//                     File.WriteAllText(project_config_path, @"<?xml version=""1.0"" encoding=""utf-8""?>
// <project>
// </project>");
//                 }
//
//                 try
//                 {
//                     projectDoc = XDocument.Load(project_config_path);
//                     projectElement = projectDoc.Root;
//                 }
//                 catch (Exception ex)
//                 {
//                     memProjectElem.Add(new XElement("do-not-load", $"Reason: {ex.Message}"));
//                     return;
//                 }
//
//                 if (projectElement == null)
//                 {
//                     memProjectElem.Add(new XElement("do-not-load", "Reason: Project config path is not valid!"));
//                     return;
//                 }
//
//                 if (force || !projectElement.HasElements)
//                 {
//                     projectElement.RemoveAll();
//                     projectElement.Add(new XElement("name", project.Name));
//                     projectElement.Add(new XElement("path", project.Path));
//                     projectElement.Add(new XElement("description", project.Description));
//                     projectElement.Add(new XElement("version", project.Version.ToString()));
//                     projectElement.Add(new XElement("editorVersion", project.EditorVersion.ToString()));
//                     projectElement.Add(new XElement("isArchived", project.IsArchived.ToString()));
//                     projectElement.Add(new XElement("isFavorite", project.IsFavorite.ToString()));
//                     projectElement.Add(new XElement("copyright", project.Copyright));
//
//                     XElement authorsElement = new XElement("authors");
//                     foreach (string author in project.Authors ?? [])
//                     {
//                         authorsElement.Add(new XElement("author", author));
//                     }
//                     projectElement.Add(authorsElement);
//
//                     XElement packsElement = new XElement("packs");
//                     foreach (string pack in project.AssetsPackPath)
//                     {
//                         packsElement.Add(new XElement("pack", pack));
//                     }
//                     projectElement.Add(packsElement);
//                 }
//                 else
//                 {
//                     projectElement.SetElementValue("name", project.Name);
//                     projectElement.SetElementValue("path", project.Path);
//                     projectElement.SetElementValue("description", project.Description);
//                     projectElement.SetElementValue("version", project.Version.ToString());
//                     projectElement.SetElementValue("editorVersion", project.EditorVersion.ToString());
//                     projectElement.SetElementValue("isArchived", project.IsArchived.ToString());
//                     projectElement.SetElementValue("isFavorite", project.IsFavorite.ToString());
//                     projectElement.SetElementValue("copyright", project.Copyright);
//
//                     projectElement.Element("authors")?.RemoveAll();
//
//                     foreach (var author in project.Authors ?? [])
//                     {
//                         projectElement.Element("authors").Add(new XElement("author", author));
//                     }
//
//                     projectElement.Element("packs")?.RemoveAll();
//
//                     foreach (var pack in project.AssetsPackPath)
//                     {
//                         projectElement.Element("packs").Add(new XElement("pack", pack));
//                     }
//
//                 }
//                 projectDoc.Save(project_config_path);
//             }
//             else
//             {
//                 projectElement = new XElement("project");
//                 projectElement.Add(new XElement("name", project.Name));
//
//                 string projectConfigPath = Path.Combine(project.Path, "project.conf.xml");
//
//                 projectElement.Add(new XElement("path", projectConfigPath));
//
//                 if (!Directory.Exists(project.Path))
//                 {
//                     Directory.CreateDirectory(project.Path);
//                 }
//
//                 if (!File.Exists(projectConfigPath))
//                 {
//                     File.WriteAllText(projectConfigPath, @"<?xml version=""1.0"" encoding=""utf-8""?>
// <project>
// </project>");
//                 }
//
//                 try
//                 {
//                     XDocument projectConfig = XDocument.Load(projectConfigPath);
//                     var root = projectConfig.Root;
//                     root.RemoveAll();
//                     root.Add(new XElement("name", project.Name));
//                     root.Add(new XElement("path", project.Path));
//                     root.Add(new XElement("description", project.Description));
//                     root.Add(new XElement("version", project.Version.ToString()));
//                     root.Add(new XElement("editorVersion", project.EditorVersion.ToString()));
//                     root.Add(new XElement("isArchived", project.IsArchived.ToString()));
//                     root.Add(new XElement("isFavorite", project.IsFavorite.ToString()));
//                     root.Add(new XElement("copyright", project.Copyright));
//                     XElement authorsElement = new XElement("authors");
//                     foreach (string author in project.Authors ?? [])
//                     {
//                         authorsElement.Add(new XElement("author", author));
//                     }
//                     root.Add(authorsElement);
//                     XElement packsElement = new XElement("packs");
//                     foreach (string pack in project.AssetsPackPath)
//                     {
//                         packsElement.Add(new XElement("pack", pack));
//                     }
//                     root.Add(packsElement);
//                     projectConfig.Save(projectConfigPath);
//
//                 }
//                 catch (Exception ex)
//                 {
//                     projectElement.Add(new XElement("do-not-load", $"Reason: {ex.Message}"));
//                     return;
//                 }
//                 Root.Add(projectElement);
//             }
        }

        public override SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(ProjectsConf));
            info.AddValue("projectLinks", ProjectLinks);
            return info;
        }

        public override void SetObjectData(SerializationInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
            }

            info.TryGetList("projectLinks", out List<BaseProjectLink> projectLinks, [], "Project links not found or invalid (Set to empty list by default).");

            ProjectLinks = projectLinks;
        }
    }
}

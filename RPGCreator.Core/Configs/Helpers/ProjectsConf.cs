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
using static RPGCreator.Core.Configs.EngineConfigs;

namespace RPGCreator.Core.Configs.Helpers
{
    public class ProjectsConf : ConfHelper
    {

        public struct SProjectData
        {
            public string name;
            public XElement element;
        }

        public ObservableCollection<BaseProject> Projects { get; private set; } = [];

        public void AddProject(BaseProject project)
        {
            Projects.Add(project);
        }

        public void SaveProject(BaseProject project, bool force = false)
        {
            EngineSerializer.Instance.Serialize(project, out string data, false);
            
            if(ConfigPath == null)
            {
                throw new InvalidOperationException("ConfigPath is not set. Cannot save project.");
            }
            
            if(!File.Exists(ConfigPath))
                throw new FileNotFoundException($"Config file not found at {ConfigPath}.");

            if (Root.Elements("project").Any())
            {
                foreach (var xElement in Root.Elements("project"))
                {
                    // We do this due to the old system that was used to save projects.
                    xElement.RemoveAll();
                }
            }
            
            XElement ProjectXmlData = XElement.Parse(data);
            
            Root.Add(ProjectXmlData);
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

        public override void LoadConfig()
        {

            if (!Root.HasElements)
                return;
            Projects.Clear();

            foreach(XElement project_target in Root.Elements("project"))
            {
                if(project_target.Element("do-not-load") != null)
                {
                    continue;
                }

                string? project_config_path = project_target.Element("path")?.Value ?? null;
                string? project_config_name = project_target.Element("name")?.Value ?? null;

                if (string.IsNullOrEmpty(project_config_path) || !File.Exists(project_config_path))
                {
                    project_target.Add(new XElement("do-not-load", "Reason: Project config path is not valid!"));
                    continue;
                }

                if(string.IsNullOrEmpty(project_config_name))
                {
                    project_target.Add(new XElement("do-not-load", "Reason: Project config name is not valid!"));
                    continue;
                }

                XDocument projectDoc;
                XElement projectElement;

                try
                {
                    projectDoc = XDocument.Load(project_config_path);
                    projectElement = projectDoc.Root;
                }
                catch (Exception ex)
                {
                    project_target.Add(new XElement("do-not-load", $"Reason: {ex.Message}"));
                    continue;
                }

                if (projectElement == null)
                {
                    project_target.Add(new XElement("do-not-load", "Reason: Project config path is not valid!"));
                    continue;
                }

                if(projectElement.Element("name") == null)
                {
                    project_target.Add(new XElement("do-not-load", "Reason: Project config name is not valid!"));
                    continue;
                }

                string? project_name = projectElement.Element("name")?.Value ?? null;

                if(string.IsNullOrEmpty(project_name))
                {
                    project_target.Add(new XElement("do-not-load", "Reason: Project config name is not valid!"));
                    continue;
                }

                if(project_name != project_config_name)
                {
                    project_target.Add(new XElement("do-not-load", $"Reason: Project config name from {project_config_path} is not the same as the one in {ConfigPath}!"));
                    continue;
                }

                BaseProject project = new(project_name)
                {
                    Name = project_name ?? string.Empty,
                    Path = projectElement.Element("path")?.Value ?? string.Empty,
                    Description = projectElement.Element("description")?.Value ?? string.Empty,
                    Version = Version.Parse(projectElement.Element("version")?.Value ?? "0.0.0.0"),
                    EditorVersion = Version.Parse(projectElement.Element("editorVersion")?.Value ?? "0.0.0.0"),
                    IsArchived = bool.Parse(projectElement.Element("isArchived")?.Value ?? "false"),
                    IsFavorite = bool.Parse(projectElement.Element("isFavorite")?.Value ?? "false"),
                    Copyright = projectElement.Element("copyright")?.Value ?? "",
                    Authors = projectElement.Element("authors")?.Elements("author").Select(x => x.Value).ToList() ?? [],
                };

                project.AssetsPackPath = projectElement.Element("packs")?.Elements("pack").Select(x => x.Value).ToList() ?? [];

                Projects.Add(project);
            }

            return;
        }

        public override void Save()
        {
            foreach(BaseProject project in Projects)
            {
                SaveProject(project);
            }
            Doc.Save(ConfigPath);
        }
    }
}

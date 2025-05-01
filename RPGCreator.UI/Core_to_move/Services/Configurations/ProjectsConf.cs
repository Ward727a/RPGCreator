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
using RPGCreator.Core.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RPGCreator.Core.Services.ConfigurationService;
using System.Xml.Linq;
using Avalonia.Media;
using System.Collections.ObjectModel;

namespace RPGCreator.Core.Services.Configurations
{

    /// <summary>
    /// This class should not be manually created, but getted from <see cref="ConfigurationService"/><br/>
    /// This class manage all configuration related to the projects.conf.xml file.
    /// </summary>
    public class ProjectsConf : ConfHelper
    {
        public event EventHandler ProjectsListEdited;
        Dictionary<string, Project> _Projects = [];

        private bool _ParsedProjects = false;

        private event EventHandler? _InternalSave;

        private Dictionary<string, EventHandler> _ProjectsEvent = [];

        public bool HasParsedProjects => _ParsedProjects;
        public bool HasParsedTags => Tags.HasParsedTags;

        public _TagsConf Tags = new();

        /// <summary>
        /// This should ONLY be called by <see cref="ConfigurationService"/> class.<br/>
        /// This is called once all variable needed are setted. If called too late or too soon, the engine WILL crash.
        /// </summary>
        public override void OnLoadedConf()
        {
            if (Config != null)
            {
                Tags = new(Config.GetValueOrDefault());
                ParseProjects();
            }
        }

        public ObservableCollection<Project> GetProjects()
        {
            return [.. _Projects.Values.ToArray()];
        }

        private void ParseProjects()
        {

            XDocument document = Config.GetValueOrDefault().Document;

            if (document == default)
                throw new Exception("Document is not valid (null or default object).");

            if (document.Root == null)
                throw new Exception("Document root is null.");

            XElement root = document.Root;

            if (!root.HasElements)
                return;

            XElement[] projectList = [.. root.Elements().Where(e => e.Name == "Project")];

            foreach (XElement project_data in projectList)
            {
                // First we check REQUIRED elements
                if (project_data.Element("Name") == null)
                    continue;

                string project_name = project_data.Element("Name")!.Value;

                if (string.IsNullOrEmpty(project_name))
                    continue;

                Project project = new(project_name);

                if (project_data.Element("EditorVersion") == null)
                    continue;

                {
                    string project_editor_version = project_data.Element("EditorVersion")!.Value;

                    if (string.IsNullOrEmpty(project_editor_version))
                        continue;

                    project.EditorVersion = (Version.Parse(project_editor_version));
                }

                if (project_data.Element("Version") == null)
                    continue;

                {
                    string project_version = project_data.Element("Version")!.Value;

                    if (string.IsNullOrEmpty(project_version))
                        continue;

                    project.Version = Version.Parse(project_version);
                }

                if (project_data.Element("Path") == null)
                    continue;

                {
                    string project_path = project_data.Element("Path")!.Value;

                    if (string.IsNullOrEmpty(project_path) || !Directory.Exists(project_path))
                        continue;

                    project.Path = (project_path);
                }

                // Optional parameters

                if (project_data.Element("Description") != null)
                {
                    string project_desc = project_data.Element("Description")!.Value;

                    if (!string.IsNullOrEmpty(project_desc))
                    {
                        project.Description = (project_desc);
                    }
                }

                if (project_data.Element("Copyright") != null)
                {
                    string project_right = project_data.Element("Copyright")!.Value;

                    if (!string.IsNullOrEmpty(project_right))
                    {
                        project.Copyright = (project_right);
                    }
                }

                if (project_data.Element("Authors") != null)
                {
                    XElement authors_elem = project_data.Element("Authors")!;

                    if (authors_elem.HasElements)
                    {
                        foreach (XElement author in authors_elem.Elements().Where(e => e.Name == "Author").ToArray())
                        {
                            string project_author = author.Value;


                            if (!string.IsNullOrEmpty(project_author))
                            {
                                project.Authors.Add(project_author);
                            }
                        }
                    }
                }

                if (project_data.Element("LastEdited") != null)
                {
                    string project_last_edit = project_data.Element("LastEdited")!.Value;

                    if (!string.IsNullOrEmpty(project_last_edit))
                    {
                        project.LastEdited = (DateTime.Parse(project_last_edit));
                    }
                }

                if (project_data.Element("Tags") != null)
                {
                    XElement tags_elem = project_data.Element("Tags")!;

                    if (tags_elem.HasElements)
                    {
                        foreach (XElement tag in tags_elem.Elements().Where(e => e.Name == "Tag").ToArray())
                        {
                            string project_tag = tag.Value;

                            if (!string.IsNullOrEmpty(project_tag))
                            {
                                if (Tags.TryGet(project_tag, out Tag? _tag))
                                {
                                    if (_tag != null)
                                    {
                                        project.Tags.Add(_tag);
                                    }
                                }
                            }
                        }
                    }
                }

                if (project_data.Element("IsFavorite") != null)
                {
                    bool IsFavorite = project_data.Element("IsFavorite")!.Value.ToLower() == "true";

                    project.IsFavorite = (IsFavorite);
                }

                if (project_data.Element("IsArchived") != null)
                {
                    bool IsArchived = project_data.Element("IsArchived")!.Value.ToLower() == "true";

                    project.IsArchived = (IsArchived);
                }



                AssetsConf.TryLoadFromProject(ref project, out _);

                project.LinkXML(project_data);
                TryAddProject(project);
            }
        }

        /// <summary>
        /// Check if the project already exist.
        /// </summary>
        /// <param name="project_name"></param>
        /// <returns>True if it exist, false otherwise.</returns>
        public bool HasProject(string project_name)
        {
            return _Projects.ContainsKey(project_name);
        }

        /// <summary>
        /// Remove the project if it exist.
        /// </summary>
        /// <param name="project_name"></param>
        /// <returns>True if it was deleted, false otherwise.</returns>
        public bool RemoveProject(string project_name)
        {
            if (_Projects.TryGetValue(project_name, out var project))
            {
                _InternalSave -= _ProjectsEvent[project_name];
                _ProjectsEvent.Remove(project_name);
                return (project.DeleteProject() && _Projects.Remove(project_name));
            }
            return false;
        }

        /// <summary>
        /// Create a new project in the configuration if the name is available.<br/>
        /// You need to first create a new <see cref="Project"/> with a unique name.<br/>
        /// This initialize the project too.
        /// </summary>
        /// <param name="project"></param>
        /// <returns>True if it was created, false otherwise</returns>
        public bool NewProject(ref Project project)
        {
            string project_name = project.Name;

            if (_Projects.ContainsKey(project_name))
            {
                return false;
            }

            CONFIG config = Config.GetValueOrDefault();

            XElement? root = config.Document.Root;

            if (root == null)
                return false;
            int sameNameProject = root.Elements().Where(e => e.Element("Name")?.Value == project_name).Count();

            if (sameNameProject > 0)
                return false;

            XElement projectX = project.ToXElement();

            if (TryAddProject(project))
            {
                if (root.Element("Tags") != null)
                {
                    root.Element("Tags")?.AddBeforeSelf(projectX);
                }
                else
                {
                    root.Add(projectX);
                }
                project.LinkXML(projectX);

                return true;
            }
            return false;
        }

        private bool TryAddProject(Project project)
        {
            if (_Projects.TryAdd(project.Name, project))
            {
                _ProjectsEvent.Add(project.Name, (_, _) =>
                {
                    XDocument document = Config.GetValueOrDefault().Document;

                    if (document == default)
                        throw new Exception("Document is not valid (null or default object).");

                    if (document.Root == null)
                        throw new Exception("Document root is null.");

                    XElement root = document.Root;

                    if (!root.HasElements)
                        return;

                    XElement[] projects = root.Elements().Where(e => e.Name == "Project").Elements().Where(n => n.Name == "Name" && n.Value == project.Name).ToArray();

                    if (projects.Length == 0)
                    {
                        XElement project_elem = new("Project",
                            new XElement("Name", project.Name),
                            new XElement("EditorVersion", project.EditorVersion.ToString()),
                            new XElement("Version", project.Version.ToString()),
                            new XElement("Path", project.Path),
                            new XElement("Description", project.Description),
                            new XElement("Copyright", project.Copyright),
                            new XElement("Authors"),
                            new XElement("LastEdited", project.LastEdited.ToString()),
                            new XElement("Tags"),
                            new XElement("IsFavorite", project.IsFavorite.ToString()),
                            new XElement("IsArchived", project.IsArchived.ToString()));

                        XElement? authors_elem = project_elem.Element("Authors");

                        if (authors_elem != null && project.Authors.Count != 0)
                        {
                            foreach (string author in project.Authors)
                            {
                                authors_elem.Add(new XElement("Author", author));
                            }
                        }

                        XElement? tags_elem = project_elem.Element("Tags");

                        if (tags_elem != null && project.Tags.Count != 0)
                        {
                            foreach (Tag tag in project.Tags)
                            {
                                tags_elem.Add(new XElement("Tag", tag.Name));
                            }
                        }

                        root.Add(project_elem);
                    }
                    else
                    {
                        XElement? project_elem = projects[0].Parent;

                        if (project_elem != null)
                        {
#pragma warning disable CS8602
                            project_elem.Element("Name").SetValue(project.Name);
                            project_elem.Element("EditorVersion").SetValue(project.EditorVersion.ToString());
                            project_elem.Element("Version").SetValue(project.Version.ToString());
                            project_elem.Element("Path").SetValue(project.Path);
                            if (project_elem.Element("Description") != null)
                            {
                                project_elem.Element("Description").SetValue(project.Description);
                            }
                            else
                            {
                                project_elem.Add(new XElement("Description", project.Description));
                            }

                            if (project_elem.Element("Copyright") != null)
                            {
                                project_elem.Element("Copyright").SetValue(project.Copyright);
                            }
                            else
                            {
                                project_elem.Add(new XElement("Copyright", project.Copyright));
                            }

                            if (project_elem.Element("LastEdited") != null)
                            {
                                project_elem.Element("LastEdited").SetValue(project.LastEdited.ToString());
                            }
                            else
                            {
                                project_elem.Add(new XElement("LastEdited", project.LastEdited.ToString()));
                            }

                            if (project_elem.Element("IsFavorite") != null)
                            {
                                project_elem.Element("IsFavorite").SetValue(project.IsFavorite);
                            }
                            else
                            {
                                project_elem.Add(new XElement("IsFavorite", project.IsFavorite));
                            }

                            if (project_elem.Element("IsArchived") != null)
                            {
                                project_elem.Element("IsArchived").SetValue(project.IsArchived);
                            }
                            else
                            {
                                project_elem.Add(new XElement("IsArchived", project.IsArchived));
                            }

                            if (project_elem.Element("Tags") != null)
                            {
                                project_elem.Element("Tags").RemoveNodes();
                            }
                            else
                            {
                                project_elem.Add(new XElement("Tags"));
                            }

                            if (project_elem.Element("Tags") != null && project.Tags.Count != 0)
                            {
                                foreach (Tag tag in project.Tags)
                                {
                                    project_elem.Element("Tags").Add(new XElement("Tag", tag.Name));
                                }
                            }

                            if (project_elem.Element("Authors") != null)
                            {
                                project_elem.Element("Authors").RemoveNodes();
                            }
                            else
                            {
                                project_elem.Add(new XElement("Authors"));
                            }

                            if (project_elem.Element("Authors") != null && project.Authors.Count != 0)
                            {
                                foreach (string author in project.Authors)
                                {
                                    project_elem.Element("Authors").Add(new XElement("Author", author));
                                }
                            }
#pragma warning restore
                        }
                    }
                    ;
                });
                _InternalSave += _ProjectsEvent[project.Name];

                ProjectsListEdited?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public bool TryGetProject(string project_name, out Project? project)
        {
            return _Projects.TryGetValue(project_name, out project);
        }

        /// <summary>
        /// Save all projects data.
        /// Note that if the current project is edited, it will be saved on each edit in the memory, but this function still need to be called.
        /// </summary>
        public override void Save(string filepath = "")
        {
            _InternalSave?.Invoke(this, EventArgs.Empty);

            foreach (Project project in _Projects.Values)
            {
                //project.AssetsConf.Save(filepath);
            }

            Config.GetValueOrDefault().Document.Save(Config.GetValueOrDefault().Filepath);
        }

        /// <summary>
        /// This class should NEVER be created manually!
        /// </summary>
        public class _TagsConf
        {
            readonly Dictionary<string, Tag> _Tags = [];

            private bool _ParsedTags = false;

            public bool HasParsedTags => _ParsedTags;

            public _TagsConf() { }

            public _TagsConf(CONFIG config)
            {
                if (config.ClassType == typeof(ProjectsConf))
                {
                    XDocument document = config.Document;
                    if (document == default)
                    {
                        throw new Exception("Document is default or null.");
                    }

                    if (document.Root == null)
                    {
                        throw new Exception("Document root is null.");
                    }

                    XElement root = document.Root;

                    if (root.Element("Tags") != null)
                    {
                        ParseTags(root.Element("Tags")!);
                    }
                }
            }

            protected void ParseTags(XElement tags_elem)
            {
                foreach (XElement tag_elem in tags_elem.Elements().Where(e => e.Name == "Tag").ToArray())
                {
                    if (!tag_elem.HasElements)
                        continue;

                    if (tag_elem.Element("Name") == null || tag_elem.Element("Color") == null)
                        continue;

                    string tag_name = tag_elem.Element("Name")!.Value;
                    Color tag_color = new();

                    if (Color.TryParse(tag_elem.Element("Color")!.Value, out tag_color))
                    {
                        Tag tag = new Tag(tag_name, tag_color);

                        _Tags.Add(tag_name, tag);

                        tag.LinkXML(tag_elem);
                    }
                }
                _ParsedTags = true;
            }
            /// <summary>
            /// Return the tag if it exist, null otherwise
            /// </summary>
            /// <param name="tagName"></param>
            /// <param name="tag"></param>
            /// <returns>True if it exist and can be getted, false otherwise</returns>
            public bool TryGet(string tagName, out Tag? tag)
            {
                return _Tags.TryGetValue(tagName, out tag);
            }

            /// <summary>
            /// Check if a tag exist.
            /// </summary>
            /// <param name="tagName"></param>
            /// <returns>True if it exist, false otherwise</returns>
            public bool Has(string tagName)
            {
                return _Tags.ContainsKey(tagName);
            }

            /// <summary>
            /// Try addind a new tag to the configuration. The tag name NEED to be unique.
            /// </summary>
            /// <param name="tag"></param>
            /// <returns>True if it was added, false otherwise</returns>
            public bool TryAdd(Tag tag)
            {
                return _Tags.TryAdd(tag.Name, tag);
            }
        }
    }
}

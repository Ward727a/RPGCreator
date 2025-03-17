using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace RPGCreator.core.io.datas
{
    /// <summary>
    /// A class that manage all the config (.xml) file of the editor.
    /// </summary>
    static public class ConfigFile
    {

        static readonly public Editor editor = new();
        static readonly public Plugins plugins = new();
        static readonly public Projects projects = new();

        public abstract class _XML_DOC_READER
        {
            protected abstract string GetClassName();
            protected abstract string GetXML();
            protected abstract string GetXSD();

            private XDocument _xDoc;
            private XElement _xRoot;

            protected void NotValidField(string name, string field_name, string additional_info = "")
            {
                Log.Logger.Error($"{GetClassName()} {name} doesn't have a valid \"{field_name}\" field.{(string.IsNullOrEmpty(additional_info) ? "" : $" {additional_info}.")}");
            }

            protected void NotValidFieldValue(string name, string field_name, string field_value, string awaited_type, string additional_info = "")
            {
                Log.Logger.Error($"{GetClassName()} {name} doesn't have a valid \"{field_name}\" field value, expected {awaited_type}, but got \"{field_value}\" value.{(string.IsNullOrEmpty(additional_info) ? "" : $" {additional_info}.")}");
            }

            protected abstract void CreateBaseXML();
            protected bool CheckXSDSchema()
            {
                XmlDocument xmlDoc = new();

                xmlDoc.Load(GetXML());

                XmlSchemaSet schemaSet = new();
                schemaSet.Add(null, GetXSD());

                XmlReaderSettings readerSettings = new();
                readerSettings.Schemas.Add(schemaSet);
                readerSettings.ValidationType = ValidationType.Schema;

                int error = 0;

                readerSettings.ValidationEventHandler += (_, e) =>
                {
                    if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
                    {
                        Log.Logger.Fatal($"XML file {Path.GetFileName(GetXML())} isn't a valid xml file. {e.Message}");
                        error++;
                    }
                };

                using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml), readerSettings))
                {
                    while (reader.Read()) ;
                }
                if (error != 0)
                {

                    return false;
                }
                Log.Logger.Information($"XML file {Path.GetFileName(GetXML())} checked and validated.");
                return true;
            }
            public XDocument GetDoc()
            {
                if (!File.Exists(GetXML()))
                {
                    CreateBaseXML();
                }

                CheckXSDSchema();

                _xDoc ??= XDocument.Load(GetXML());
                _xRoot ??= _xDoc.Root;
                return _xDoc;
            }

            public XElement GetRoot()
            {
                if (_xRoot == null)
                {
                    GetDoc();
                }
                return _xRoot;
            }
        }

        public class Editor
        {
            private string _xml = Path.Combine(BaseContent.Folders.GetConfig(), "editor.xml");

            private XDocument _xDoc;
            private XElement _root;

            private void CreateBaseXML()
            {
                XmlWriterSettings settings = new()
                {
                    Indent = true
                };

                using XmlWriter writer = XmlWriter.Create(_xml, settings);

                writer.WriteStartDocument();

                writer.WriteStartElement("Editor");
                writer.WriteAttributeString("version", GlobalData.EditorVersion.ToString());

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            public XDocument GetDoc()
            {
                if(!File.Exists(_xml))
                {
                    CreateBaseXML();
                }

                _xDoc ??= XDocument.Load(_xml);
                _root ??= _xDoc.Root;
                return _xDoc;
            }

            public XElement GetRoot()
            {
                if (_root == null)
                {
                    GetDoc();
                }
                return _root;
            }
        }

        public class Projects : _XML_DOC_READER
        {
            public struct PROJECT_DATA
            {
                public string unique;
                public string path;
            }
            protected override void CreateBaseXML()
            {
                XmlWriterSettings settings = new()
                {
                    Indent = true
                };

                using XmlWriter writer = XmlWriter.Create(_xml, settings);

                writer.WriteStartDocument();

                writer.WriteStartElement("projects");
                writer.WriteAttributeString("version", GlobalData.EditorVersion.ToString());

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            protected override string GetClassName()
            {
                return "Projects";
            }

            protected override string GetXML()
            {
                return _xml;
            }

            protected override string GetXSD()
            {
                return _xsd;
            }
            private string _xml = Path.Combine(BaseContent.Folders.GetConfig(), "projects.xml");
            private string _xsd = Path.Combine(BaseContent.Folders.GetXSDConfig(), "projects.xsd");

            private IEnumerable<XElement> GetXProjectsItems()
            {
                return GetRoot().Descendants("project");
            }

            private XElement GetXProject(string unique)
            {
                return GetXProjectsItems().Where(x => x.Element("unique").Value == unique).FirstOrDefault();
            }

            public PROJECT_DATA GetProject(string unique)
            {
                XElement xProject = GetXProject(unique);

                PROJECT_DATA projectData = new PROJECT_DATA()
                {
                    unique = xProject.Element("unique").Value,
                    path = xProject.Element("path").Value
                };

                return projectData;
            }

            public bool HasProject(string unique)
            {
                return GetXProject(unique) != default(XElement);
            }

            public void AddProject(string unique, string path)
            {
                GetRoot().Add(new XElement("project", [new XElement("unique", unique), new XElement("path", path)]));
                GetDoc().Save(_xml);
            }

            public void RemoveProject(string unique)
            {
                GetXProject(unique).Remove();
                GetDoc().Save(_xml);
            }

            /// <summary>
            /// Return an array of all projects that can be found inside the data.
            /// </summary>
            /// <param name="error">True if there was an error, false otherwise.</param>
            /// <returns></returns>
            public PROJECT_DATA[] GetProjects(out bool error)
            {
                error = false;
                int total_items = GetXProjectsItems().Count();
                PROJECT_DATA[] projects = new PROJECT_DATA[total_items];

                int index = 0;
                foreach(XElement items in GetXProjectsItems())
                {
                    PROJECT_DATA data = new();

                    string unique = items.Element("unique").Value;
                    string path = items.Element("path").Value;

                    if(path == string.Empty || unique == string.Empty) // This should never happen as the file is generated by the engine.
                    {
                        error = true;
                        continue;
                    }

                    data.path = path;
                    data.unique = unique;

                    projects[index] = data;

                    index++;
                }

                if(projects.Length != index) // This should be false everytime.
                {
                    error = true;
                }

                return projects;
            }
        }

        public class Plugins
        {

            private string _xml = Path.Combine(BaseContent.Folders.GetConfig(), "plugins.xml");
            private string _xsd = Path.Combine(BaseContent.Folders.GetXSDConfig(), "plugins.xsd");

            // Errors helper
            private void NotValidField(string name, string field_name, string additional_info = "")
            {
                Log.Logger.Error($"Plugin {name} doesn't have a valid \"{field_name}\" field.{(string.IsNullOrEmpty(additional_info) ? "" : $" {additional_info}.")}");
            }
            
            private void NotValidFieldValue(string name, string field_name, string field_value, string awaited_type, string additional_info = "")
            {
                Log.Logger.Error($"Plugin {name} doesn't have a valid \"{field_name}\" field value, expected {awaited_type}, but got \"{field_value}\" value.{(string.IsNullOrEmpty(additional_info) ? "" : $" {additional_info}.")}");
            }

            private XDocument _xDoc;
            private XElement _root;

            private void CreateBaseXML()
            {
                XmlWriterSettings settings = new()
                {
                    Indent = true
                };

                using XmlWriter writer = XmlWriter.Create(_xml, settings);

                writer.WriteStartDocument();

                writer.WriteStartElement("plugins");
                writer.WriteAttributeString("version", GlobalData.EditorVersion.ToString());

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            private bool CheckXSDSchema()
            {
                XmlDocument xmlDoc = new();
                
                xmlDoc.Load(_xml);

                XmlSchemaSet schemaSet = new();
                schemaSet.Add(null, _xsd);

                XmlReaderSettings readerSettings = new();
                readerSettings.Schemas.Add(schemaSet);
                readerSettings.ValidationType = ValidationType.Schema;

                int error = 0;

                readerSettings.ValidationEventHandler += (_, e) =>
                {
                    if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
                    {
                        Log.Logger.Fatal($"XML file {Path.GetFileName(_xml)} isn't a valid xml file. {e.Message}");
                        error++;
                    }
                };

                using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml), readerSettings))
                {
                    while (reader.Read()) ;
                }
                if (error != 0)
                {
                    
                    return false;
                }
                Log.Logger.Information($"XML file {Path.GetFileName(_xml)} checked and validated.");
                return true;
            }

            public XDocument GetDoc()
            {
                if (!File.Exists(_xml))
                {
                    CreateBaseXML();
                }

                CheckXSDSchema();

                _xDoc ??= XDocument.Load(_xml);
                _root ??= _xDoc.Root;
                return _xDoc;
            }

            public XElement GetRoot()
            {
                if (_root == null)
                {
                    GetDoc();
                }
                return _root;
            }

            private IEnumerable<XElement> GetXPluginsItems()
            {
                return GetRoot().Descendants("plugin");
            }

            private XElement GetXPlugin(string name)
            {
                return GetXPluginsItems().Where(x => (string)x.Element("unique").Value == name).FirstOrDefault();
            }

            private IEnumerable<XElement> GetXPluginsEnabledItems()
            {
                return GetXPluginsItems().Where(x => {
                    if(IsEnabled(x.Element("unique").Value))
                    {
                        if(IsOutdated(x.Element("unique").Value))
                        {
                            return false;
                        }
                        return true;
                    }
                    return false;
                    });
            }

            private IEnumerable<XElement> GetXPluginsDisabledItems()
            {
                return GetXPluginsItems().Where(x => {
                    return !IsEnabled(x.Element("unique").Value);
                });
            }

            private bool HasXElement(XElement value, string element)
            {
                if (value.HasElements)
                {
                    return value.Elements().Any(x => x.Name == element);
                }
                return false;
            }

            public bool HasPlugin(string name)
            {
                if(GetXPlugin(name) == default(XElement))
                {
                    return false;
                }
                return true;
            }

            public int GetPluginsCount()
            {
                return GetXPluginsItems().Count();
            }
            
            public int GetEnabledPluginsCount()
            {
                return GetXPluginsEnabledItems().Count();
            }

            public int GetDisabledPluginsCount()
            {
                return GetXPluginsDisabledItems().Count();
            }

            public string[] GetPluginsUnique()
            {
                return GetXPluginsItems().Select(x => x.Element("unique").Value).ToArray();
            }

            public string GetPluginName(string unique)
            {
                return GetXPlugin(unique).Element("name").Value;
            }

            public string[] GetPluginsName()
            {
                return GetXPluginsItems().Select(x => x.Element("name").Value).ToArray();
            }

            public bool IsEnabled(string unique)
            {
                if(!HasXElement(GetXPlugin(unique), "enabled"))
                {
                    return false;
                }
                if(bool.TryParse(GetXPlugin(unique).Element("enabled").Value, out bool result))
                {
                    return result;
                }

                NotValidFieldValue(unique, "enabled", GetXPlugin(unique).Element("enabled").Value, "bool (true or false)");
                return false;
            }

            public bool IsOutdated(string unique)
            {
                return !GetEditorVersion(unique).Contains(GlobalData.EditorVersion.ToString());
            }

            public string[] GetEditorVersion(string unique)
            {
                XElement pluginVersion = GetXPlugin(unique).Element("supported-versions");
                if(pluginVersion.HasElements)
                {
                    return pluginVersion.Elements().Select(x => x.Value).ToArray();
                }

                if(!string.IsNullOrWhiteSpace(pluginVersion.Value))
                {
                    return [pluginVersion.Value];
                }

                NotValidField(unique, "supported-versions", "Not parent of \"<version>\" elements");
                return [];
            }

            public string GetPluginVersion(string unique)
            {
                return GetXPlugin(unique).Element("version").Value;
            }

            public string[] GetPluginAuthors(string unique)
            {
                XElement pluginAuthors = GetXPlugin(unique).Element("authors");
                if (pluginAuthors.HasElements)
                {
                    return pluginAuthors.Elements().Select(x => x.Value).ToArray();
                }

                NotValidField(unique, "authors", "Not parent of \"<author>\" elements");
                return [];
            }

            public string GetPluginDescription(string unique)
            {
                XElement plugin = GetXPlugin(unique);
                if(HasXElement(plugin, "description"))
                {
                    return plugin.Element("description").Value;
                }
                return $"No description provided by {unique} plugin.";
            }

            public string[] GetPluginDependencies(string unique)
            {
                XElement plugin = GetXPlugin(unique);
                if(HasXElement(plugin, "dependencies") && plugin.Element("dependencies").HasElements)
                {
                    XElement DependenciesElem = plugin.Element("dependencies");

                    if(HasXElement(DependenciesElem, "plugin"))
                    {
                        return DependenciesElem.Elements("plugin").Select(x => x.Value).ToArray();
                    }
                }
                return [];
            }

            public bool HasPluginDependencies(string unique)
            {
                return GetPluginDependencies(unique).Length > 0;
            }

            public string[] GetSupportedLanguages(string unique)
            {
                XElement plugin = GetXPlugin(unique);
                if(HasXElement(plugin, "languages") && plugin.Element("languages").HasElements)
                {
                    XElement LanguagesElem = plugin.Element("languages");

                    if(HasXElement(LanguagesElem, "lang"))
                    {
                        return LanguagesElem.Elements("lang").Select(x => x.Value).ToArray();
                    }
                }
                return [];
            }

            public bool HasSupportForLang(string unique, string lang)
            {
                return GetSupportedLanguages(unique).Contains(lang);
            }

            public string GetConfigFilePath(string unique)
            {
                XElement plugin = GetXPlugin(unique);
                if(HasXElement(plugin, "config"))
                {
                    return plugin.Element("config").Value;
                }
                NotValidField(unique, "config", "Expected to find it, but no config element could be found.");
                return "";
            }

            public string GetPluginRoot(string unique)
            {
                string configPath = GetConfigFilePath(unique);
                if (string.IsNullOrEmpty(configPath))
                {
                    return "";
                }
                return configPath.Replace(Path.GetFileName(configPath), "");
            }

            public bool AddPlugin(XDocument pluginDocument, string path_to_config, bool is_enabled = false)
            {
                pluginDocument.Root.Add(new XElement("enabled", is_enabled));
                pluginDocument.Root.Add(new XElement("config", path_to_config));
                GetRoot().Add(pluginDocument.Root);
                GetDoc().Save(_xml);
                return true;
            }

            public void RemovePlugin(string unique)
            {
                GetXPlugin(unique).Remove();
                GetDoc().Save(_xml);
            }
        }
    }
}

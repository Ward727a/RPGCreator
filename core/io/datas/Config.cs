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

                writer.WriteStartElement("Plugins");
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
                return GetRoot().Descendants("Plugin");
            }

            private XElement GetXPlugin(string name)
            {
                return GetXPluginsItems().Where(x => (string)x.Element("Unique").Value == name).FirstOrDefault();
            }

            private IEnumerable<XElement> GetXPluginsEnabledItems()
            {
                return GetXPluginsItems().Where(x => {
                    if(IsEnabled(x.Element("Unique").Value))
                    {
                        if(IsOutdated(x.Element("Unique").Value))
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
                    return !IsEnabled(x.Element("Unique").Value);
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
                return GetXPluginsItems().Select(x => x.Element("Unique").Value).ToArray();
            }

            public string GetPluginName(string unique)
            {
                return GetXPlugin(unique).Element("Name").Value;
            }

            public string[] GetPluginsName()
            {
                return GetXPluginsItems().Select(x => x.Element("Name").Value).ToArray();
            }

            public bool IsEnabled(string unique)
            {
                if(!HasXElement(GetXPlugin(unique), "Enabled"))
                {
                    return false;
                }
                if(bool.TryParse(GetXPlugin(unique).Element("Enabled").Value, out bool result))
                {
                    return result;
                }

                NotValidFieldValue(unique, "Enabled", GetXPlugin(unique).Element("Enabled").Value, "bool (true or false)");
                return false;
            }

            public bool IsOutdated(string unique)
            {
                return !GetEditorVersion(unique).Contains(GlobalData.EditorVersion.ToString());
            }

            public string[] GetEditorVersion(string unique)
            {
                XElement pluginVersion = GetXPlugin(unique).Element("EditorVersions");
                if(pluginVersion.HasElements)
                {
                    return pluginVersion.Elements().Select(x => x.Value).ToArray();
                }

                if(!string.IsNullOrWhiteSpace(pluginVersion.Value))
                {
                    return [pluginVersion.Value];
                }

                NotValidField(unique, "EditorVersion", "Not parent of \"<Version>\" elements");
                return [];
            }

            public string GetPluginVersion(string unique)
            {
                return GetXPlugin(unique).Element("Version").Value;
            }

            public string[] GetPluginAuthors(string unique)
            {
                XElement pluginAuthors = GetXPlugin(unique).Element("Authors");
                if (pluginAuthors.HasElements)
                {
                    return pluginAuthors.Elements().Select(x => x.Value).ToArray();
                }

                NotValidField(unique, "Authors", "Not parent of \"<Author>\" elements");
                return [];
            }

            public string GetPluginDescription(string unique)
            {
                XElement plugin = GetXPlugin(unique);
                if(HasXElement(plugin, "Description"))
                {
                    return plugin.Element("Description").Value;
                }
                return $"No description provided by {unique} plugin.";
            }

            public string[] GetPluginDependencies(string unique)
            {
                XElement plugin = GetXPlugin(unique);
                if(HasXElement(plugin, "Dependencies") && plugin.Element("Dependencies").HasElements)
                {
                    XElement DependenciesElem = plugin.Element("Dependencies");

                    if(HasXElement(DependenciesElem, "Plugin"))
                    {
                        return DependenciesElem.Elements("Plugin").Select(x => x.Value).ToArray();
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
                if(HasXElement(plugin, "Languages") && plugin.Element("Languages").HasElements)
                {
                    XElement LanguagesElem = plugin.Element("Languages");

                    if(HasXElement(LanguagesElem, "Lang"))
                    {
                        return LanguagesElem.Elements("Lang").Select(x => x.Value).ToArray();
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
                if(HasXElement(plugin, "Config"))
                {
                    return plugin.Element("Config").Value;
                }
                NotValidField(unique, "Config", "Expected to find it, but no Config element could be found.");
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
        }
    }
}

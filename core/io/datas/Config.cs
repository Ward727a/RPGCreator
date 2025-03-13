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
    /// A class that manage all the config file of the editor.
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

                writer.WriteStartElement("editor");
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
                return GetXPluginsItems().Where(x => (string)x.Element("Name").Value == name).FirstOrDefault();
            }

            private IEnumerable<XElement> GetXPluginsEnabledItems()
            {
                return GetXPluginsItems().Where(x => {
                    return IsEnabled(x.Element("Name").Value);
                    });
            }

            private IEnumerable<XElement> GetXPluginsDisabledItems()
            {
                return GetXPluginsItems().Where(x => {
                    return !IsEnabled(x.Element("Name").Value);
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

            public string[] GetPluginsName()
            {
                return GetXPluginsItems().Select(x => x.Element("Name").Value).ToArray();
            }

            public string[] GetEnabledName()
            {
                return GetXPluginsEnabledItems().Select(x => x.Element("Name").Value).ToArray();
            }

            public bool IsEnabled(string name)
            {
                if(bool.TryParse(GetXPlugin(name).Element("Enabled").Value, out bool result))
                {
                    return result;
                }

                NotValidFieldValue(name, "Enabled", GetXPlugin(name).Element("Enabled").Value, "bool (true or false)");
                return false;
            }

            public bool IsOutdated(string name)
            {
                return GetEditorVersion(name).Contains(GlobalData.EditorVersion.ToString());
            }

            public string[] GetEditorVersion(string name)
            {
                XElement pluginVersion = GetXPlugin(name).Element("EditorVersions");
                if(pluginVersion.HasElements)
                {
                    return pluginVersion.Elements().Select(x => x.Value).ToArray();
                }

                if(!string.IsNullOrWhiteSpace(pluginVersion.Value))
                {
                    return [pluginVersion.Value];
                }

                NotValidField(name, "EditorVersion", "Not parent of \"<Version>\" elements");
                return [];
            }

            public string GetPluginVersion(string name)
            {
                return GetXPlugin(name).Element("version").Value;
            }

            public string[] GetPluginAuthors(string name)
            {
                XElement pluginAuthors = GetXPlugin(name).Element("Authors");
                if (pluginAuthors.HasElements)
                {
                    return pluginAuthors.Elements().Select(x => x.Value).ToArray();
                }

                NotValidField(name, "Authors", "Not parent of \"<Author>\" elements");
                return [];
            }

            public string GetPluginDescription(string name)
            {
                XElement plugin = GetXPlugin(name);
                if(HasXElement(plugin, "Description"))
                {
                    return plugin.Element("Description").Value;
                }
                return $"No description provided by {name} plugin.";
            }
        }
    }
}

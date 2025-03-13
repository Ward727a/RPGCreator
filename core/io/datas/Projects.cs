using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RPGCreator.core.io.datas
{
    static public class Projects
    {

        private static readonly string _xml = Path.Combine(BaseContent.Folders.GetProjects(), "projects.xml");
        static private XDocument _xDoc;
        static private XElement _root;

        static private void CreateBaseXML()
        {
            XmlWriterSettings settings = new()
            {
                Indent = true
            };

            using XmlWriter writer = XmlWriter.Create(_xml, settings);

            writer.WriteStartDocument();

            writer.WriteStartElement("projects-list");
            writer.WriteAttributeString("version", GlobalData.EditorVersion.ToString());

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        static public XDocument GetDoc()
        {

            if( !File.Exists(_xml) )
            {
                CreateBaseXML();
            }

            _xDoc ??= XDocument.Load(_xml);
            _root ??= _xDoc.Root;
            return _xDoc;
        }

        static public XElement GetRoot()
        {
            if( _root == null )
            {
                GetDoc();
            }
            return _root;
        }
        static private IEnumerable<XElement> GetXProjectsList()
        {
            return GetRoot().Descendants("project");
        }
        static private XElement GetXProject(string projectName)
        {
            return GetXProjectsList().Where(x => (string)x.Element("name") == projectName).FirstOrDefault();
        }

        static public int GetProjectCount()
        {
            return GetXProjectsList().Count();
        }

        static public bool HasProject(string projectName)
        {
            return GetXProjectsList().Select(x => x.Element("name")?.Value ==  projectName).Any();
        }

        static public bool RemoveProject(string projectName)
        {
            if (!HasProject(projectName))
            {
                Log.Logger.Error($"No project \"{projectName}\" exist.");
                return false;
            }

            if (GetXProject(projectName) != null)
            {
                GetXProject(projectName).Remove();

                GetDoc().Save(_xml);
                return true;
            }
            Log.Logger.Error($"Couldn't remove project \"{projectName}\" due to an unexpected error. Please report it to the developer.");
            return false;
        }
    }
}

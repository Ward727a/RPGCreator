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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static RPGCreator.Core.Services.ConfigurationService;
using System.Xml.Linq;

namespace RPGCreator.Core.Services.Configurations
{/// <summary>
 /// This class should not be manually created, but getted from <see cref="ConfigurationService"/>.<br/>
 /// This class manage all configuration related to the app.conf.xml file.
 /// </summary>
    public class AppConf() : ConfHelper
    {
        public PathConfig path;

        static public Version EditorVersion { get; private set; } = Assembly.GetExecutingAssembly().GetName().Version;

        private event EventHandler? _InternalSave;

        public override void OnLoadedConf()
        {
            if (Config != null)
            {
                ParseConfig(Config.GetValueOrDefault());
                path = new PathConfig(this, Config.GetValueOrDefault());
            }
            else
            {
                throw new Exception("Config is null!");
            }
        }

        private void ParseConfig(CONFIG config)
        {
            if (config.Document.Root != null)
            {
                XElement root = config.Document.Root;

            }
        }

        public override void Save(string filepath = "")
        {
            _InternalSave?.Invoke(this, EventArgs.Empty);
        }

        public class PathConfig()
        {

            string Projects;
            string Style;
            string Assets;
            AppConf Conf;

            public PathConfig(AppConf conf, CONFIG config) : this()
            {

                if (config.Document.Root == null || config.Document.Root.Element("ConfigPath") == null)
                {
                    throw new Exception("Document ROOT or ConfigPath element is null!");
                }

                Conf = conf;

                XElement? config_path = config.Document.Root.Element("ConfigPath");

                if (config_path == null) // This should not happen with the check before it, but it remove a warning.
                    return;

                AddPath(config_path, "Projects", "%APPDATA%/Configs/", ref Projects);
                AddPath(config_path, "Style", "%APPDATA%/Configs/", ref Style);
                AddPath(config_path, "Assets", "%APPDATA%/Configs/", ref Assets);
            }

            protected void AddPath(XElement config_path, string element_name, string default_value, ref string path_variable)
            {
                path_variable = FormatPath(config_path.Element(element_name)?.Value ?? default_value);

                if (Path.IsPathRooted(path_variable))
                {
                    if (!Directory.Exists(path_variable))
                    {
                        Directory.CreateDirectory(path_variable);
                    }
                }

                string data_to_save = path_variable;

                Conf._InternalSave += (_, _) => {
                    if (config_path.Element(element_name) != null)
                    {
                        config_path.Element(element_name)!.Value = data_to_save;
                    }
                    else
                    {
                        config_path.Add(new XElement(element_name, data_to_save));
                    }
                };
            }

            protected static string FormatPath(string path)
            {
                return path.Replace("%APPDATA%", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator")).Replace("%APPDIR%", Directory.GetCurrentDirectory());
            }

            public string GetProject()
            {
                return Projects;
            }
            public void SetProject(string new_path)
            {
                Projects = new_path;
            }

            public string GetStyle()
            {
                return Style;
            }

            public void SetStyle(string new_path)
            {
                Style = new_path;
            }

            public string GetAssets()
            {
                return Assets;
            }

            public void SetAssets(string new_path)
            {
                Assets = new_path;
            }

        }

    }
}

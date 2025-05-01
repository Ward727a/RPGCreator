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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static RPGCreator.Core.Configs.EngineConfigs;

namespace RPGCreator.Core.Configs.Helpers
{
    public class AppConf : ConfHelper
    {
        public struct SAppConfPath
        {
            public string BaseFolder { get; set; }
            public string AppDataFolder { get; set; }
            public string AssetsFolder { get; set; }
            public string StyleFolder { get; set; }
            public string LogsFolder { get; set; }
            public string ProjectsFolder { get; set; }
        }

        private XElement _root;

        public SAppConfPath ConfigPath = new();


        public override void LoadConfig()
        {

            if(Doc.Root == null)
            {
                throw new Exception("Root is null");
            }
            _root = Doc.Root;
            LoadFolders();
        }

        protected virtual void LoadFolders()
        {
            if (_root.Element("ConfigPath") == null)
                return;

            XElement confPathElem = _root.Element("ConfigPath")!;

            // Base folder is inside the folder where the .exe is located
            ConfigPath.BaseFolder = confPathElem.Element("Base")?.Value ?? AppDomain.CurrentDomain.BaseDirectory;

            if (string.IsNullOrEmpty(ConfigPath.BaseFolder))
                throw new Exception("BaseFolder is null or empty");

            ConfigPath.AppDataFolder = confPathElem.Element("Appdata")?.Value ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), EngineData.AppName);

            ConfigPath.ProjectsFolder = FormatPath(confPathElem.Element("Projects")?.Value ?? string.Empty);
            ConfigPath.StyleFolder = FormatPath(confPathElem.Element("Style")?.Value ?? string.Empty);
            ConfigPath.AssetsFolder = FormatPath(confPathElem.Element("Assets")?.Value ?? string.Empty);
            ConfigPath.LogsFolder = FormatPath(confPathElem.Element("Logs")?.Value ?? string.Empty);

        }

        protected string FormatPath(string unformatted_path)
        {
            return unformatted_path.Replace("%BASE_FOLDER%", ConfigPath.BaseFolder).Replace("%APPDATA%", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), EngineData.AppName));
        }

        public override void Save()
        {
            throw new NotImplementedException();
        }

        
    }
}

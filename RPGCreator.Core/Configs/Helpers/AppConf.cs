#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
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

using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using static RPGCreator.Core.Configs.EngineConfigs;

namespace RPGCreator.Core.Configs.Helpers
{
    [SerializingType("AppConf")]
    public class AppConf : ConfHelper
    {
        public override string ConfigName { get; set; } = "AppConf";
        
        public struct SAppConfPath()
        {
            public string BaseFolder = AppDomain.CurrentDomain.BaseDirectory;
            public string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GlobalStates.ApplicationName);
            public string AssetsFolder = $"%APPDATA%/Assets";
            public string StyleFolder = $"%APPDATA%/Style";
            public string LogsFolder = $"%APPDATA%/Logs";
            public string ProjectsFolder = $"%APPDATA%/Projects";
        }

        public SAppConfPath Paths = new();

        public override void LoadConfig()
        {
            base.LoadConfig();
            if (!Directory.Exists(Paths.BaseFolder))
            {
                Directory.CreateDirectory(Paths.BaseFolder);
            }
            if(!Directory.Exists(Paths.AppDataFolder))
            {
                Directory.CreateDirectory(Paths.AppDataFolder);
            }
            if (!Directory.Exists(Paths.AssetsFolder))
            {
                Directory.CreateDirectory(Paths.AssetsFolder);
            }
            if (!Directory.Exists(Paths.StyleFolder))
            {
                Directory.CreateDirectory(Paths.StyleFolder);
            }
            if (!Directory.Exists(Paths.LogsFolder))
            {
                Directory.CreateDirectory(Paths.LogsFolder);
            }
            if (!Directory.Exists(Paths.ProjectsFolder))
            {
                Directory.CreateDirectory(Paths.ProjectsFolder);
            }
        }

        private string FormatPath(string unformattedPath)
        {
            return unformattedPath.Replace("%BASE_FOLDER%", Paths.BaseFolder).Replace("%APPDATA%", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GlobalStates.ApplicationName));
        }
        
        private string UnformatPath(string formattedPath)
        {
            return formattedPath.Replace(Paths.BaseFolder, "%BASE_FOLDER%").Replace(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GlobalStates.ApplicationName), "%APPDATA%");
        }

        public override SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(AppConf));
            info.AddValue("base_folder", UnformatPath(Paths.BaseFolder));
            info.AddValue("appdata_folder", UnformatPath(Paths.AppDataFolder));
            info.AddValue("assets_folder", UnformatPath(Paths.AssetsFolder));
            info.AddValue("style_folder", UnformatPath(Paths.StyleFolder));
            info.AddValue("logs_folder", UnformatPath(Paths.LogsFolder));
            info.AddValue("projects_folder", UnformatPath(Paths.ProjectsFolder));
            return info;
        }

        public override List<Ulid> GetReferencedAssetIds()
        {
            return new List<Ulid>();
        }

        public override void SetObjectData(DeserializationInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
            }

            info.TryGetValue("base_folder", out Paths.BaseFolder!, AppDomain.CurrentDomain.BaseDirectory);
            info.TryGetValue("appdata_folder", out Paths.AppDataFolder!, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GlobalStates.ApplicationName));
            info.TryGetValue("assets_folder", out Paths.AssetsFolder!, string.Empty);
            info.TryGetValue("style_folder", out Paths.StyleFolder!, string.Empty);
            info.TryGetValue("logs_folder", out Paths.LogsFolder!, string.Empty);
            info.TryGetValue("projects_folder", out Paths.ProjectsFolder!, string.Empty);
            
            Paths.BaseFolder = FormatPath(Paths.BaseFolder);
            Paths.AppDataFolder = FormatPath(Paths.AppDataFolder);
            Paths.AssetsFolder = FormatPath(Paths.AssetsFolder);
            Paths.StyleFolder = FormatPath(Paths.StyleFolder);
            Paths.LogsFolder = FormatPath(Paths.LogsFolder);
            Paths.ProjectsFolder = FormatPath(Paths.ProjectsFolder);
            
        }
    }
}

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
using RPGCreator.Core.Configs.EventsArgs;
using RPGCreator.Core.Configs.Helpers;

namespace RPGCreator.Core.Configs
{
    public class EngineConfigs
    {
        public EngineConfigEvents Events { get; private set; }

        private Dictionary<string, ConfHelper> LoadedConfig = [];
        private Dictionary<string, string> ConfigMap = []; // Map of the config path to the config name

        internal EngineConfigs()
        {
            Events = new EngineConfigEvents();
            // Load the default config file that should be inside the folder where the .exe is
            var t = AppDomain.CurrentDomain.BaseDirectory;
            LoadOrCreateConfig<AppConf>(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.conf.xml")
            );

            LoadOrCreateConfig<ProjectsConf>(
                Path.Combine(((AppConf)LoadedConfig[ConfigMap["AppConf"]]).Paths.ProjectsFolder, "Projects.conf.xml")
            );
        }
        
        public T? LoadOrCreateConfig<T>(string configPath) where T : ConfHelper, new()
        {
            if (LoadedConfig.ContainsKey(configPath))
            {
                if (LoadedConfig[configPath] is T conf)
                {
                    return conf;
                }
                throw new InvalidOperationException($"Config at {configPath} is not of type {typeof(T).Name}.");
            }

            if (TryLoadConfig(configPath, out ConfHelper? outConf, out System.Type? outConfType))
            {
                if(outConfType == null || !typeof(T).IsAssignableFrom(outConfType))
                {
                    throw new InvalidOperationException($"Config at {configPath} is not of type {typeof(T).Name}.");
                }
                return (T)outConf;
            }

            if (File.Exists(configPath))
            {
                throw new InvalidOperationException($"Config at {configPath} does not contain valid data or is not of type {typeof(T).Name}.");
            }
            
            var newConf = new T();
            EngineSerializer.Instance.Serialize(newConf, out var data, false);
            if (string.IsNullOrEmpty(data))
            {
                throw new InvalidOperationException($"Config at {configPath} is not valid.");
            }
            
            try
            {
                File.WriteAllText(configPath, data);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create config at {configPath}: {ex.Message}");
            }
            newConf.ConfigPath = configPath;
            AddConfig(configPath, newConf);
            newConf.LoadConfig();
            return newConf;
        }

        private void AddConfig(string configPath, ConfHelper conf)
        {
            LoadedConfig[configPath] = conf;
            ConfigMap[conf.ConfigName] = configPath;
        }
        
        public bool TryLoadConfig(string configPath, out ConfHelper? confObject, out System.Type? confType, bool force = false)
        {

            confObject = null;
            confType = null;
            if (!File.Exists(configPath))
            {
                return false;
            }

            if (LoadedConfig.ContainsKey(configPath))
            {
                confObject = LoadedConfig[configPath];
                confType = confObject.GetType();
                return true;
            }
            
            var data = File.ReadAllText(configPath);
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }
            EngineSerializer.Instance.Deserialize(data, out var o, out var t);

            if (o == null || t == null)
                return false;

            if (o is ConfHelper conf)
            {
                confObject = conf;
                confType = t;
                confObject.ConfigPath = configPath;
                confObject.LoadConfig();
                AddConfig(configPath, confObject);
                return true;
            }

            return false;
        }
        
        // OLD CODE - kept for reference
        // public bool LoadConfig(string configPath)
        // {
        //
        //     if(ConfigMap.ContainsKey(configPath))
        //         return true;
        //
        //     var doc = XDocument.Load(configPath);
        //
        //     if (doc.Root == null)
        //         return false;
        //
        //     if (doc.Root.Attribute("configName") == null ||
        //         string.IsNullOrEmpty(doc.Root.Attribute("configName")!.Value))
        //     {
        //         doc.Root.SetAttributeValue("configName", Path.GetFileName(configPath).Split(".")[0]);
        //     }
        //
        //     if (doc.Root.Attribute("configClass") == null ||
        //         string.IsNullOrEmpty(doc.Root.Attribute("customClass")!.Value))
        //     {
        //         // LoadedConfig.Add(configPath, new ConfHelper() { Doc = doc, ConfigPath = configPath });
        //         ConfigMap.Add(configPath, Path.GetFileName(configPath).Split(".")[0]);
        //         return true;
        //     }
        //     else
        //     {
        //         string customClass = doc.Root.Attribute("customClass")!.Value;
        //         System.Type? type = System.Type.GetType(customClass);
        //         if (type == null)
        //             return false;
        //         if (!typeof(ConfHelper).IsAssignableFrom(type))
        //             return false;
        //         object? obj = Activator.CreateInstance(type);
        //         if (obj == null)
        //             return false;
        //         ConfHelper conf = (ConfHelper)obj;
        //         conf.SetType(type);
        //         conf.ConfigPath = configPath;
        //         conf.LoadConfig();
        //         LoadedConfig.Add(configPath, conf);
        //         ConfigMap.Add(configPath, Path.GetFileName(configPath).Split(".")[0]);
        //         return true;
        //     }
        // }
        //
        // public ConfHelper? GetConfig(string configName)
        // {
        //     if (LoadedConfig.ContainsKey(configName))
        //     {
        //         return LoadedConfig[configName];
        //     }
        //     else
        //     {
        //         return null;
        //     }
        // }

        public T? GetConfig<T>(string configName) where T : ConfHelper
        {
            if (LoadedConfig.ContainsKey(configName))
            {
                if (LoadedConfig[configName] is T conf)
                {
                    return conf;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public string GetPath(string config)
        {
            var mapList = ConfigMap.Where(cm => cm.Value == config).ToList();

            if (mapList.Count == 1)
                return mapList[0].Key;
            else
                return "";
        }

        public bool Save(string config)
        {
            var preArgs = new SavingConfigArgs(config);
            Events.OnSavingConfig(preArgs);

            if(preArgs.Cancel)
                return false;

            config = preArgs.ConfigName;

            if (LoadedConfig.TryGetValue(config, out var conf))
            {
                conf.Save();
                Events.OnSavedConfig(preArgs.ToPost());
                return true;
            }
            Events.OnSavedConfig(preArgs.ToPost().SetError(true, $"Config {config} is not found in the LoadedConfig."));
            return false;

        }

        public bool SaveAll()
        {
            foreach (var config in LoadedConfig.Keys)
            {
                if(!Save(config))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Export(string config, string export_path)
        {
            var preArgs = new ExportingConfigArgs(config, export_path);
            Events.OnExportingConfig(preArgs);

            if (preArgs.Cancel)
                return false;

            config = preArgs.ConfigName;
            export_path = preArgs.ExportPath;

            if (LoadedConfig.ContainsKey(config))
            {
                LoadedConfig[config].Save(export_path);
                Events.OnExportedConfig(preArgs.ToPost());
                return true;
            }
            else
            {
                Events.OnExportedConfig(preArgs.ToPost().SetError(true, $"Config {config} is not found in the LoadedConfig."));
                return false;
            }
        }

        /// <summary>
        /// This class should ONLY be inherited for config helper.
        /// </summary>
        public abstract class ConfHelper : ISerializable, IDeserializable
        {
            public string ConfigPath = "";
            public abstract string ConfigName { get; set; }

            public void Save(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return;
                
                EngineSerializer.Instance.Serialize(this, out var data, false);
                if (string.IsNullOrEmpty(data))
                    throw new InvalidOperationException("Data is null or empty. Cannot save config.");
                try
                {
                    File.WriteAllText(path, data);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save config at {path}: {ex.Message}");
                }
            }

            public virtual void LoadConfig() { }

            public void Save()
            {
                if (string.IsNullOrEmpty(ConfigPath))
                    throw new InvalidOperationException("ConfigPath is not set. Please set it before saving.");

                Save(ConfigPath);
            }
            public abstract SerializationInfo GetObjectData();
            public abstract void SetObjectData(SerializationInfo info);
        }
    }
}

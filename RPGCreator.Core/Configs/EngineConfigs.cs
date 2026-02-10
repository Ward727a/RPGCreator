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
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.SDK;
using RPGCreator.SDK.Serializer;
using Serilog;

namespace RPGCreator.Core.Configs
{
    public class EngineConfigs
    {

        private Dictionary<string, ConfHelper> LoadedConfig = [];
        private Dictionary<string, string> ConfigMap = []; // Map of the config path to the config name

        internal EngineConfigs()
        {
            // Load the default config file that should be inside the folder where the .exe is
            var t = AppDomain.CurrentDomain.BaseDirectory;
            LoadOrCreateConfig<AppConf>(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.conf")
            );

            LoadOrCreateConfig<ProjectsConf>(
                Path.Combine(((AppConf)LoadedConfig[ConfigMap["AppConf"]]).Paths.ProjectsFolder, "Projects.conf")
            );
            
            Log.Information($"EngineConfigs initialized.");
        }
        
        public T? LoadOrCreateConfig<T>(string configPath) where T : ConfHelper, new()
        {
            
            Log.Information("[EngineConfig] Loading configuration at {0}", configPath);
            
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
            EngineServices.SerializerService.Serialize(newConf, out var data);
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
            Log.Information($"Config {configPath} loaded/created successfully.");
            return newConf;
        }

        private void AddConfig(string configPath, ConfHelper conf)
        {
            var status = EngineDB.AddConfig(new EngineDB.DatabaseFileData()
            {
                FilePath = configPath,
                LastModified = File.GetLastWriteTimeUtc(configPath)
            });
            
            if(status != EngineDB.EFileInsertStatus.Success)
            {
                Log.Error($"Failed to add config {configPath} to EngineDB. Status: {status}");
            }
            
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
            EngineServices.SerializerService.Deserialize<ConfHelper>(data, out var o, out var t);

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
            if (ConfigMap.TryGetValue(configName, out var configPath))
            {
                if (TryLoadConfig(configPath, out var confObject, out var confType))
                {
                    if (confType == null || !typeof(T).IsAssignableFrom(confType))
                    {
                        return null;
                    }
                    return (T)confObject;
                }
            }
            return null;
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
            if (LoadedConfig.TryGetValue(config, out var conf))
            {
                conf.Save();
                return true;
            }
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

            if (LoadedConfig.ContainsKey(config))
            {
                LoadedConfig[config].Save(export_path);
                return true;
            }
            else
            {
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
                
                EngineServices.SerializerService.Serialize(this, out var data);
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
            public abstract List<Ulid> GetReferencedAssetIds();

            public abstract void SetObjectData(DeserializationInfo info);
        }
    }
}

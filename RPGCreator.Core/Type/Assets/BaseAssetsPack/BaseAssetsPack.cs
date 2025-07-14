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
using RPGCreator.Core.Events;
using RPGCreator.Core.Type.Assets.BaseAssetsPack.EventsArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RPGCreator.Core.Type.Assets.BaseAssetsPack
{
    public class BaseAssetsPack : ISerializable, IDeserializable
    {

        public bool ErrorOnLoad { get; private set; } = false;

        public Dictionary<string, BaseAsset> Assets = [];
        public readonly BaseAssetsPackEvents Events = new();

        public string ConfigPath;
        public XDocument ConfigDocument;

        public enum PACK_TYPE
        {
            UNKNOWN,
            PROJECT,
            PACK
        }

        public string Name;
        public string? Description;
        public PACK_TYPE Type;
        public Ulid Id = Ulid.NewUlid();

        public BaseAssetsPack()
        {
            ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator", "AssetsPacks", $"{Id}.xml");
        }

        public BaseAssetsPack(string configPath)
        {
            ConfigPath = configPath;
            LoadFromFile();
            if(ErrorOnLoad)
            {
                throw new Exception($"Error loading assets pack from path {ConfigPath}.");
            }
            LoadAssets();
        }

        protected virtual void LoadFromFile()
        {

            if(string.IsNullOrEmpty(ConfigPath))
            {
                ErrorOnLoad = true;
                throw new Exception("Config path is null.");
            }

            if (!File.Exists(ConfigPath))
            {
                ErrorOnLoad = true;
                throw new Exception($"Config file at path {ConfigPath} doesn't exist.");
            }

            ConfigDocument ??= XDocument.Load(ConfigPath);
            //            // For now we will use the parse for testing
            //            ConfigDocument ??= XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
            //<pack>
            //    <meta>
            //        <name>Test</name>
            //        <description>Test</description>
            //        <type>PACK</type>
            //    </meta>
            //    <assets>
            //        <asset>
            //            <type>TILESETS</type>
            //            <name>test.png</name>
            //            <file_path>C:\Users\Ward\AppData\Roaming\RPG Creator\Assets\Tilesets\Spring Tile.png</file_path>
            //            <tile_width>16</tile_width>
            //            <tile_height>16</tile_height>
            //         </asset>
            //    </assets>
            //</pack>
            //");

            XElement? root = ConfigDocument.Root;

            if (root == null)
            {
                ErrorOnLoad = true;
                throw new Exception($"Config file at path {ConfigPath} is not valid.");
            }

            XElement meta = root.Element("meta") ?? throw new Exception($"Config file at path {ConfigPath} is not valid.");

            Id = Ulid.Parse(meta.Element("id")?.Value ?? Ulid.NewUlid().ToString());
            Name = meta.Element("name")?.Value ?? Path.GetFileNameWithoutExtension(ConfigPath);
            Description = meta.Element("description")?.Value ?? "No description.";
            Type = (PACK_TYPE)Enum.Parse(typeof(PACK_TYPE), meta.Element("type")?.Value ?? "UNKNOWN");

            if(Type == PACK_TYPE.UNKNOWN)
            {
                ErrorOnLoad = true;
                throw new Exception($"Config file at path {ConfigPath} is not valid.");
            }

        }

        public void LoadAssets()
        {
            var root = ConfigDocument.Root;
            
            if (root == null) return;
            
            var assets = root.Element("assets") ?? throw new Exception($"Config file at path {ConfigPath} is not valid.");
            
            if (!assets.HasElements) return;
            
            foreach (var assetElem in assets.Elements())
            {
                if (assetElem.Name == "asset")
                {
                    AddAssetFromFile(assetElem);
                }
            }
        }

        protected virtual void AddAssetFromFile(XElement assetElem)
        {
            var assetType = assetElem.Element("type")?.Value ?? "UNKNOWN";

            if (string.IsNullOrEmpty(assetType) || assetType == "UNKNOWN")
            {
                throw new Exception($"Asset type is null or empty.");
            }

            var assetTypeEnum = (BaseAsset.TYPE)Enum.Parse(typeof(BaseAsset.TYPE), assetType);

            if (assetTypeEnum == BaseAsset.TYPE.UNKNOWN)
            {
                throw new Exception($"Asset type is unknown.");
            }

            BaseAsset asset = assetTypeEnum switch
            {
                BaseAsset.TYPE.TILESETS => BaseAsset.CreateFromFile<Tileset>(assetElem, assetTypeEnum),
                _ => throw new Exception($"Asset type {assetType} is not supported.")
            };

            string asset_path = asset.PackPath;
            asset.PackName = Name;

            if (string.IsNullOrEmpty(asset_path))
            {
                throw new Exception($"Asset path is null or empty.");
            }

            if (Assets.ContainsKey(asset_path))
            {
                asset_path = $"{asset_path}_{Guid.NewGuid()}";
                asset.PackPath = asset_path;
            }

            if (asset.ShouldBeCached)
            {
                asset.IsCached = true;
                if (!EngineCore.ManagersReady)
                {
                    EventHandler<CoreManagersReadyArgs> handler = null;
                    handler = (object? sender, CoreManagersReadyArgs args) =>
                    {
                        EngineCore.Instance.Events.CoreManagersReady -= handler;
                        EngineCore.Instance.Managers.Assets.AddCachedAsset(asset);
                    };
                    EngineCore.Instance.Events.CoreManagersReady += handler;
                } else {
                    EngineCore.Instance.Managers.Assets.AddCachedAsset(asset);
                }
            }

            Assets.Add(asset_path, asset);

            //SaveAsset(asset);
        }

        public BaseAsset? GetAsset(string path)
        {
            var preArgs = new BaseAssetsPackGettingArgs(path);
            Events.OnAssetGetting(preArgs);

            if(preArgs.Cancel)
                return null;

            if(Assets.TryGetValue(path, out BaseAsset? asset))
            {
                Events.OnAssetGot(preArgs.ToPost(asset));
                return asset;
            }

            Events.OnAssetGot(preArgs.ToPost(null).SetError(true, $"Asset at path {path} doesnt' exist."));

            return null;
        }

        public bool HasAssetOfType(BaseAsset.TYPE type)
        {
            return Assets.Values.Any(asset => asset.Type == type);
        }

        public void UpdateAsset<T>(T asset) where T : BaseAsset
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset), "Asset cannot be null.");
            }
            if (!Assets.ContainsKey(asset.PackPath))
            {
                throw new KeyNotFoundException($"Asset at path {asset.PackPath} does not exist in the pack.");
            }
            if (asset.PackName != Name)
            {
                throw new InvalidOperationException($"Asset {asset.Name} doesn't belong to this pack (Belong to: {asset.PackName}).");
            }

            SaveAsset(asset);
        }

        public bool MoveAsset(string oldPath, string newPath)
        {

            var preArgs = new BaseAssetsPackMovingArgs(oldPath, newPath);
            Events.OnAssetMoving(preArgs);
            if (preArgs.Cancel)
                return false;
            if (!Assets.TryGetValue(oldPath, out BaseAsset? asset))
            {
                Events.OnAssetMoved(preArgs.ToPost(asset).SetError(true, $"Asset at path {oldPath} doesn't exist."));
                return false;
            }
            Assets.Remove(oldPath);
            asset.PackPath = newPath;
            Assets.Add(newPath, asset);
            Events.OnAssetMoved(preArgs.ToPost(asset));
            return true;
        }

        public bool RemoveAsset(string path)
        {
            var preArgs = new BaseAssetsPackRemovingArgs(path);
            Events.OnAssetRemoving(preArgs);
            if (preArgs.Cancel)
                return false;
            if (!Assets.Remove(path, out BaseAsset? asset))
            {
                Events.OnAssetRemoved(preArgs.ToPost().SetError(true, $"Asset at path {path} doesn't exist."));
                return false;
            }
            Events.OnAssetRemoved(preArgs.ToPost(asset));
            return true;
        }

        public bool RemoveAsset(BaseAsset asset)
        {
            var preArgs = new BaseAssetsPackRemovingArgs(asset.PackPath);

            Events.OnAssetRemoving(preArgs);
            if (preArgs.Cancel)
                return false;
            if (!Assets.Remove(asset.PackPath, out BaseAsset? _))
            {
                Events.OnAssetRemoved(preArgs.ToPost().SetError(true, $"Asset at path {asset.PackPath} doesn't exist."));
                return false;
            }
            Save();
            EngineCore.Instance.Managers.Assets.Event.OnRemovedAsset(new(asset, true));
            Events.OnAssetRemoved(preArgs.ToPost(asset));
            return true;
        }

        public void AddAsset<T>(T asset) where T : BaseAsset
        {
            var PreArgs = new BaseAssetsPackAddingArgs(asset.Name, asset);
            Events.OnAssetAdding(PreArgs);

            if(PreArgs.Cancel)
                return;

            if(string.IsNullOrEmpty(asset.PackName))
            {
                asset.PackName = Name;
            }

            if(string.IsNullOrEmpty(asset.PackPath))
            {
                asset.PackPath = $"{asset.Type}/{asset.Name}";
            }

            else if(asset.PackName != Name)
            {
                Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset {asset.Name} doesn't belong to this pack (Belong to: {asset.PackName}."));
                return;
            }

            if (!Assets.TryAdd(asset.PackPath, asset))
            {
                Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset at path {asset.Name} already exist."));
                return;
            }
            SaveAsset(asset);

            Events.OnAssetAdded(PreArgs.ToPost());
        }

        public void AddAsset<T>(string path, T asset) where T : BaseAsset
        {
            var PreArgs = new BaseAssetsPackAddingArgs(path, asset);
            Events.OnAssetAdding(PreArgs);

            if (PreArgs.Cancel)
                return;

            if (string.IsNullOrEmpty(asset.PackName))
            {
                asset.PackName = Name;
            }
            else if (asset.PackName != Name)
            {
                Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset {asset.Name} doesn't belong to this pack (Belong to: {asset.PackName}."));
                return;
            }

            asset.PackPath = path;

            if (!Assets.TryAdd(path, asset))
            {
                Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset at path {path} already exist."));
                return;
            }

            SaveAsset(asset);

            Events.OnAssetAdded(PreArgs.ToPost());
        }

        public BaseAsset? AddAsset(string path)
        {
            var preArgs = new BaseAssetsPackAddingArgs(path);
            Events.OnAssetAdding(preArgs);

            if(preArgs.Cancel)
                return null;

            string assetName;

            if (path.Contains('/'))
            {
                var pathSplit = path.Split("/");
                assetName = pathSplit[^1];
            }
            else
                assetName = path;

            if (!Assets.TryAdd(path, new BaseAsset(assetName) { PackPath = path, PackName = Name }))
            {
                Events.OnAssetAdded(preArgs.ToPost().SetError(true, $"Asset at path {path} already exist."));
                return null;
            }

            Events.OnAssetAdded(preArgs.ToPost(Assets[path]));

            return Assets[path];
        }

        public void SaveAsset<T>(T asset) where T : BaseAsset
        {
            if(asset == null)
            {
                throw new ArgumentNullException(nameof(asset), "Asset cannot be null.");
            }

            if(asset.PackName != Name)
            {
                throw new InvalidOperationException($"Asset {asset.Name} doesn't belong to this pack (Belong to: {asset.PackName}).");
            }

            if(ConfigDocument == null)
            {
                if(string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Description) || Type == PACK_TYPE.UNKNOWN)
                {
                    throw new InvalidOperationException("Pack name, description or type is not set and no configuration document found.");
                }

                CreateXMLDocument();
                if(ConfigDocument == null)
                {
                    throw new InvalidOperationException("Configuration document is null and couldn't be created.");
                }
            }

            asset.Save();

            if(asset.AssetData == null)
            {
                throw new InvalidOperationException($"Asset data for {asset.Name} is null.");
            }

            XElement? assetsElement = ConfigDocument.Root?.Element("assets");
            if (assetsElement == null)
            {
                throw new InvalidOperationException("Assets element in configuration document is null.");
            }

            XElement? existingAsset = assetsElement.Elements("asset").FirstOrDefault(a => a.Element("unique")?.Value == asset.Unique.ToString());
            if (existingAsset != null)
            {
                existingAsset.ReplaceWith(asset.AssetData);
            }
            else
            {
                assetsElement.Add(asset.AssetData);
            }

            ConfigDocument.Save(ConfigPath);

            EngineCore.Instance.Managers.Assets.Event.OnUpdatedAsset(new Managers.AssetsManager.EventsArgs.AssetsManagerUpdatedAssetArgs(asset.Type, asset));
        }

        public void CreateXMLDocument()
        {
            if(ConfigDocument != null)
            {
                return; // Already created
            }

            ConfigDocument = new XDocument(
                new XElement("pack",
                    new XElement("meta",
                        new XElement("name", Name),
                        new XElement("description", Description),
                        new XElement("type", Type.ToString()),
                        new XElement("id", Id.ToString())
                    ),
                    new XElement("assets")
                )
            );

            // Save the document to the config path
            if (!string.IsNullOrEmpty(ConfigPath))
            {
                
                string configDir = Path.GetDirectoryName(ConfigPath) ?? throw new InvalidOperationException("Config path directory cannot be null.");
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
            }
            else
            {
                // Or else, create a default path
                ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator", "AssetsPacks", $"{Id}.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath) ?? throw new InvalidOperationException("Config path directory cannot be null."));
            }

            ConfigDocument.Save(ConfigPath);
        }

        public void Save()
        {
            if(ConfigDocument == null)
            {
                CreateXMLDocument();
            }
            if(ConfigDocument != null)
            {

                // Update the asset data in the configuration document

                XElement? assetsElement = ConfigDocument.Root?.Element("assets");
                if (assetsElement == null)
                {
                    throw new InvalidOperationException("Assets element in configuration document is null.");
                }

                var assetsList = assetsElement.Elements("asset").ToList();

                foreach (var asset in Assets.Values)
                {
                    XElement? existingAsset = assetsElement.Elements("asset").FirstOrDefault(a => a.Element("unique")?.Value == asset.Unique.ToString());
                    if (existingAsset != null)
                    {
                        assetsList.Remove(existingAsset);
                        existingAsset.ReplaceWith(asset.AssetData);
                    }
                    else
                    {
                        assetsElement.Add(asset.AssetData);
                    }
                }

                // Remove any assets that are not in the current Assets dictionary
                foreach (var asset in assetsList)
                {
                    if (!Assets.Values.Any(a => a.Unique.ToString() == asset.Element("unique")?.Value))
                    {
                        asset.Remove();
                    }
                }

                ConfigDocument.Save(ConfigPath);
            }
            else
            {
                throw new InvalidOperationException("Configuration document is null and couldn't be created.");
            }
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(BaseAssetsPack));
            info.AddValue("ConfigPath", ConfigPath);
            info.AddValue("Name", Name);
            info.AddValue("Description", Description ?? "");
            info.AddValue("Type", Type);
            info.AddValue("Id", Id);
            info.AddValue("Assets", Assets);
            return info;
        }

        public void SetObjectData(SerializationInfo info)
        {
            info.TryGetValue("ConfigPath", out ConfigPath, "", "Field 'ConfigPath' not found in serialization info.");
            info.TryGetValue("Name", out Name, "", "Field 'Name' not found in serialization info.");
            info.TryGetValue("Description", out Description, "");
            info.TryGetValue("Type", out Type, PACK_TYPE.UNKNOWN, "Field 'Type' not found in serialization info.");
            info.TryGetValue("Id", out Id, Ulid.NewUlid(), "Field 'Id' not found in serialization info.");
            info.TryGetDictionary("Assets", out Assets);
        }
    }
}

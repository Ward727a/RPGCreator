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
    public class BaseAssetsPack
    {

        public readonly Dictionary<string, BaseAsset> Assets = [];
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
        public string Description;
        public PACK_TYPE Type;

        public BaseAssetsPack()
        { }

        public BaseAssetsPack(string configPath)
        {
            ConfigPath = configPath;
            LoadFromFile();
        }

        protected virtual void LoadFromFile()
        {

            if(ConfigPath == null && false) // Need to think of removing the true
            {
                throw new Exception("Config path is null.");
            }

            if (!File.Exists(ConfigPath) && false) // Need to think of removing the true
            {
                throw new Exception($"Config file at path {ConfigPath} doesn't exist.");
            }

            //ConfigDocument ??= XDocument.Load(ConfigPath);
            // For now we will use the parse for testing
            ConfigDocument ??= XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8""?>
<pack>
    <meta>
        <name>Test</name>
        <description>Test</description>
        <type>PACK</type>
    </meta>
    <assets>
        <asset>
            <type>TILESETS</type>
            <name>test.png</name>
            <file_path>C:\Users\Ward\AppData\Roaming\RPG Creator\Assets\Tilesets\Spring Tile.png</file_path>
            <tile_width>16</tile_width>
            <tile_height>16</tile_height>
         </asset>
    </assets>
</pack>
");

            XElement? root = ConfigDocument.Root;

            if (root == null)
            {
                throw new Exception($"Config file at path {ConfigPath} is not valid.");
            }

            XElement meta = root.Element("meta") ?? throw new Exception($"Config file at path {ConfigPath} is not valid.");
            XElement assets = root.Element("assets") ?? throw new Exception($"Config file at path {ConfigPath} is not valid.");

            Name = meta.Element("name")?.Value ?? Path.GetFileNameWithoutExtension(ConfigPath);
            Description = meta.Element("description")?.Value ?? "No description.";
            Type = (PACK_TYPE)Enum.Parse(typeof(PACK_TYPE), meta.Element("type")?.Value ?? "UNKNOWN");

            if(Type == PACK_TYPE.UNKNOWN)
            {
                throw new Exception($"Config file at path {ConfigPath} is not valid.");
            }

            if(assets.HasElements)
            {

                foreach (XElement asset_elem in assets.Elements())
                {
                    if(asset_elem.Name == "asset")
                    {
                        AddAssetFromFile(asset_elem);
                    }
                }

            }


        }

        protected virtual void AddAssetFromFile(XElement asset_elem)
        {
            string asset_type = asset_elem.Element("type")?.Value ?? "UNKNOWN";

            if (string.IsNullOrEmpty(asset_type) || asset_type == "UNKNOWN")
            {
                throw new Exception($"Asset type is null or empty.");
            }

            BaseAsset.TYPE asset_type_enum = (BaseAsset.TYPE)Enum.Parse(typeof(BaseAsset.TYPE), asset_type);

            if (asset_type_enum == BaseAsset.TYPE.UNKNOWN)
            {
                throw new Exception($"Asset type is unknown.");
            }

            BaseAsset asset;

            switch (asset_type_enum)
            {
                case BaseAsset.TYPE.TILESETS:
                    asset = BaseAsset.CreateFromFile<Tileset>(asset_elem, asset_type_enum);
                    break;
                default:
                    throw new Exception($"Asset type {asset_type} is not supported.");
            }

            string asset_path = asset.PackPath;

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
        }

        public BaseAsset? GetAsset(string path)
        {
            var preArgs = new BaseAssetsPackGettingArgs(path);
            Events.OnAssetGetting(preArgs);

            if(preArgs.Cancel)
                return null;

            if(Assets.ContainsKey(path))
            {
                BaseAsset asset = Assets[path];

                Events.OnAssetGot(preArgs.ToPost(asset));
                return asset;
            }

            Events.OnAssetGot(preArgs.ToPost(null).SetError(true, $"Asset at path {path} doesnt' exist."));

            return null;
        }

        public bool HasAssetOfType(BaseAsset.TYPE type)
        {
            foreach (var asset in Assets.Values)
            {
                if (asset.Type == type)
                    return true;
            }
            return false;
        }

        public bool MoveAsset(string oldPath, string newPath)
        {

            var PreArgs = new BaseAssetsPackMovingArgs(oldPath, newPath);
            Events.OnAssetMoving(PreArgs);
            if (PreArgs.Cancel)
                return false;
            if (!Assets.TryGetValue(oldPath, out BaseAsset? asset))
            {
                Events.OnAssetMoved(PreArgs.ToPost(asset).SetError(true, $"Asset at path {oldPath} doesn't exist."));
                return false;
            }
            Assets.Remove(oldPath);
            asset.PackPath = newPath;
            Assets.Add(newPath, asset);
            Events.OnAssetMoved(PreArgs.ToPost(asset));
            return true;
        }

        public bool RemoveAsset(string path)
        {
            var PreArgs = new BaseAssetsPackRemovingArgs(path);
            Events.OnAssetRemoving(PreArgs);
            if (PreArgs.Cancel)
                return false;
            if (!Assets.Remove(path, out BaseAsset? asset))
            {
                Events.OnAssetRemoved(PreArgs.ToPost().SetError(true, $"Asset at path {path} doesn't exist."));
                return false;
            }
            Events.OnAssetRemoved(PreArgs.ToPost(asset));
            return true;
        }

        public bool RemoveAsset(BaseAsset asset)
        {
            var PreArgs = new BaseAssetsPackRemovingArgs(asset.PackPath);

            Events.OnAssetRemoving(PreArgs);
            if (PreArgs.Cancel)
                return false;
            if (!Assets.Remove(asset.PackPath, out BaseAsset? _))
            {
                Events.OnAssetRemoved(PreArgs.ToPost().SetError(true, $"Asset at path {asset.PackPath} doesn't exist."));
                return false;
            }
            Events.OnAssetRemoved(PreArgs.ToPost(asset));
            return true;
        }

        public void AddAsset(BaseAsset asset)
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

            Events.OnAssetAdded(PreArgs.ToPost());
        }

        public void AddAsset(string path, BaseAsset asset)
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

            Events.OnAssetAdded(PreArgs.ToPost());
        }

        public BaseAsset? AddAsset(string path)
        {
            var PreArgs = new BaseAssetsPackAddingArgs(path);
            Events.OnAssetAdding(PreArgs);

            if(PreArgs.Cancel)
                return null;

            string asset_name;

            if (path.Contains('/'))
            {
                string[] path_split = path.Split("/");
                asset_name = path_split[^1];
            }
            else
                asset_name = path;

            if (!Assets.TryAdd(path, new BaseAsset(asset_name) { PackPath = path, PackName = Name }))
            {
                Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset at path {path} already exist."));
                return null;
            }

            Events.OnAssetAdded(PreArgs.ToPost(Assets[path]));

            return Assets[path];
        }
    }
}

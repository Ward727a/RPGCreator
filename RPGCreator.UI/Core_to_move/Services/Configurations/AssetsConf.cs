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
using RPGCreator.Core.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RPGCreator.Core.Services.ConfigurationService;
using System.Xml.Linq;
using RPGCreator.Core.Types;

namespace RPGCreator.Core.Services.Configurations
{

    public class AssetsConf : ConfHelper
    {

        static private string ASSET_CONF_TEMPLATE = $@"
{CONF_WARN_TEMPLATE}
<Assets>
    <Using/>
    <Tilesets/>
</Assets>
";
        protected static readonly Dictionary<string, AssetsConf> _loadedAssets = [];
        protected static ConfigurationService? _configService;

        private XDocument _doc;
        private bool _isProject = false;
        private bool _isPack = false;

        private string XMLPath = "";

        protected readonly List<string> _using = [];
        public readonly ObservableDictionary<string, Tileset> Tilesets = [];

        public bool IsInited => _configService != null;
        public bool IsValid => (_isProject || _isPack) && _configService != null && !string.IsNullOrEmpty(XMLPath);
        public bool IsProject => _isProject;
        public bool IsPack => _isPack;

        public static bool TryLoadFromProject(ref Project project, out AssetsConf assetsConfig)
        {
            assetsConfig = new();
            AppConf appConf;

            if (_configService == null || !_configService.TryGetConfig<AppConf>("app", out appConf))
            {
#if DEBUG
                throw new Exception("configService is either null, or doesn't have app config loaded.");
#else
                    return false;
#endif
            }

            if (appConf == null)
            {
#if DEBUG
                throw new Exception("appConf is null even after TryGetConfig returned true?");
#else
                    return false;
#endif
            }

            string assetsFolderPath = project.Path;

            if (string.IsNullOrEmpty(assetsFolderPath))
            {
#if DEBUG
                throw new Exception("assets path or project name is null or empty.");
#else
                    return false;
#endif
            }

            string fullAssetsPath = Path.Combine(assetsFolderPath, "_Assets");

            if (!Directory.Exists(fullAssetsPath))
            {
                Directory.CreateDirectory(fullAssetsPath);
            }

            string configFile = Path.Combine(fullAssetsPath, "assets.conf.xml");

            if (!File.Exists(configFile))
            {
                File.WriteAllText(configFile, ASSET_CONF_TEMPLATE);
            }
            File.Copy(configFile, $"{configFile}.bak", true);
            try
            {
                assetsConfig._doc = XDocument.Load(configFile);
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"The asset config file at {configFile} couldn't be loaded.");
            }

            assetsConfig.XMLPath = configFile;
            assetsConfig._isProject = true;

            project.AssetsConf = assetsConfig;

            assetsConfig.OnLoadedConf();

            return false;
        }

        public static void InitAssetsConf(ConfigurationService configurationService)
        {
            _configService = configurationService;
        }

        public override void OnLoadedConf()
        {
            if (_doc.Root == null)
            {
                throw new NullReferenceException("The document root element is null, the asset config file can't be parsed.");
            }

            if (!ParseAssetsConfig())
            {
                throw new Exception("Couldn't parse the document due to an unknown reason. Please contact the developer.");
            }
        }

        private bool ParseAssetsConfig()
        {

            if (!_doc.Root.HasElements)
                return false;

            if (_doc.Root.Element("Using") != null)
            {
                foreach (XElement use_item in _doc.Root.Element("Using")!.Elements().Where(e => e.Name == "Use").ToArray())
                {
                    if (use_item.HasElements || string.IsNullOrEmpty(use_item.Value))
                    {
                        continue;
                    }

                    if (_using.Contains(use_item.Value))
                    {
                        _using.Add(use_item.Value);
                    }
                }
            }

            if (_doc.Root.Element("Tilesets") != null)
            {
                foreach (XElement tileset_item in _doc.Root.Element("Tilesets")!.Elements().Where(e => e.Name == "Item").ToArray())
                {
                    if (!tileset_item.HasElements)
                        continue;

                    if (tileset_item.Element("Name") == null || tileset_item.Element("Path") == null || tileset_item.Element("Dimension") == null)
                        continue;

                    string tile_name = tileset_item.Element("Name")!.Value;
                    string tile_path = tileset_item.Element("Path")!.Value;
                    int tile_dim = int.Parse(tileset_item.Element("Dimension")!.Value);

                    bool tile_hardcopy = false;

                    if (tileset_item.Element("IsHardcopy") != null)
                    {
                        tile_hardcopy = tileset_item.Element("IsHardcopy").Value == "true";
                    }
                    else
                        tileset_item.Add(new XElement("IsHardcopy", "false"));

                    if (string.IsNullOrEmpty(tile_path) || string.IsNullOrEmpty(tile_name) || !int.IsPositive(tile_dim) || tile_dim == 0)
                        continue;

                    if (!File.Exists(tile_path))
                        continue;

                    Tileset item = new Tileset(tileset_item)
                    {
                        Name = tile_name,
                        Path = tile_path,
                        Dimension = tile_dim,
                        Is_hardcopy = tile_hardcopy
                    };

                    Tilesets.TryAdd(tile_name, item);
                }
            }

            return true;
        }

        public bool TryAddTileset(string path, string name, int dimension, bool hardlink = false)
        {
            if (!File.Exists(path) || string.IsNullOrEmpty(name) || Tilesets.ContainsKey(name) || !IsValid)
            {
                return false;
            }

            if (_doc.Root.Element("Tilesets") == null)
            {
                _doc.Root.Add("Tilesets");
            }

            XElement? tilesets = _doc.Root.Element("Tilesets");

            if (tilesets == null)
                return false;


            XElement tilesetXML = new XElement("Item",
                new XElement("Name", name),
                new XElement("Path", path),
                new XElement("Dimension", dimension),
                new XElement("IsHardcopy", hardlink?"true":"false")
                );

            tilesets.Add(tilesetXML);

            Tilesets.Add(name, new Tileset(tilesetXML) {Name = name, Path = path, Dimension = dimension, Is_hardcopy = hardlink });
            return true;
        }

        public void SetTileset(string name, Tileset item)
        {
            Tilesets[name] = item;
        }

        public bool TryRenameTileset(string current_name, string new_name)
        {

            if (!Tilesets.ContainsKey(current_name) || Tilesets.ContainsKey(new_name))
                return false;

            if (TryGetTileset(current_name, out Tileset item))
            {
                item.Name = new_name;
                Tilesets[new_name] = item;

                Tilesets.Remove(current_name);
                return true;
            }
            return false;
        }

        public bool TryGetTileset(string name, out Tileset tileset)
        {
            return Tilesets.TryGetValue(name, out tileset);
        }

        public List<Tileset> GetTilesets()
        {
            return Tilesets.Values.ToList();
        }

        public bool HasTileset(string name)
        {
            return Tilesets.ContainsKey(name);
        }

        public override void Save(string filepath = "")
        {
            _doc.Save(XMLPath);
        }
    }
}

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
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;
using RPGCreator.Core.Type.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Type.Project
{
    public partial class BaseProject : ObservableObject, ISerializable, IDeserializable
    {
        public Ulid Id { get; set; } = Ulid.NewUlid();
        public string? Name { get; set; } = "";
        public string? Description { get; set; } = "";
        public string? Path  { get; set; } = "";
        public Version? Version  { get; set; } = new Version(0, 0, 0, 0);
        public Version? EditorVersion  { get; set; } = new Version(0, 0, 0, 0);
        public bool IsArchived  { get; set; }
        public bool IsFavorite  { get; set; }
        public bool IsForcedLock { get; set; } = false; // If true, the project cannot be opened in the editor, due to bug or other issues.
        public string? Copyright { get; set; } = "";
        public List<string> Authors { get; set; } = [];
        public List<string> AssetsPackPath = [];

        [ObservableProperty]
        private BaseMap? _EditMap = null;

        public bool EditingMap => EditMap != null;

        public ProjectGameData GameData;

        public ProjectEvent Event;

        #region PropertyEvent


        #endregion

        public BaseProject()
        {
            Event = new ProjectEvent();
        }
        
        public BaseProject(string name)
        {
            Name = name;
            Event = new ProjectEvent();
            GameData = new ProjectGameData(this);
            GameData.Maps.CollectionChanged += (_, _) =>
            {
                Event.OnMapsListChanged();
            };
        }

        public void Load()
        {
            EngineCore.Instance.Managers.Assets.ClearAssetsPacks();

            foreach (string packPath in AssetsPackPath)
            {
                EngineCore.Instance.Managers.Assets.LoadPack(packPath);
            }

            EngineCore.Instance.Data.EditedProject = this;
        }

        public void Unload()
        {
            EngineCore.Instance.Managers.Assets.ClearAssetsPacks();
            EngineCore.Instance.Data.EditedProject = null;
        }

        public void Save()
        {
            ProjectsConf conf = EngineCore.Instance.Configs.GetConfig<ProjectsConf>("ProjectsConf");
            conf.SaveProject(this, false);
        }

        private string[] FormatString(string unformatedString)
        {
            return unformatedString.Split([':'], StringSplitOptions.TrimEntries);
        }

        public BaseAsset? GetAsset(string path)
        {
            var pathParts = FormatString(path);

            if(pathParts.Length != 2)
                return null;

            string AssetPack = pathParts[0];
            string AssetPath = pathParts[1];

            if(!EngineCore.Instance.Managers.Assets.HasAssetsPack(AssetPack))
            {
                return null;
            }

            BaseAsset? asset = EngineCore.Instance.Managers.Assets.GetAsset(path);

            if(asset == null)
            {
                return null;
            }

            return asset;
        }

        public List<BaseAsset> GetAssetsType(BaseAsset.TYPE type)
        {
            List<BaseAsset> assets_found = [];

            foreach (BaseAssetsPack pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
            {
                if (pack.Assets.Values.Any(x => x.Type == type))
                {
                    assets_found.AddRange(pack.Assets.Values.Where(x => x.Type == type).ToList());
                }
            }

            return assets_found;
        }
        public List<T> GetAssetsType<T>(BaseAsset.TYPE type) where T:BaseAsset
        {
            List<T> assets_found = [];

            foreach (BaseAssetsPack pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
            {
                if (pack.Assets.Values.Any(x => x.Type == type))
                {
                    assets_found.AddRange(
                        pack.Assets.Values.Where(x => x.Type == type && x is T).Cast<T>().ToList()
                        );
                }
            }

            return assets_found;
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(BaseProject));
            info.AddValue("id", Id);
            info.AddValue("name", Name);
            info.AddValue("description", Description);
            info.AddValue("path", Path);
            info.AddValue("version", Version);
            info.AddValue("editorVersion", EditorVersion);
            info.AddValue("isArchived", IsArchived);
            info.AddValue("isFavorite", IsFavorite);
            info.AddValue("isForcedLock", IsForcedLock);
            info.AddValue("copyright", Copyright);
            info.AddValue("authors", Authors);
            info.AddValue("assetsPackPath", AssetsPackPath);
            info.AddValue("gameData", GameData);
            return info;
        }

        public void SetObjectData(SerializationInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
            }

            info.TryGetValue("id", out Ulid id, Ulid.NewUlid(), "ID not found or invalid (Set to new Ulid by default).");
            info.TryGetValue("name", out string name, "Unnamed Project", "Name not found or invalid (Set to 'Unnamed Project' by default).");
            info.TryGetValue("description", out string description, "", "Description not found or invalid (Set to empty string by default).");
            info.TryGetValue("path", out string path, "", "Path not found or invalid (Set to empty string by default).");
            info.TryGetValue("version", out Version version, new Version(0, 0, 0, 0), "Version not found or invalid (Set to (0, 0, 0, 0) by default).");
            info.TryGetValue("editorVersion", out Version editorVersion, new Version(0, 0, 0, 0), "Editor version not found or invalid (Set to (0, 0, 0, 0) by default).");
            info.TryGetValue("isArchived", out bool isArchived, false, "Is archived not found or invalid (Set to false by default).");
            info.TryGetValue("isFavorite", out bool isFavorite, false, "Is favorite not found or invalid (Set to false by default).");
            info.TryGetValue("isForcedLock", out bool isForcedLock, false, "Is forced lock not found or invalid (Set to false by default).");
            info.TryGetValue("copyright", out string copyright, "", "Copyright not found or invalid (Set to empty string by default).");
            info.TryGetList("authors", out List<string> authors, [], "Authors not found or invalid (Set to empty list by default).");
            info.TryGetList("assetsPackPath", out List<string> assetsPackPath, [], "Assets pack path not found or invalid (Set to empty list by default).");
            info.TryGetValue("gameData", out ProjectGameData gameData, new ProjectGameData(this), "Game data not found or invalid (Set to null by default).");

            Name = name;
            Description = description;
            Path = path;
            Version = version;
            EditorVersion = editorVersion;
            IsArchived = isArchived;
            IsFavorite = isFavorite;
            IsForcedLock = isForcedLock;
            Copyright = copyright;
            Authors = authors;
            AssetsPackPath = assetsPackPath;
            GameData = gameData;
            GameData.Maps.CollectionChanged += (_, _) =>
            {
                Event.OnMapsListChanged();
            };
        }
    }
}

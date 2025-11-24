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
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Map;
using Serilog;

namespace RPGCreator.Core.Types.Project
{
    public partial class BaseProject : ObservableObject, ISerializable, IDeserializable
    {
        public event Action? OnProjectLoaded;
        
        public Ulid Id { get; set; }
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
        private MapInstance? _EditMap = null;

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
            Id = Ulid.NewUlid();
            GameData.Maps.CollectionChanged += (_, _) =>
            {
                Event.OnMapsListChanged();
            };
        }

        public void Load()
        {
            EngineCore.Instance.Data.EditedProject = this;

            foreach (string packPath in AssetsPackPath)
            {
                try
                {
                    EngineCore.Instance.Managers.Assets.AddPack(packPath);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to load assets pack at {packPath}: {Message}", packPath, ex.Message);
                    continue; // Skip this pack and continue with the next one
                }
            }

            OnProjectLoaded?.Invoke();
        }

        public void Unload()
        {
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
        public List<T> GetAssetsType<T>(BaseAsset.TYPE type) where T:BaseAsset
        {
            List<T> assets_found = [];

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

        public void SetObjectData(Serializer.DeserializationInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
            }

            info.TryGetValue("id", out Ulid id, Ulid.NewUlid());
            info.TryGetValue("name", out string name, "Unnamed Project");
            info.TryGetValue("description", out string description, "");
            info.TryGetValue("path", out string path, "");
            info.TryGetValue("version", out Version version, new Version(0, 0, 0, 0));
            info.TryGetValue("editorVersion", out Version editorVersion, new Version(0, 0, 0, 0));
            info.TryGetValue("isArchived", out bool isArchived, false);
            info.TryGetValue("isFavorite", out bool isFavorite, false);
            info.TryGetValue("isForcedLock", out bool isForcedLock, false);
            info.TryGetValue("copyright", out string copyright, "");
            info.TryGetList("authors", out List<string> authors);
            info.TryGetList("assetsPackPath", out List<string> assetsPackPath);
            info.TryGetValue("gameData", out ProjectGameData gameData, new ProjectGameData(this));

            Id = id;
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

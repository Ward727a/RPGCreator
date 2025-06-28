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
    public partial class BaseProject : ObservableObject
    {

        public string? Name { get; set; } = "";
        public string? Description { get; set; } = "";
        public string? Path  { get; set; } = "";
        public Version? Version  { get; set; } = new Version(0, 0, 0, 0);
        public Version? EditorVersion  { get; set; } = new Version(0, 0, 0, 0);
        public bool IsArchived  { get; set; }
        public bool IsFavorite  { get; set; }
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

            //TODO: Need to switch to AssetsManager for this!

            //foreach (string packPath in AssetsPackPath)
            //{
            //    EngineCore.Instance.Managers.AssetsPack.LoadAssetsPack(packPath);
            //}

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

        public void SaveConfig()
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

    }
}

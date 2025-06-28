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
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;
using RPGCreator.Core.Type.Map;
using RPGCreator.Core.Type.Project;
using RPGCreator.MonoGame;
using RPGCreator.UI.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RPGCreator.UI.OLD.ViewModels._Editor
{
    public partial class EditorViewModel : ViewModelBase
    {
        public static BaseProject? ProjectItem => EngineCore.Instance.Data.EditedProject;

        public static EditorGame? EditorGame => EngineCore.Instance.Data.RTPGame as EditorGame;

        [ObservableProperty]
        private List<string> layersName;

        public List<Tileset> AvalTilesets => ProjectItem?.GetAssetsType<Tileset>(BaseAsset.TYPE.TILESETS) ?? [];
        public List<BaseMap> Maps => GetRootMaps() ?? [new BaseMap("Test")];
        public List<MapLayer> MapLayers => GetSortedMaps() ?? [];

        private List<BaseMap>? GetRootMaps()
        {
            var allChild = new HashSet<BaseMap>(
                ProjectItem?.GameData.Maps?.SelectMany(map => map.ChildMaps)
                );

            return ProjectItem?.GameData.Maps?.Where(map => !allChild.Contains(map)).ToList();
        }

        private List<MapLayer>? GetSortedMaps()
        {
            return ProjectItem?.EditMap?.Layers.OrderBy(L => L.ZIndex).ToList() ?? [];
        }

        public ObservableCollection<BaseMap> SelectedMap { get; set; } = [];

        [ObservableProperty]
        private int positionX;
        [ObservableProperty]
        private int positionY;

        [ObservableProperty]
        private string buttonContent;

        [ObservableProperty]
        private int selectedTilesetIndex = -1;

        [ObservableProperty]
        private int selectedLayerIndex = -1;

        [ObservableProperty]
        private Tileset _selected_tileset;

        private int _zoomFactor = 0;

        [ObservableProperty]
        private float selectionSquareSize;

        [ObservableProperty]
        private Transform renderPosition = new TranslateTransform();

        [ObservableProperty]
        private float zoomSize = 240f;
        public Microsoft.Xna.Framework.Rectangle SelectedTile;

        [ObservableProperty]
        private string newLayerName = "";

        public EditorViewModel()
        {
            new EditorGame(); // We create the game instance here, but we do not add it to a variable simply because we want to use the EngineData instance.

            EngineCore.Instance.Events.OnUIEditorOpened(new());

            EngineCore.Instance.Managers.Assets.Event.UpdatedAsset += (object? sender, AssetsManagerUpdatedAssetArgs? args) =>
            {

                if (args == null || args.Type == BaseAsset.TYPE.UNKNOWN)
                {
                    return;
                }

                switch (args.Type)
                {
                    case BaseAsset.TYPE.TILESETS:
                        OnPropertyChanged(nameof(AvalTilesets));
                        break;
                    default:
                        break;
                }
            };

            if (EngineCore.Instance.Data.EditedProject != null)
            {
                OnPropertyChanged(nameof(AvalTilesets));
                OnPropertyChanged(nameof(Maps));

                EngineCore.Instance.Data.EditedProject.Event.MapsListChanged += (_, _) =>
                {
                    OnPropertyChanged(nameof(Maps));
                };

                SelectedMap.CollectionChanged += SelectedMap_CollectionChanged;
            }

            EngineCore.Instance.Scheduler.WaitSecond(2, () =>
            {
                EngineCore.Instance.Managers.Assets.CreateAssetsPack("Default", BaseAssetsPack.PACK_TYPE.PACK);
                EngineCore.Instance.Managers.Assets.AddAsset("Default", new Tileset("Default Tileset", 16, 16, "C:\\Users\\Ward\\AppData\\Roaming\\RPG Creator\\Assets\\Tilesets\\spr_tileset_sunnysideworld_16px.png"));
            });

            //App.Services.GetRequiredService<EditorService>().ProjectChanged += EditorViewModel_ProjectChanged;
        }

        private void Layers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MapLayers));
        }

        private void SelectedMap_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(ProjectItem.EditMap != null)
                ProjectItem.EditMap.Layers.CollectionChanged -= Layers_CollectionChanged;
            ProjectItem.EditMap = SelectedMap.FirstOrDefault();

            if (ProjectItem.EditMap != null)
                ProjectItem.EditMap.Layers.CollectionChanged += Layers_CollectionChanged;
            OnPropertyChanged(nameof(MapLayers));
        }


        //private void EditorViewModel_ProjectChanged(object? sender, EventArgs e)
        //{
        //    if (ProjectItem != null)
        //    {
        //        EngineCore.Instance.Data.RTPGame._events.OnLoaded += GameEditor_OnLoaded;
        //    }
        //}

        private void GameEditor_OnLoaded(object? sender, EventArgs e)
        {

            //# This could probably be removed and changed with the new EngineCore system.


            //EngineCore.Instance.Managers.Assets.Event.AddedAsset += Event_AddedAsset;
            //ProjectItem.AssetsConf.Tilesets.CollectionChanged += Tilesets_CollectionChanged;

            //AvalTilesets = ProjectItem.AssetsConf.GetTilesets();

            //foreach (Tileset tileset in AvalTilesets)
            //{
            //    if (tileset != null)
            //    {
            //        tileset.CreateTexture();
            //    }
            //}

            //EditorGame.UsedTilesets = [.. AvalTilesets.Select(t => new GameTileset() { Texture = t.texture, Dimension = t.Dimension })];
        }

        private void Tilesets_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //# This should be changed to use the new EngineCore system.

            //AvalTilesets = ProjectItem.GetAssetsType<Tileset>(Core.Type.Assets.BaseAsset.TYPE.TILESETS);
            //EditorGame.UsedTilesets = [.. AvalTilesets.Select(t => new GameTileset() { Texture = t.GetTexture(), Dimension = t.Dimension })];
        }

        partial void OnSelectedTilesetIndexChanged(int value)
        {

            //# This need to be reworked

            //ZoomSize = 240;
            //_zoomFactor = 0;
            //if (avalTilesets.Count > 0 && avalTilesets.Count-1 >= value && value > -1)
            //{
            //    Selected_tileset = avalTilesets[value];
            //    SelectionSquareSize = Selected_tileset.Dimension;

            //}
            //EditorGame.SelectedTileset = value;
        }

        partial void OnSelectedLayerIndexChanged(int value)
        {
            //EditorGame.SelectedLayer = value;
        }

        [RelayCommand]
        private void AddLayer()
        {
            ProjectItem?.EditMap?.AddLayer(new($"Test-{MapLayers.Count}", MapLayers.Count, true));
        }

        [RelayCommand]
        private void SaveMap()
        {
            //EditorGame.CurrentMap.SaveMap("E:\\");
        }

        [RelayCommand]
        private void LoadMap()
        {
            //EditorGame.CurrentMap.LoadMap("E:\\", "Default map");
        }

        [RelayCommand]
        private void ZoomIn()
        {
            ZoomSize *= 1.1f;
            SelectionSquareSize *= 1.1f;
        }

        [RelayCommand]
        private void ZoomOut()
        {
            ZoomSize /= 1.1f;

            SelectionSquareSize /= 1.1f;
        }

        [RelayCommand]
        private void AddButton()
        {
            if (string.IsNullOrEmpty(ButtonContent))
            {
                //EditorGame?.AddButton(PositionX, PositionY);
            } else
                //EditorGame?.AddButton(PositionX, PositionY, ButtonContent);
            ButtonContent = "";
        }

        [RelayCommand]
        private void AddMap()
        {
            BaseMap map = new("TestMap");
            ProjectItem?.GameData.Maps?.Add(map);
        }

        [RelayCommand]
        private void AddChildMap(Ulid ulidMap)
        {
            ProjectItem?.GameData.Maps?.SingleOrDefault(e => e.Identifier == ulidMap)?.CreateChildMap("TestChild");
        }

        [RelayCommand]
        private void ImportFile()
        {
            // Test file
            string TestFile = "C:\\Users\\Ward\\AppData\\Roaming\\RPG Creator\\Assets\\Tilesets\\Summer Tile.png";

            //ProjectItem.AssetsConf.TryAddTileset(TestFile, "Summer Tile", 16);
            //ProjectItem.AssetsConf.Save();
        }

    }
}

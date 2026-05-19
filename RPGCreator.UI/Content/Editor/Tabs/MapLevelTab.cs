// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Diagnostics;
using MethodTimer;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.EditorUiService;
using RPGCreator.UI.Common;

namespace RPGCreator.UI.Content.Editor.Tabs
{
    public class MapLevelTab : UserControl
    {

        private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<MapLevelTab>();

        private Grid _BodyGrid;
        private AutoCompleteBox _SearchBox;

        private Grid _ContentGrid;
        private ScrollViewer _Scroller;
        private StackPanel _MapList;

        public MapLevelTab()
        {
            CreateComponents();
            RegisterEvents();
        }

        private void CreateComponents()
        {
            
            _BodyGrid = new Grid
            {
                Margin = App.style.Margin,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                RowDefinitions = new RowDefinitions("Auto,*"),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0))
            };
            Content = _BodyGrid;

            _SearchBox = new AutoCompleteBox()
            {
                Watermark = "Search ([map] | [level] | [level]@[map])",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            _BodyGrid.Children.Add(_SearchBox);

            _ContentGrid = new Grid
            {
                Margin = App.style.Margin,
                RowDefinitions = new("*")
            };
            _BodyGrid.Children.Add(_ContentGrid);

            _Scroller = new ScrollViewer
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
                ClipToBounds = true,
            };

            _MapList = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin
            };

            _Scroller.Content = _MapList;
            _BodyGrid.Children.Add(_Scroller);
            Grid.SetRow(_Scroller, 1);
        }

        private void RegisterEvents()
        {
            Loaded += OnLoaded;
            _BodyGrid.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(_BodyGrid).Properties.IsRightButtonPressed)
                {
                    var elementUnderPointer = _BodyGrid.InputHitTest(e.GetPosition(_BodyGrid));

                    if (elementUnderPointer != _BodyGrid && elementUnderPointer != _Scroller && elementUnderPointer.GetType().Name != "ScrollContentPresenter")
                        return; // If the right click is not on the MapLevelPanel itself, do nothing

                    e.Handled = true;

                    GlobalStaticUiData.CloseContext();
                    GlobalStaticUiData.CurrentContext = new ContextMenu();
                    var addMapItem = new MenuItem { Header = "Add Map" };
                    addMapItem.Click += (s, e) => OnCreateNewMap();
                    (GlobalStaticUiData.CurrentContext as ContextMenu).Items.Add(addMapItem);
                    GlobalStaticUiData.OpenContext(_BodyGrid);
                }
            };
        }

        [Time]
        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            var project = EngineServices.ProjectsManager.GetCurrentProject();
            _MapList.Children.Clear();
            if (project != null)
            {
                var maps = EngineServices.AssetsManager.GetAssetsOfClass(MapDefinition.ClassURN);
                
                foreach (var map in maps.Value)
                {
                    AddMapToUi(EngineServices.AssetsManager.Load<MapDefinition>(map).Value);
                }
            }
        }

        private async Task OnCreateNewMap()
        {
            var result = await EditorUiServices.DialogService.PromptTextAsync("Add Map", "Map Name:", "New Map", new(SizeToContent: DialogSizeToContent.HeightOnly));
            if (result == null)
                return;

            Guard.IsNotNull(GlobalStates.ProjectState.CurrentProject, "CurrentProject");

            var creationResult = EngineServices.AssetsManager.Create<MapDefinition>(MapDefinition.ClassURN);

            if (creationResult.IsFailure)
            {
                Logger.Error("Failed to create map: {0}", creationResult.Error);
                return;
            }
            
            var mapDef = creationResult.Value;
            mapDef.Name = result;

            EngineServices.AssetsManager.Save(mapDef);
            
            AddMapToUi(mapDef);
            Logger.Info($"Map '{result}' created.");
        }
        
        private void AddMapToUi(MapDefinition? mapMetaData)
        {
            if (mapMetaData == null) return;
            var map = new MapItem(mapMetaData);
            _MapList.Children.Add(map);
        }
    }
}

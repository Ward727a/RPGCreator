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
using Avalonia.VisualTree;
using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.UiService;

namespace RPGCreator.UI.Content.Editor.Tabs
{
    public class MapLevelTab : UserControl, ITab
    {

        private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<MapLevelTab>();
        private readonly IAssetScope _assetScope = EngineServices.AssetsManager.CreateAssetScope("MapLevelTab");
        
        private ScrollViewer _Scroller;
        private StackPanel _MapList;

        private class PopupMap : Window
        {
            public PopupMap()
            {
                Title = "Add Map";
                Width = 300;
                Height = 200;
                Content = new TextBox
                {
                    Watermark = "Map Name",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = App.style.Margin
                };
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private class PopupLevel : Window
        {
            public PopupLevel()
            {
                Title = "Add Level";
                Width = 300;
                Height = 200;
                Content = new TextBox
                {
                    Watermark = "Level Name",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = App.style.Margin
                };
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

       
        

        public MapLevelTab()
        {
            var cont = new Grid
            {
                Margin = App.style.Margin,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                RowDefinitions = new RowDefinitions("Auto,*"),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0))
            };
            this.Content = cont;

            var searchBox = new TextBox
            {
                Watermark = "Search ([map] | [level] | [level]@[map])",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            cont.Children.Add(searchBox);

            var grid = new Grid
            {
                Margin = App.style.Margin,
                RowDefinitions = new("*")
            };
            cont.Children.Add(grid);

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
            cont.Children.Add(_Scroller);
            Grid.SetRow(_Scroller, 1);

            cont.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(cont).Properties.IsRightButtonPressed)
                {
                    var elementUnderPointer = cont.InputHitTest(e.GetPosition(cont));

                    if (elementUnderPointer != cont && elementUnderPointer != _Scroller && elementUnderPointer.GetType().Name != "ScrollContentPresenter")
                        return; // If the right click is not on the MapLevelPanel itself, do nothing

                    e.Handled = true;

                    GlobalStaticUIData.CloseContext();
                    GlobalStaticUIData.CurrentContext = new ContextMenu();
                    var addMapItem = new MenuItem { Header = "Add Map" };
                    addMapItem.Click += (s, e) => OnCreateNewMap();
                    (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(addMapItem);
                    GlobalStaticUIData.OpenContext(cont);
                }
            };
        }

        private async Task OnCreateNewMap()
        {
            var result = await UiServices.DialogService.PromptTextAsync("Add Map", "Map Name:", "New Map", new(SizeToContent: DialogSizeToContent.HeightOnly));
            if (result == null)
                return;

            Guard.IsNotNull(EngineStates.ProjectState.CurrentProject, "CurrentProject");

            var mapDef = EngineServices.AssetsManager.CreateTransientAsset<MapDefinition>();
            mapDef.Name = result;
            
            EngineStates.ProjectState.CurrentProject.GameData.Maps.Add(mapDef);
            
            AddMapToUi(mapDef);
            Logger.Info($"Map '{result}' created.");
        }
        
        private void AddMapToUi(MapDefinition mapDef)
        {
            var map = new MapItem(mapDef);
            _MapList.Children.Add(map);
        }
        
        public static TabItem CreateTab(Window host)
        {
            // Need to do this to avoid create "multiple" instances...
            // In fact, even if we don't create multiple instances, it still crashes the application due to creating "multiple" instances of the same control.
            // Weird issues, but well, this works for now.
            var tab = new TabItem
            {
                Header = "Map/Level"
            };

            tab.Content = new MapLevelTab();

            return tab;
        }
    }
}

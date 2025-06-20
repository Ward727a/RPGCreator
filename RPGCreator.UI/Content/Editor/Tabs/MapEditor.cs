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
using Avalonia.Controls;
using RPGCreator.Core;
using RPGCreator.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Content.Editor.Tabs
{
    public class MapEditor : UserControl, ITab
    {
        public ScrollViewer BodyScroller { get; private set; }
        public Grid Body { get; private set; }
        public StackPanel BodyContent { get; private set; }

        // Sections

        #region LayersSection
        public ClosableBox LayersBox { get; private set; }
        public StackPanel LayersBody { get; private set; }
        public TextBlock SelectedLayerText { get; private set; }
        public Button AddLayerButton { get; private set; }
        public ListBox LayersList { get; private set; }
        #endregion

        #region MapPropertiesSection
        public ClosableBox MapPropertiesBox { get; private set; }
        public StackPanel MapPropertiesBody { get; private set; }
        public TextBlock MapNameText { get; private set; }
        public TextBlock MapSizeText { get; private set; }
        public TextBlock MapDescriptionText { get; private set; }
        public TextBlock MapEntitiesNumberText { get; private set; }
        public TextBlock MapLayersNumberText { get; private set; }
        #endregion

        // Constructor

        private MapEditor()
        {
            BodyScroller = new ScrollViewer
            {
            };

            Body = new Grid
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };
            BodyScroller.Content = Body;

            BodyContent = new StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
            Body.Children.Add(BodyContent);

            Content = BodyScroller;

            // Initialize the map editor UI components here
            InitializeComponents();
        }

        // Functions

        protected void InitializeComponents()
        {
            // Add UI components for the map editor, such as buttons, grids, etc.
            // This is just a placeholder; you can add actual UI elements as needed.
            BodyContent.Children.Add(new TextBlock { 
                Text = "Map Editor is under construction.",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center
            });

            #region LayersSection
            
            LayersBody = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Margin = App.style.Margin
            };

            var selectedLayerText = new TextBlock
            {
                Text = "Selected Layer: None",
                Margin = App.style.Margin,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center
            };

            LayersBody.Children.Add(selectedLayerText);

            AddLayerButton = new Button
            {
                Content = "Add Layer",
                Margin = App.style.Margin,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            LayersBody.Children.Add(AddLayerButton);

            AddLayerButton.Click += (s, e) =>
            {
                // Logic to add a new layer
                var newLayerName = $"Layer {LayersList.Items.Count + 1}";
                LayersList.Items.Add(newLayerName);
                LayersList.SelectedItem = newLayerName;
                selectedLayerText.Text = $"Selected Layer: {newLayerName}";
            };

            var separator = new Separator
            {
                Margin = App.style.Margin
            };

            LayersBody.Children.Add(separator);

            LayersList = new ListBox
            {
                Margin = App.style.Margin,
                SelectionMode = SelectionMode.Single,
            };
            LayersList.Items.Add("Test1");
            LayersList.Items.Add("Test2");
            LayersList.Items.Add("Test3");
            LayersList.SelectionChanged += (s, e) =>
            {
                if (LayersList.SelectedItem != null)
                {
                    selectedLayerText.Text = $"Selected Layer: {LayersList.SelectedItem}";
                }
            };

            LayersBody.Children.Add(LayersList);

            LayersBox = new ClosableBox(LayersBody, "Layers");

            BodyContent.Children.Add(LayersBox);

            #endregion

            #region MapPropertiesSection
            MapPropertiesBody = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Margin = App.style.Margin
            };

            MapNameText = new TextBlock
            {
                Text = "Map Name: placeholder",
                Margin = App.style.Margin,
            };

            MapPropertiesBody.Children.Add(MapNameText);

            MapSizeText = new TextBlock
            {
                Text = "Map Size: 100x100",
                Margin = App.style.Margin,
            };

            MapPropertiesBody.Children.Add(MapSizeText);

            MapDescriptionText = new TextBlock
            {
                Text = "Map Description: This is a placeholder description.",
                Margin = App.style.Margin,
                TextWrapping = Avalonia.Media.TextWrapping.WrapWithOverflow
            };

            MapPropertiesBody.Children.Add(MapDescriptionText);

            MapEntitiesNumberText = new TextBlock
            {
                Text = "Entities: 0",
                Margin = App.style.Margin,
            };

            MapPropertiesBody.Children.Add(MapEntitiesNumberText);

            MapLayersNumberText = new TextBlock
            {
                Text = "Layers: 0",
                Margin = App.style.Margin,
            };

            MapPropertiesBody.Children.Add(MapLayersNumberText);

            MapPropertiesBox = new ClosableBox(MapPropertiesBody, "Map Properties", true);

            BodyContent.Children.Add(MapPropertiesBox);

            #endregion

            RegisterEvents();
        }

        protected void RegisterEvents()
        {
            EngineCore.Instance.Data.EditedMapChanged += Data_EditedMapChanged;
        }

        private void Data_EditedMapChanged(object? sender, EventArgs e)
        {
            RefreshMapProperties();
            RefreshLayersList();
        }

        public void RefreshMapProperties()
        {
            if(EngineCore.Instance.Data.EditedMap == null)
                return;
            MapNameText.Text = $"Map Name: {EngineCore.Instance.Data.EditedMap.Name}";
            MapSizeText.Text = $"Map Size: {EngineCore.Instance.Data.EditedMap.Size.Width}x{EngineCore.Instance.Data.EditedMap.Size.Height}";
            MapDescriptionText.Text = $"Map Description: {EngineCore.Instance.Data.EditedMap.Description}";
            MapEntitiesNumberText.Text = $"Entities: 0 (not working yet)";
            MapLayersNumberText.Text = $"Layers: {EngineCore.Instance.Data.EditedMap.Layers.Count}";
        }

        public void RefreshLayersList()
        {
            if (EngineCore.Instance.Data.EditedMap == null)
                return;
            LayersList.Items.Clear();
            foreach (var layer in EngineCore.Instance.Data.EditedMap.Layers)
            {
                LayersList.Items.Add(layer.Name);
            }
        }

        public static TabItem CreateTab(Window host)
        {
            // Need to do this to avoid create "multiple" instances...
            // In fact, even if we don't create multiple instances, it still crashes the application due to creating "multiple" instances of the same control.
            // Weird issues, but well, this works for now.
            var tab = new TabItem
            {
                Header = "Map Editor",
                Content = new MapEditor()
            };

            return tab;
        }
    }
}

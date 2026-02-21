#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 
#endregion
using Avalonia.Controls;
using RPGCreator.Core.Types;
using System;
using System.Linq;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.UI.Content.Editor.LayersListComponents
{
    /// <summary>
    /// This class represents a component that displays a list of layers in the map editor.<br/>
    /// It can be seen in the Layers tab of the map editor.
    /// </summary>
    public class LayersListComponent : UserControl
    {
        #region Components

        public StackPanel LayersBody { get; private set; }
        public Button AddLayerButton { get; private set; }
        public ListBox LayersList { get; private set; }
        public TextBlock SelectedLayerText { get; private set; }

        #endregion

        public LayersListComponent()
        {
            CreateComponents();
            Content = LayersBody;
        }

        private void CreateComponents()
        {

            LayersBody = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
            };

            SelectedLayerText = new TextBlock
            {
                Text = "Selected Layer: None",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center
            };

            LayersBody.Children.Add(SelectedLayerText);

            AddLayerButton = new Button
            {
                Content = "Add Layer",
                Margin = App.style.Margin,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            LayersBody.Children.Add(AddLayerButton);

            AddLayerButton.Click += OnAddLayerButtonClicked;

            var separator = new Separator
            {
                Margin = App.style.Margin
            };

            LayersBody.Children.Add(separator);

            LayersList = new ListBox
            {
                SelectionMode = SelectionMode.Single,
            };

            LayersList.SelectionChanged += OnLayerSelected;

            LayersBody.Children.Add(LayersList);

            RefreshComponents();
            RegisterEvents();
        }

        protected void RefreshComponents()
        {
            LayersList.Items.Clear();
            RuntimeServices.OnceServiceReady((IMapService mapService) =>
            {
                if (mapService.CurrentLoadedMapDefinition == null) return;
                foreach (var layer in mapService.CurrentLoadedMapDefinition.TileLayers.OrderBy(l=>l.ZIndex))
                {
                    LayerItem layerItem = new LayerItem(layer);
                    LayersList.Items.Add(layerItem);
                    layerItem.LayerRemoved += () =>
                    {
                        RefreshComponents();
                    };
                }
            });
        }

        protected void RegisterEvents()
        {
            GlobalStates.MapState.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IMapState.CurrentMapDef))
                {
                    OnMapChanged();
                }
            };
            RuntimeServices.OnceServiceReady((IMapService mapService) => mapService.OnMapLoaded += (mapId) => OnMapChanged());
        }

        #region EventsHandlers

        protected void OnMapChanged()
        {
            // Refresh the layers list when the map changes
            RefreshComponents();
        }

        protected async void OnAddLayerButtonClicked(object? sender, EventArgs e)
        {
            if(!RuntimeServices.MapService.HasLoadedMap)
            {
                EditorUiServices.NotificationService.Error("No Map Loaded", "Please load a map before adding layers.");
                return;
            }

            var promptContent = new StackPanel()
                { Spacing = 5};
            var layerNameTextBox = new TextBox()
            {
                Watermark = "Enter layer name...",
                Text = "My Layer",
                InnerLeftContent = "Layer Name:",
            };
            var layerTypeComboBox = new ComboBox()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            
            layerTypeComboBox.Items.Add("Tile Layer");
            layerTypeComboBox.Items.Add("Auto Layer");
            layerTypeComboBox.Items.Add("Entity Layer");
            
            layerTypeComboBox.SelectedIndex = 0;
            
            promptContent.Children.Add(layerNameTextBox);
            promptContent.Children.Add(layerTypeComboBox);

            var result = await EditorUiServices.DialogService.ConfirmAsync(
                "Add Layer", 
                promptContent,
                confirmButtonText: "Add");

            if(!result)
            {
                // User canceled the dialog
                return;
            }
            
            var resultText = layerNameTextBox.Text;
            var resultType = layerTypeComboBox.SelectedIndex;
            
            if (string.IsNullOrWhiteSpace(resultText))
            {
                EditorUiServices.NotificationService.Warn("Invalid Layer Name", "Layer name cannot be empty.");
                return;
            }
            
            
            var layerName = resultText;
            if (!string.IsNullOrWhiteSpace(layerName))
            {
                // Logic to add a new layer with the specified name
                BaseLayerDef newLayer;
                
                switch (resultType)
                {
                    case 0: // Tile Layer
                        newLayer = EngineServices.AssetsManager.CreateAsset<TileLayerDefinition>();
                        break;
                    case 1: // Auto Layer
                        newLayer = EngineServices.AssetsManager.CreateAsset<AutoLayerDefinition>();
                        break;
                    case 2: // Entity Layer
                        newLayer = EngineServices.AssetsManager.CreateAsset<EntityLayerDefinition>();
                        break;
                    default:
                        EditorUiServices.NotificationService.Error("Error Adding Layer", "Invalid layer type selected.");
                        return;
                }
                
                newLayer.Name = layerName;
                newLayer.ZIndex = RuntimeServices.MapService.CurrentLoadedMapDefinition!.TileLayers.Count; // Set ZIndex to the last index
                newLayer.LayerIndex = RuntimeServices.MapService.GetLastLayerIndex() + 1; // Set LayerIndex to the next available index
                if(!RuntimeServices.MapService.HasLoadedMap)
                {
                    return;
                }

                if (RuntimeServices.MapService.TryAddLayer(newLayer))
                {
                    LayerItem newLayerItem = new LayerItem(newLayer);

                    newLayerItem.LayerRemoved += () => { RefreshComponents(); };

                    LayersList.Items.Add(newLayerItem);
                    LayersList.SelectedItem = newLayerItem;

                    SelectedLayerText.Text = $"Selected Layer: {layerName}";

                    newLayer.LayerIndex = RuntimeServices.MapService.GetLastLayerIndex();
                    RuntimeServices.MapService.SelectLayer(newLayer.LayerIndex);

                    return;
                }
                
                EditorUiServices.NotificationService.Error("Error Adding Layer", "Could not add the new layer. It may already exist?");
            }
        }

        private void OnLayerSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (LayersList.SelectedItem is not LayerItem layerItem) return;
            SelectedLayerText.Text = $"Selected Layer: {layerItem.Layer.Name}";
            RuntimeServices.MapService.SelectLayer(layerItem.Layer.LayerIndex);
        }

        #endregion
    }
}

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
using System;
using System.Linq;
using Avalonia;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.UI.Contexts;

namespace RPGCreator.UI.Content.Editor.LayersListComponents
{
    /// <summary>
    /// This class represents a component that displays a list of layers in the map editor.<br/>
    /// It can be seen in the Layers tab of the map editor.
    /// </summary>
    public class LayersListComponent : UserControl
    {

        public class LayerCreateModal : StackPanel
        {
            public readonly struct CreatedLayerEventArgs(string name, string key)
            {
                public readonly string Name = name;
                public readonly string Key = key;
            }

            [ExposeEventToPlugin("EditorLeftPanel.LayerPanel.LayerCreator")]
            public event Action<CreatedLayerEventArgs> LayerCreated;

            [ExposePropToPlugin("EditorLeftPanel.LayerPanel.LayerCreator")]
            public TextBox NameInput { get; } = new();
            [ExposePropToPlugin("EditorLeftPanel.LayerPanel.LayerCreator")]
            public ComboBox TypeInput { get; } = new();
            
            public LayerCreateModal()
            {
                Spacing = 5;
            }

            protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
            {
                base.OnAttachedToVisualTree(e);
                CreateComponents();
                RegisterEvents();
            }

            private void CreateComponents()
            {
                NameInput.Watermark = "Enter layer name...";
                NameInput.Text = "My Layer";
                NameInput.InnerLeftContent = "Layer Name:";
                
                TypeInput.HorizontalAlignment = HorizontalAlignment.Stretch;
                TypeInput.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                
                AddType("tile_layer", "Tile Layer", true);
                AddType("auto_layer", "Auto Layer");
                AddType("entity_layer", "Entity Layer");
            
                TypeInput.SelectedIndex = 0;
            
                Children.Add(NameInput);
                Children.Add(TypeInput);
            }

            [ExposeToPlugin("EditorLeftPanel.LayerPanel.LayerCreator")]
            public void AddType(string key, string typeName, bool selectedByDefault = false)
            {
                TypeInput.Items.Add(new ComboBoxItem()
                {
                    Content = typeName,
                    Tag = key
                });

                if (selectedByDefault)
                {
                    TypeInput.SelectedIndex = TypeInput.Items.Count - 1;
                }
            }

            private void RegisterEvents()
            {
            }

            public void LinkToExtension()
            {
                var context = new EditorLeftPanelLayerPanelLayerCreatorContext(
                    new EditorLeftPanelLayerPanelLayerCreatorContext.Config()
                    {
                        AddLayerCreated = (action) =>
                        {
                            LayerCreated += action;
                        },
                        RemoveLayerCreated = (action) =>
                        {
                            LayerCreated -= action;
                        },
                        AddType = AddType,
                        GetNameInput = () => NameInput,
                        GetTypeInput = () => TypeInput
                    });
                EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelLayerPanelLayerCreator, this, context);
            }
            
            internal void CallLayerCreated(string name, string key)
            {
                LayerCreated?.Invoke(new CreatedLayerEventArgs(name, key));
            }
        }
        
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
                Orientation = Orientation.Vertical,
            };

            SelectedLayerText = new TextBlock
            {
                Text = "Selected Layer: None",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center
            };

            LayersBody.Children.Add(SelectedLayerText);

            AddLayerButton = new Button
            {
                Content = "Add Layer",
                Margin = App.style.Margin,
                HorizontalAlignment = HorizontalAlignment.Center
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
            RuntimeServices.OnceServiceReady((IMapService mapService) =>
            {
                mapService.MapLoaded += (mapId) => OnMapChanged();
                mapService.SelectedLayer += def =>
                {
                    SelectedLayerText.Text = $"Selected Layer: {def.Name}";
                };
                mapService.AddedLayer += def =>
                {
                    LayerItem newLayerItem = new LayerItem(def);
                    
                    newLayerItem.LayerRemoved += () => { RefreshComponents(); };
                    
                    LayersList.Items.Add(newLayerItem);
                    LayersList.SelectedItem = newLayerItem;
                };
            });
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

            var promptContent = new LayerCreateModal();
            promptContent.LayerCreated += (args) =>
            {
                var layerName = args.Name;
                var layerType = args.Key;
                // Logic to add a new layer with the specified name
                BaseLayerDef newLayer;
                
                switch (layerType)
                {
                    case "tile_layer": // Tile Layer
                        newLayer = EngineServices.AssetsManager.CreateAsset<TileLayerDefinition>();
                        break;
                    case "auto_layer": // Auto Layer
                        newLayer = EngineServices.AssetsManager.CreateAsset<AutoLayerDefinition>();
                        break;
                    case "entity_layer": // Entity Layer
                        newLayer = EngineServices.AssetsManager.CreateAsset<EntityLayerDefinition>();
                        break;
                    default: // Not supported by default
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
                    newLayer.LayerIndex = RuntimeServices.MapService.GetLastLayerIndex();
                    RuntimeServices.MapService.SelectLayer(newLayer.LayerIndex);
                    return;
                }
                
                EditorUiServices.NotificationService.Error("Error Adding Layer", "Could not add the new layer. It may already exist?");
            };
            promptContent.LinkToExtension();

            var result = await EditorUiServices.DialogService.ConfirmAsync(
                "Add Layer", 
                promptContent,
                confirmButtonText: "Add");

            if(!result)
            {
                // User canceled the dialog
                return;
            }
            
            promptContent?.CallLayerCreated(promptContent.NameInput.Text ?? string.Empty, (promptContent.TypeInput.SelectedItem as ComboBoxItem)?.Tag as string ?? string.Empty);
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

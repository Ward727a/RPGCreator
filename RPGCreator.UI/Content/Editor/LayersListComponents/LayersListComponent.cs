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
using Avalonia.VisualTree;
using RPGCreator.Core;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.UI.Content.Editor.LayersListComponents
{
    /// <summary>
    /// This class represents a component that displays a list of layers in the map editor.<br/>
    /// It can be seen in the Layers tab of the map editor.
    /// </summary>
    public class LayersListComponent : UserControl
    {
        private MapEditorContext _context;
        
        #region Components

        public StackPanel LayersBody { get; private set; }
        public Accordion Body { get; private set; }
        public Button AddLayerButton { get; private set; }
        public ListBox LayersList { get; private set; }
        public TextBlock SelectedLayerText { get; private set; }

        #endregion

        public LayersListComponent(MapEditorContext context)
        {
            _context = context;
            CreateComponents();
            Content = Body;
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

            Body = new Accordion(LayersBody, "Layers");

            RefreshComponents();
            RegisterEvents();
        }

        protected void RefreshComponents()
        {
            LayersList.Items.Clear();
            if (_context.Map == null) return;
            foreach (var layer in _context.Map.TileLayers.OrderBy(l=>l.ZIndex))
            {
                LayerItem layerItem = new LayerItem(layer);
                LayersList.Items.Add(layerItem);
                layerItem.LayerRemoved += () =>
                {
                    RefreshComponents();
                };
            }
            if (LayersList.Items.Count > 0)
            {
                LayersList.SelectedIndex = 0;
                SelectedLayerText.Text = $"Selected Layer: {((LayerItem)LayersList.SelectedItem).Layer.Name}";
                EngineCore.Instance.Data.SelectedLayer = ((LayerItem)LayersList.SelectedItem).Layer;
            }
            else
            {
                SelectedLayerText.Text = "Selected Layer: None";
                EngineCore.Instance.Data.SelectedLayer = null;
            }
        }

        protected void RegisterEvents()
        {
            _context.MapChanged += OnMapChanged;
        }

        #region EventsHandlers

        protected void OnMapChanged()
        {
            // Refresh the layers list when the map changes
            RefreshComponents();
        }

        protected void OnAddLayerButtonClicked(object? sender, EventArgs e)
        {
            if (this.GetVisualRoot() is not Window win) return;

            var popup = new Window
            {
                Title = "Add Layer",
                SizeToContent = Avalonia.Controls.SizeToContent.WidthAndHeight,
                MinWidth = 300,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
            };

            var stackPanel = new StackPanel
            {
                Margin = App.style.Margin
            };
            popup.Content = stackPanel;

            var layerNameTextBox = new TextBox
            {
                Watermark = "Enter layer name",
                Margin = App.style.Margin
            };
            stackPanel.Children.Add(layerNameTextBox);

            var addButton = new Button
            {
                Content = "Add Layer",
                Margin = App.style.Margin,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            addButton.Click += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(layerNameTextBox.Text))
                {
                    // Logic to add a new layer with the specified name
                    var newLayerName = layerNameTextBox.Text;
                    TileLayerDefinition layer = new TileLayerDefinition()
                    {
                        Name = newLayerName
                    };
                    
                    if(_context.Map == null)
                    {
                        return;
                    }

                    layer.ZIndex = _context.Map.TileLayers.Count - 1; // Set ZIndex to the last index
                    // layer.ZIndexChanged += (value) =>
                    // {
                    //     RefreshComponents();
                    // };

                    _context.Map?.AddLayer(layer);

                    LayerItem newLayerItem = new LayerItem(layer);

                    newLayerItem.LayerRemoved += () =>
                    {
                        RefreshComponents();
                    };

                    LayersList.Items.Add(newLayerItem);
                    LayersList.SelectedItem = newLayerItem;

                    SelectedLayerText.Text = $"Selected Layer: {newLayerName}";

                    _context.SelectedLayer = layer;

                    popup.Close();
                }
                else
                {
                    // Show an error message or handle empty input
                }
            };
            layerNameTextBox.KeyDown += (s, args) =>
            {
                if (args.Key == Avalonia.Input.Key.Enter)
                {
                    addButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent, addButton));
                }
            };
            stackPanel.Children.Add(addButton);

            popup.Opened += (s, e) =>
            {
                layerNameTextBox.Focus();
                // Focus the TextBox when the popup opens
            };

            popup.ShowDialog(win).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Handle any errors that occurred while showing the popup
                    Console.WriteLine("Error showing popup: " + t.Exception?.Message);
                }
            });
        }

        private void OnLayerSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (LayersList.SelectedItem is not LayerItem layerItem) return;
            SelectedLayerText.Text = $"Selected Layer: {layerItem.Layer.Name}";
            _context.SelectedLayer = layerItem.Layer;
        }

        #endregion
    }
}

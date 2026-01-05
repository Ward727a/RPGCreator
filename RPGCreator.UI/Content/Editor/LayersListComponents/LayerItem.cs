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
using RPGCreator.Core.Types;
using RPGCreator.Core.Types.Windows;
using System;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;

namespace RPGCreator.UI.Content.Editor.LayersListComponents
{
    /// <summary>
    /// This class represents a single layer item in the layers list component.<br/>
    /// It is used to display the name of a layer in the layers list and to retrieve the layer associated with the item.<br/>
    /// It is used in the <see cref="LayersListComponent"/> class to display the list of layers in the map editor.<br/>
    /// </summary>
    public class LayerItem : UserControl
    {

        public event Action? LayerRemoved;

        #region Components

        public StackPanel Body { get; private set; }
        public NumericUpDown ZIndexSelector { get; private set; }
        public TextBlock LayerNameText { get; private set; }

        #endregion
        public BaseLayerDef Layer { get; private set; } = null!;

        public LayerItem(BaseLayerDef layer)
        {
            Layer = layer ?? throw new ArgumentNullException(nameof(layer), "Layer cannot be null.");
            CreateComponents();
        }

        private void CreateComponents()
        {
            Body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Background = Avalonia.Media.Brushes.Transparent,
            };

            ZIndexSelector = new NumericUpDown
            {
                Value = Layer.ZIndex,
                Minimum = -1000,
                Maximum = 1000,
                Width = 60,
                Margin = new Avalonia.Thickness(5, 0, 5, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontSize = App.style.SmallTextFontSize,
            };
            ZIndexSelector.ValueChanged += ZIndexSelector_ValueChanged;
            ToolTip.SetTip(ZIndexSelector, "Z-Index of the layer. This determines the rendering order of the layer.\nLayers with a higher Z-Index are rendered on top of layers with a lower Z-Index.");
            Body.Children.Add(ZIndexSelector);

            string LayerTypeText;

            switch (Layer)
            {
                case TileLayerDefinition:
                    LayerTypeText = "[TL] ";
                    break;
                case AutoLayerDefinition:
                    LayerTypeText = "[AL] ";
                    break;
                default:
                    LayerTypeText = "[??] ";
                    break;
            }

            LayerNameText = new TextBlock
            {
                Text = $"{LayerTypeText} {Layer.Name}",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };

            Body.Children.Add(LayerNameText);

            Body.PointerPressed += Body_PointerPressed;

            Content = Body;
        }

        private void Body_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if(e.GetCurrentPoint(Body).Properties.IsRightButtonPressed)
            {
                e.Handled = true;

                GlobalStaticUIData.CloseContext();
                GlobalStaticUIData.CurrentContext = new ContextMenu();
                var removeLayerItem = new MenuItem { Header = "Remove Layer" };
                removeLayerItem.Click += (s, e) => OnRemoveLayer();
                (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(removeLayerItem);
                GlobalStaticUIData.OpenContext(Body);
            }
        }

        private void OnRemoveLayer()
        {

            var confirmation = new ConfirmDialog("Remove Layer", "Are you sure you want to remove this layer? This action cannot be undone.", "Remove", "Cancel");
            confirmation.Confirmed += () =>
            {
                if (Layer != null && Layer is BaseLayerDef mapLayer)
                {
                    // Remove the layer from the engine data
                    EngineState.EditorState.CurrentMap?.RemoveLayer(mapLayer);
                    LayerRemoved?.Invoke();
                }
            };

            confirmation.ShowDialog((Window)this.GetVisualRoot()!).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Handle any errors that occurred while showing the confirmation dialog
                    Console.WriteLine("Error showing confirmation dialog: " + t.Exception?.Message);
                }
            });

        }


        #region EventsHandlers

        private void ZIndexSelector_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (Layer != null && e.NewValue.HasValue)
            {
                Layer.ZIndex = (int)e.NewValue.Value;
            }
        }

        #endregion
    }
}

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
using Avalonia.Controls.Primitives;
using RPGCreator.Core;
using RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;
using RPGCreator.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Content.Editor.Toolbar
{
    public class ToolbarControl : UserControl
    {

        #region StaticData

        public record BrushType(string Name, string IconPath, IBrush brush);
        private Dictionary<string, BrushType> _Brushes = new()
        {
            ["Simple Brush"] = new ("Simple Brush", "", new SimpleBrush()),
            ["Eraser Brush"] = new ("Eraser Brush", "", new EraserBrush()),
            // Add more brushes as needed
        };

        #endregion

        public ToggleButton? LastChecked { get; private set; } = null; // Initialize to null, will be set when a button is checked

        #region Components

        public StackPanel Body { get; private set; }

        #region Drawing Mode
        public ToggleButton DrawButton { get; private set; }
        public Button DrawOptionButton { get; private set; }
        public Flyout DrawOptionFlyout { get; private set; }
        public StackPanel DrawOptionContent { get; private set; }
        public ComboBox BrushSelector { get; private set; }

        public StackPanel BrushSizePanel { get; private set; }
        public NumericUpDown BrushSizeSelector { get; private set; }

        public StackPanel BrushPreviewPanel { get; private set; } // Panel to show brush preview if needed
        public CheckBox BrushPreviewCheckBox { get; private set; } // CheckBox to toggle brush preview visibility
        #endregion

        #region Place Mode

        public ToggleButton PlaceButton { get; private set; }
        //public Button PlaceOptionButton { get; private set; }
        //public Flyout PlaceOptionFlyout { get; private set; }
        //public StackPanel PlaceOptionContent { get; private set; }
        //public ComboBox PlaceSelector { get; private set; }

        #endregion


        #endregion

        public ToolbarControl()
        {
            CreateComponents();
            LoadBrushes();
            Content = Body;
        }

        protected void CreateComponents()
        {
            Body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Height = 32,
            };

            #region Drawing Mode

            DrawButton = new ToggleButton
            {
                Content = "Draw",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                CornerRadius = new(2, 0, 0, 2),
            };
            DrawButton.IsCheckedChanged += DrawButton_IsCheckedChanged;
            Body.Children.Add(DrawButton);
            ToolTip.SetTip(DrawButton, "Toggle drawing mode. When enabled, you can draw on the map.");

            DrawOptionButton = new Button
            {
                Content = "⋮",
                FontStretch = Avalonia.Media.FontStretch.UltraCondensed,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                CornerRadius = new(0, 2, 2, 0),
                Margin = new Avalonia.Thickness(0, 0, 4, 0), // Add margin to the right for spacing
            };

            DrawOptionContent = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };

            DrawOptionFlyout = new Flyout
            {
                ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
                Content = DrawOptionContent,
                Placement = PlacementMode.Bottom,
            };
            Flyout.SetAttachedFlyout(DrawOptionButton, DrawOptionFlyout);

            DrawOptionButton.Click += (s, e) => Flyout.ShowAttachedFlyout(DrawOptionButton); // Show the flyout when the button is clicked

            Body.Children.Add(DrawOptionButton);

            BrushSelector = new ComboBox
            {
                Width = 150,
                AutoScrollToSelectedItem = true,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
            BrushSelector.Items.Add("Simple Brush");
            BrushSelector.SelectionChanged += BrushSelector_SelectionChanged;
            DrawOptionContent.Children.Add(BrushSelector);

            ToolTip.SetTip(BrushSelector, "Select the brush type to use for drawing on the map.");

            var textSeparator = new TextSeparator
            {
                Text = "Brush Options",
            };

            DrawOptionContent.Children.Add(textSeparator);

            BrushSizePanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 4),
            };
            var brushSizeLabel = new TextBlock
            {
                Text = "Brush Size:",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 4),
            };
            BrushSizePanel.Children.Add(brushSizeLabel);
            BrushSizeSelector = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 100,
                Value = 5, // Default brush size
                Width = 60,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(4, 0, 0, 0),
            };
            BrushSizeSelector.ValueChanged += (s, e) => 
            {
                // Handle brush size change
                if (EngineCore.Instance.Data.EditorSettings.BrushType is IBrushResizeFeature brush)
                {
                    brush.ResizeBrush((int)BrushSizeSelector.Value);
                    Console.WriteLine($"Brush size changed to: {BrushSizeSelector.Value}");
                }
                else
                {
                    Console.WriteLine("Current brush does not support resizing.");
                }
            };
            BrushSizePanel.Children.Add(BrushSizeSelector);

            ToolTip.SetTip(BrushSizeSelector, "Adjust the size of the brush used for drawing on the map.");

            DrawOptionContent.Children.Add(BrushSizePanel);

            BrushPreviewPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 4),
            };
            BrushPreviewCheckBox = new CheckBox
            {
                Content = "Enable Brush Preview",
                IsChecked = true, // Default to enabled
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 4),
            };
            BrushPreviewCheckBox.IsCheckedChanged += (s, e) =>
            {
                // Handle brush preview toggle
                if (EngineCore.Instance.Data.EditorSettings.BrushType is IBrushPreviewFeature brush)
                {
                    brush.IsPreviewEnabled = BrushPreviewCheckBox.IsChecked == true;
                    Console.WriteLine($"Brush preview enabled: {brush.IsPreviewEnabled}");
                }
                else
                {
                    Console.WriteLine("Current brush does not support preview.");
                }
            };
            BrushPreviewPanel.Children.Add(BrushPreviewCheckBox);
            ToolTip.SetTip(BrushPreviewCheckBox, "Enable or disable brush preview while drawing on the map.");
            DrawOptionContent.Children.Add(BrushPreviewPanel);

            #endregion

            #region Place Mode

            PlaceButton = new ToggleButton // Tool that allows placing entities on the map (like characters, doors, chests, etc.)
            {
                Content = "Place",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                CornerRadius = new(2),
                Margin = new Avalonia.Thickness(0, 0, 4, 0)
            };
            PlaceButton.IsCheckedChanged += PlaceButton_IsCheckedChanged;
            Body.Children.Add(PlaceButton);

            #endregion
        }

        public void UseTool() { } // Placeholder, this method can be used to force the user to use a specific tool if needed

        private void PlaceButton_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // This method handles the toggle state change of the Place button
            if(sender is ToggleButton button)
            {
                if (button.IsChecked == true)
                {
                    // Logic to enable place mode
                    Console.WriteLine("Place mode enabled.");
                    EngineCore.Instance.Data.EditorSettings.IsPlacing = true; // Set the placing mode in editor settings
                    if (LastChecked != null && LastChecked != button)
                    {
                        LastChecked.IsChecked = false; // Uncheck the last checked button
                    }
                    LastChecked = button; // Update the last checked button
                }
                else
                {
                    // Logic to disable place mode
                    EngineCore.Instance.Data.EditorSettings.IsPlacing = false; // Set the placing mode in editor settings
                    Console.WriteLine("Place mode disabled.");
                    if (LastChecked == button)
                    {
                        LastChecked = null; // Reset last checked if the current button is unchecked
                    }
                }
            }
        }

        #region BrushesFeatures

        protected void LoadBrushes()
        {
            // Load brushes into the BrushSelector ComboBox
            BrushSelector.Items.Clear();
            foreach (var brush in _Brushes.Keys)
            {
                BrushSelector.Items.Add(brush);
            }
            if (BrushSelector.Items.Count > 0)
            {
                BrushSelector.SelectedIndex = 0; // Select the first brush by default
            }
        }

        private void ReloadBrushAvailableFeatures()
        {
            if(EngineCore.Instance.Data.EditorSettings.BrushType is IBrushResizeFeature brushResizeFeature)
            {
                // If the current brush supports resizing, enable the brush size panel
                BrushSizePanel.IsVisible = true;
                BrushSizeSelector.Minimum = brushResizeFeature.MinSize;
                BrushSizeSelector.Maximum = brushResizeFeature.MaxSize;
                BrushSizeSelector.Value = brushResizeFeature.GetBrushSize(); // Set the current brush size
                BrushSizeSelector.Increment = brushResizeFeature.Step; // Set the step size for resizing
                BrushSizeSelector.ParsingNumberStyle = System.Globalization.NumberStyles.Integer;
            }
            else
            {
                // Otherwise, hide the brush size panel
                BrushSizePanel.IsVisible = false;
            }

            if (EngineCore.Instance.Data.EditorSettings.BrushType is IBrushPreviewFeature brushPreviewFeature)
            {
                // If the current brush supports preview, enable the brush preview checkbox
                BrushPreviewPanel.IsVisible = true;
                BrushPreviewCheckBox.IsChecked = brushPreviewFeature.IsPreviewEnabled; // Set the current preview state
            }
            else
            {
                // Otherwise, hide the brush preview panel
                BrushPreviewPanel.IsVisible = false;
            }
        }

        #endregion

        #region EventsHandlers

        private void DrawButton_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                // Handle the draw button toggle state change
                if (button.IsChecked == true)
                {
                    // Logic to enable drawing mode
                    Console.WriteLine("Drawing mode enabled.");
                    EngineCore.Instance.Data.EditorSettings.IsDrawing = true; // Set the drawing mode in editor settings
                    if (LastChecked != null && LastChecked != button)
                    {
                        LastChecked.IsChecked = false; // Uncheck the last checked button
                    }
                    LastChecked = button; // Update the last checked button
                }
                else
                {
                    // Logic to disable drawing mode
                    EngineCore.Instance.Data.EditorSettings.IsDrawing = false; // Set the drawing mode in editor settings
                    Console.WriteLine("Drawing mode disabled.");
                    if (LastChecked == button)
                    {
                        LastChecked = null; // Reset last checked if the current button is unchecked
                    }
                }
            }
        }
        private void BrushSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedBrush)
            {
                // Handle the brush selection change
                if (_Brushes.TryGetValue(selectedBrush, out var brushData))
                {
                    Console.WriteLine($"Selected brush: {brushData.Name}");
                    EngineCore.Instance.Data.EditorSettings.BrushType = brushData.brush;
                    ReloadBrushAvailableFeatures(); // Reload features based on the selected brush
                }
                else
                {
                    Console.WriteLine($"Brush '{selectedBrush}' not found.");
                }
            }
        }


        #endregion
    }
}

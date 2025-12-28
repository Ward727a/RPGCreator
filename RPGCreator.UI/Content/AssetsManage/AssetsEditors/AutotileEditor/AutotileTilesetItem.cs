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
using RPGCreator.Core.Types.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Layout;
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor
{
    public class AutotileTilesetItem : UserControl
    {

        public event Action? TilesetSelected;

        public ITilesetDef TilesetDef { get; set; }

        public AutotileTilesetItem(ITilesetDef tilesetDef)
        { 
        
            TilesetDef = tilesetDef ?? throw new ArgumentNullException(nameof(tilesetDef), "Tileset cannot be null");
            CreateComponents();
            RegisterEvents();
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);

        }

        private void CreateComponents()
        {
            // Initialize components here, e.g., setting up the UI elements
            // This is a placeholder for actual UI component creation logic
            // For example, you might create a TextBlock to display the Tileset name
            var backPanel = new StackPanel()
            {
                Width = 180,
                Orientation = Orientation.Horizontal,
                // Light gray with 20% opacity
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(20, 211, 211, 211)),
                Spacing = 5,
                Margin = new Avalonia.Thickness(5),
            };

            this.Content = backPanel;
            
            var image = new Image
            {
                Source = TilesetDef.GetBitmap(),
                Width = 64,
                Height = 64,
                Margin = new Avalonia.Thickness(5)
            };
            backPanel.Children.Add(image);
            
            var textBlock = new TextBlock
            {
                Text = TilesetDef.Name,
                Margin = new Avalonia.Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            backPanel.Children.Add(textBlock);
        }

        private void RegisterEvents()
        {
            // Register events here, e.g., click events for selecting the tileset
            this.PointerPressed += (s, e) =>
            {
                TilesetSelected?.Invoke();
            };
        }

    }
}

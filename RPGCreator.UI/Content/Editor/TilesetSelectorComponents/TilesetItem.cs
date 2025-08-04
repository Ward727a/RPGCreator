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
using Avalonia.Media.Imaging;
using RPGCreator.Core.Type.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Content.Editor.TilesetSelectorComponents
{
    public class TilesetItem : UserControl
    {
        public bool Error { get; private set; } = false;
        public StackPanel Body { get; private set; }
        public TextBlock NameTextBlock { get; private set; }
        public Image TilesetImage { get; private set; }
        public ITileset Tileset { get; private set; }

        public TilesetItem(ITileset tileset)
        {
            Body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            };

            NameTextBlock = new TextBlock
            {
                Text = tileset.Name,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5, 0, 0, 0),
            };

            if(tileset.ImagePath == null)
            {
                throw new ArgumentNullException(nameof(tileset.ImagePath), "Tileset image path cannot be null.");
            }

            if(!File.Exists(tileset.ImagePath))
            {
                Error = true;
                return; // If the file doesn't exist, we can skip loading the image.
            }

            TilesetImage = new Image
            {
                Source = tileset.GetBitmap(),
                Width = 32,
                Height = 32,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            tileset.ImageChanged += (sender, e) =>
            {
                TilesetImage.Source = tileset.GetBitmap();
            };

            Tileset = tileset;

            Body.Children.Add(TilesetImage);
            Body.Children.Add(NameTextBlock);

            this.Content = Body;
        }

    }
}

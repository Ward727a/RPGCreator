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
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.UI.Content.Editor.TilesetSelectorComponents
{
    public class TilesetItem : UserControl
    {
        public bool Error { get; private set; } = false;
        public StackPanel Body { get; private set; }
        public TextBlock NameTextBlock { get; private set; }
        public Image TilesetImage { get; private set; }
        public BaseTilesetDef TilesetDef { get; private set; }

        public TilesetItem(BaseTilesetDef tilesetDef)
        {
            Body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            };

            NameTextBlock = new TextBlock
            {
                Text = tilesetDef.Name,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5, 0, 0, 0),
            };

            TilesetImage = new Image
            {
                Source = EngineServices.ResourcesService.Load<Bitmap>(tilesetDef.ImagePath),
                Width = 32,
                Height = 32,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            tilesetDef.ImageChanged += () =>
            {
                TilesetImage.Source = EngineServices.ResourcesService.Load<Bitmap>(tilesetDef.ImagePath);
            };

            TilesetDef = tilesetDef;

            Body.Children.Add(TilesetImage);
            Body.Children.Add(NameTextBlock);

            this.Content = Body;
        }

    }
}

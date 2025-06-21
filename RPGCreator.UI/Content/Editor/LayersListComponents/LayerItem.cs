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
using RPGCreator.Core.Type.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Content.Editor.LayersListComponents
{
    /// <summary>
    /// This class represents a single layer item in the layers list component.<br/>
    /// It is used to display the name of a layer in the layers list and to retrieve the layer associated with the item.<br/>
    /// It is used in the <see cref="LayersListComponent"/> class to display the list of layers in the map editor.<br/>
    /// </summary>
    public class LayerItem : UserControl
    {

        #region Components

        public StackPanel Body { get; private set; }
        public TextBlock LayerNameText { get; private set; }

        #endregion
        public MapLayer Layer { get; private set; } = null!;

        public LayerItem(MapLayer layer)
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
            };

            LayerNameText = new TextBlock
            {
                Text = Layer.Name,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };

            Body.Children.Add(LayerNameText);

            Content = Body;
        }
    }
}

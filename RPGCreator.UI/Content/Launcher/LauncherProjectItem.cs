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
using Avalonia.Media;
using System;
using RPGCreator.SDK.Projects;

namespace RPGCreator.UI.Content.Launcher
{
    public class LauncherProjectItem : UserControl
    {

        public event EventHandler<IBaseProject>? ProjectSelected;

        public LauncherProjectItem(IBaseProject project)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Background = Brushes.Transparent,
            };

            stackPanel.Children.Add(new TextBlock
            {
                Text = project.Name,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new(0, 20, 0, 0),
            });
            stackPanel.Children.Add(new Separator
            {
                Margin = new (0, 20, 0, 0),
            });

            PointerPressed += (_, e) =>
            {
                ProjectSelected?.Invoke(this, project);
            };

            PointerEntered += (_, e) =>
            {
                // Change cursor to hand when hovering over the item
                Cursor = Avalonia.Input.Cursor.Parse("hand");
                // Set the background less transparent
                stackPanel.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)); // Semi-transparent black
            };
            PointerExited += (_, e) =>
            {
                // Reset cursor when not hovering over the item
                Cursor = Avalonia.Input.Cursor.Default;
                // Reset background color
                stackPanel.Background = Brushes.Transparent;
            };

            Content = stackPanel; 
        }
    }
}

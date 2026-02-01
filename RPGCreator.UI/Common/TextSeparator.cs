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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace RPGCreator.Core.Types
{
    public class TextSeparator : UserControl
    {
        public string Text { get; set; } = string.Empty;
        public int SeparatorWidth { get; set; } = 20; // Default width for the separators

        public Grid? MainPanel { get; private set; } = null;
        public TextBlock? SeparatorText { get; private set; } = null;
        public Separator? LeftSeparator { get; private set; } = null;
        public Separator? RightSeparator { get; private set; } = null;

        public TextSeparator()
        {
            // Add control
            HorizontalAlignment = HorizontalAlignment.Stretch;

            MainPanel = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ColumnDefinitions = new ColumnDefinitions("Auto, Auto, Auto"),
            };

            LeftSeparator = new Separator
            {
                VerticalAlignment = VerticalAlignment.Center,
                Width = 20, // Set a fixed width for the right separator
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 5, 0),
            };
            MainPanel.Children.Add(LeftSeparator);
            Grid.SetColumn(LeftSeparator, 0);
            SeparatorText = new TextBlock
            {
                Name = "SeparatorText",
                Text = Text,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray),
            };
            MainPanel.Children.Add(SeparatorText);
            Grid.SetColumn(SeparatorText, 1);

            RightSeparator = new Separator
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(5, 0, 0, 0),
                Width = 20, // Set a fixed width for the right separator
            };
            MainPanel.Children.Add(RightSeparator);
            Grid.SetColumn(RightSeparator, 2);

            this.Content = MainPanel;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            SeparatorText.Text = Text;
            LeftSeparator.Width = SeparatorWidth;
            RightSeparator.Width = SeparatorWidth;

        }
    }
}

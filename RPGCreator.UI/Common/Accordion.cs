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
using RPGCreator.UI;

namespace RPGCreator.Core.Types
{
    public class Accordion : UserControl
    {
        // Bottom arrow text
        const string DOWN_ARROW_TEXT = "▼";
        // Up arrow text
        const string UP_ARROW_TEXT = "▲";


        public event Action? Closed;
        public event Action? Opened;

        public bool IsClosed { get; private set; } = false;

        #region Components

        public StackPanel Body { get; private set; }
        public TextBlock CloseStatus { get; private set; }
        public TextBlock TitleTextBlock { get; private set; }
        public StackPanel HeaderPanel { get; private set; }
        public Border ContentBorder { get; private set; }
        public Grid ContentPanel { get; private set; }

        #endregion

        public Accordion(Control content, string title = "Closable box", bool closed = false)
        {
            IsClosed = closed;

            // Initialize components

            Body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Margin = App.style.Margin
            };

            CloseStatus = new TextBlock { Text = DOWN_ARROW_TEXT, FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, TextAlignment = Avalonia.Media.TextAlignment.Center, Margin = App.style.Margin };

            TitleTextBlock = new TextBlock { Text = title, Margin = App.style.Margin };

            HeaderPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 0, 0, 0)),
                Children = { CloseStatus, TitleTextBlock }
            };

            Body.Children.Add(HeaderPanel);

            HeaderPanel.PointerPressed += (s, e) => ToggleContentVisibility();
            HeaderPanel.PointerEntered += (s, e) =>
            {
                // Change cursor to hand when hovering over the header
                this.Cursor = Avalonia.Input.Cursor.Parse("hand");
                // Set the background less transparent
                HeaderPanel.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(150, 0, 0, 0)); // More opaque
            };
            HeaderPanel.PointerExited += (s, e) =>
            {
                // Reset cursor to default when not hovering
                this.Cursor = Avalonia.Input.Cursor.Default;
                // Reset the background to semi-transparent
                HeaderPanel.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 0, 0, 0)); // Semi-transparent black
            };

            ContentBorder = new Border
            {
                Background = Avalonia.Media.Brushes.Transparent,
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 0, 0, 0)),
            };
            Body.Children.Add(ContentBorder);

            ContentPanel = new Grid
            {
                Background = Avalonia.Media.Brushes.Transparent,
                Children = { content },
                Margin = App.style.Margin
            };

            ContentBorder.Child = ContentPanel;

            Content = Body;

            if(IsClosed)
            {
                ContentBorder.IsVisible = false; // Hide content if closed
                CloseStatus.Text = DOWN_ARROW_TEXT; // Show up arrow when closed
            }
        }

        protected void ToggleContentVisibility()
        {
            if(ContentBorder.IsVisible)
            {
                ContentBorder.IsVisible = false;
                CloseStatus.Text = DOWN_ARROW_TEXT; // Change to up arrow
                IsClosed = true; // Mark as closed
                Closed?.Invoke(); // Invoke the closed event
            }
            else
            {
                ContentBorder.IsVisible = true;
                CloseStatus.Text = UP_ARROW_TEXT; // Change to down arrow
                IsClosed = false; // Mark as open
                Opened?.Invoke(); // Invoke the opened event
            }
        }
    }
}

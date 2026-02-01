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
using System;
using RPGCreator.UI;

namespace RPGCreator.Core.Types.Windows
{
    public class ConfirmDialog : Window
    {
        public event Action? Confirmed;
        public event Action? Cancelled;

        public bool AutoClose { get; set; } = true;

        protected StackPanel PanelContent;
        protected readonly Button ConfirmButton;
        protected Button CancelButton;

        public ConfirmDialog(
            string title = "Confirm",
            string message = "Are you sure?",
            string confirmButtonText = "Yes",
            string cancelButtonText = "No")
        {

            PanelContent = new StackPanel()
            {
                Margin = App.style.Margin
            };
            
            Title = title;
            Content = PanelContent;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var messageBlock = new TextBlock
            {
                Text = message
            };
            PanelContent?.Children.Add(messageBlock);
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            PanelContent?.Children.Add(buttonsPanel);
            ConfirmButton = new Button
            {
                Content = confirmButtonText,
                Margin = new Thickness(5)
            };
            buttonsPanel.Children.Add(ConfirmButton);
            ConfirmButton.Click += (s, e) => OnConfirm();
            CancelButton = new Button
            {
                Content = cancelButtonText,
                Margin = new Thickness(5),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red)
            };
            buttonsPanel.Children.Add(CancelButton);
            CancelButton.Click += (s, e) => OnCancel();

        }

        protected virtual void OnConfirm()
        {
            Confirmed?.Invoke();
            if (AutoClose)
            {
                Close();
            }
        }
        protected virtual void OnCancel()
        {
            Cancelled?.Invoke();
            if (AutoClose)
            {
                Close();
            }
        }
    }
}

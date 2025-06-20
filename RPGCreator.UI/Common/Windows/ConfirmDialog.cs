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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Common.Windows
{
    public class ConfirmDialog : Window
    {
        public event Action? Confirmed;
        public event Action? Cancelled;

        public bool AutoClose { get; set; } = true;

        public ConfirmDialog(
            string title = "Confirm",
            string message = "Are you sure?",
            string confirmButtonText = "Yes",
            string cancelButtonText = "No")
        {
            Title = title;
            Content = new StackPanel
            {
                Margin = App.style.Margin
            };
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var panel = Content as StackPanel;
            var messageBlock = new TextBlock
            {
                Text = message
            };
            panel?.Children.Add(messageBlock);
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel?.Children.Add(buttonsPanel);
            var yesButton = new Button
            {
                Content = confirmButtonText,
                Margin = new Thickness(5)
            };
            buttonsPanel.Children.Add(yesButton);
            yesButton.Click += (s, e) => OnConfirm();
            var noButton = new Button
            {
                Content = cancelButtonText,
                Margin = new Thickness(5),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red)
            };
            buttonsPanel.Children.Add(noButton);
            noButton.Click += (s, e) => OnCancel();

        }

        protected void OnConfirm()
        {
            Confirmed?.Invoke();
            if (AutoClose)
            {
                Close();
            }
        }
        protected void OnCancel()
        {
            Cancelled?.Invoke();
            if (AutoClose)
            {
                Close();
            }
        }
    }
}

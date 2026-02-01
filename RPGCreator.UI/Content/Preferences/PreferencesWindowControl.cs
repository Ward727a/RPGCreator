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
using System.Collections.Generic;
using System.Linq;

namespace RPGCreator.UI.Content.Preferences
{
    public class PreferencesWindowControl : UserControl
    {

        #region Components

        public Grid Body { get; private set; }
        public StackPanel MenuPanel { get; private set; }

        public UserControl SettingsPanel { get; private set; }

        #endregion

        // To Add a new settings panel, add it to the _SettingsPanels dictionary with a unique key.
        // If the key contains dots, it will be considered a sub-panel, but it need to be under the same parent entry.
        // For example, "General.Appearance" is a sub-panel of "General", so its just under the "General" entry in the dictionary.
        // If you had "General.Appearance.Color", it would be a sub-panel of "General.Appearance", and should be under the "General.Appearance" entry in the dictionary.
        private Dictionary<string, UserControl> _SettingsPanels = new()
        {
            ["General"] = new UserControl(), // Replace with actual settings panel
            ["General.Appearance"] = new UserControl(), // Replace with actual settings panel
            ["General.Language"] = new UserControl(), // Replace with actual settings panel
            ["General.Shortcuts"] = new UserControl(), // Replace with actual settings panel
            //["Graphics"] = new UserControl(), // Replace with actual settings panel
            //["Audio"] = new UserControl(), // Replace with actual settings panel
            ["Controls"] = new UserControl() // Replace with actual settings panel
        };
        // TODO: Voir pour continuer à bosser sur le système de préférences
        // Pourquoi pas voir pour passé sur chaque panel, et ajouter / uniformiser les styles ?
        // Voir pour aussi bosser sur le système de gestion des assets, pour l'instant c'est pas encore fait, mais foudrais voir pour le faire
        // pour gérer les assets de manière plus simple.
        public PreferencesWindowControl()
        {
            CreateComponents();
            LoadSettingsPanels();
            RegisterEvents();

            Content = Body;
        }

        protected void CreateComponents()
        {

            Body = new Grid
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,

                ColumnDefinitions = new ColumnDefinitions("Auto, *"),

            };

            MenuPanel = new StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Margin = new Avalonia.Thickness(10)
            };
            Body.Children.Add(MenuPanel);
            Grid.SetColumn(MenuPanel, 0);

            // Initialize components for the preferences window here
            // This could include settings panels, buttons, etc.
        }

        protected void LoadSettingsPanels()
        {
            foreach (var panel in _SettingsPanels)
            {
                // Count the number of dots in the panel name to determine the depth
                int depth = panel.Key.Count(c => c == '.');
                // Remove the dots to keep only the last part of the panel name
                string displayName = panel.Key.Split('.').Last();
                var button = new Button
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Content = displayName,
                    Margin = new Avalonia.Thickness(20*depth, 4, 0, 4)
                };
                button.Click += (sender, e) => ShowSettingsPanel(panel.Key);
                MenuPanel.Children.Add(button);
            }
        }

        protected void ShowSettingsPanel(string panelName)
        {
            if (_SettingsPanels.TryGetValue(panelName, out var panel))
            {
                Body.Children.Remove(SettingsPanel); // Remove the previous settings panel
                SettingsPanel = panel; // Set the new settings panel
                Body.Children.Add(SettingsPanel); // Add the new settings panel
                Grid.SetColumn(SettingsPanel, 1); // Set it to the second column
            }
            else
            {
                throw new ArgumentException($"Settings panel '{panelName}' does not exist.");
            }
        }

        protected void RegisterEvents()
        {
        }
    }
}

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
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Content.Preferences.Components.Projects;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Preferences
{
    public class PreferencesWindowControl : UserControl
    {

        #region Components

        private Grid Body { get; set; } = null!;
        private NavMenu MenuPanel { get; set; } = null!;
        private UserControl SettingsPanel { get; set; } = null!;

        #endregion
        
        // We really need to change that to a more dynamic system.
        
        // To Add a new settings panel, add it to the _SettingsPanels dictionary with a unique key.
        // If the key contains dots, it will be considered a sub-panel, but it need to be under the same parent entry.
        // For example, "General.Appearance" is a sub-panel of "General", so its just under the "General" entry in the dictionary.
        // If you had "General.Appearance.Color", it would be a sub-panel of "General.Appearance", and should be under the "General.Appearance" entry in the dictionary.
        private Dictionary<PipedPath, Control> _settingsPanels = new()
        {
            { "General".ToPipedPath(), new TextBlock()
                {
                    Text = "General settings"
                }
            },
            { "General".ToPipedPath().Extend("Appearance"), new TextBlock()
                {
                    Text = "Appearance settings - Coming soon!"
                }
            },
            { "General".ToPipedPath().Extend("Keybinds"), new TextBlock()
                {
                    Text = "Keybinds settings - Coming soon!"
                }
            },
            {
                "Modules".ToPipedPath(), new TextBlock()
                {
                    Text = "Modules settings - Coming soon!"
                }
            },
            { "Project".ToPipedPath(), new ProjectSettingsControl()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                }
            },
            { "Editor".ToPipedPath(), new TextBlock()
                {
                    Text = "Editor settings - Coming soon!"
                }
            }
        };
        
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
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            };

            MenuPanel = new NavMenu()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Avalonia.Thickness(10),
            };
            
            Body.Children.Add(MenuPanel);
            Grid.SetColumn(MenuPanel, 0);
            
            SettingsPanel = new UserControl
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(10)
            };
            Body.Children.Add(SettingsPanel);
            Grid.SetColumn(SettingsPanel, 1);

        }

        protected void LoadSettingsPanels()
        {
            MenuPanel.Items.Clear();

            var lastPath = PipedPath.Empty;
            NavMenuItem? lastButton = null;
            foreach (var settingsPanel in _settingsPanels.GetSortedByParentsAndBrother())
            {
                var path = settingsPanel.path;
                var control = settingsPanel.item;
                
                var button = new NavMenuItem()
                {
                    Command = new RelayCommand(() =>
                    {
                        SettingsPanel.Content = control;
                    }),
                    Header = path.Name,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                
                if(lastButton != null)
                {
                    if (path.IsChildOf(lastPath))
                    {
                        lastButton.Items.Add(button);
                    }
                    else
                    {
                        MenuPanel.Items.Add(button);
                        lastButton = button;
                        lastPath = path;
                    }
                }
                else
                {
                    MenuPanel.Items.Add(button);
                    lastButton = button;
                    lastPath = path;
                }
            }
        }

        protected void RegisterEvents()
        {
        }
    }
}

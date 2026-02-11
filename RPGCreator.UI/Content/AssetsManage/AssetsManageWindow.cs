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
using RPGCreator.UI.Content.AssetsManage.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor;
using RPGCreator.UI.Content.AssetsManage.Components.Skills;

namespace RPGCreator.UI.Content.AssetsManage
{
    public class AssetsManageWindow : Window
    {

        #region Components

        public Grid Body { get; private set; }

        public StackPanel MenuPanel { get; private set; }

        public UserControl AssetsPanel { get; private set; }

        #endregion

        public static AssetsManageWindow Instance { get; private set; }
        
        private Dictionary<string, Func<UserControl>> _AssetsMenuOptions = new(
            new Dictionary<string, Func<UserControl>>
            {
                ["Tilesets"] = () => new TilesetsManageControl(), // Tilesets / Auto-tiling system
                ["Auto-tiles"] = () => new AutoLayerEditorControl(), // Replace with actual assets panel
                ["---0"] = null, // Separator
                ["Characters"] = () => new CharactersManageControl(), // Replace with actual assets panel
                ["Enemies"] = () => new UserControl(), // Replace with actual assets panel
                // ["Stats"] = () => new StatsManageControl(), // Replace with the actual assets panel (This should be for creating / editing stats, like HP, MP, ATK, DEf, etc...)
                ["Items"] = () => new UserControl(), // Replace with actual assets panel (This items section should be for consumables, weapons, armor, etc...)
                ["Skills"] = () => new SkillsManageControl(), // Replace with actual assets panel
                ["Classes"] = () => new UserControl(), // Replace with actual assets panel
                ["Actors"] = () => new UserControl(), // Replace with actual assets panel
                ["Maps"] = () => new UserControl(), // Replace with actual assets panel
                ["Events"] = () => new UserControl(), // Replace with actual assets panel
                ["Quests"] = () => new UserControl(), // Replace with actual assets panel
                ["---4"] = null, // Separator
                ["Backgrounds"] = () => new UserControl(), // Replace with actual assets panel (This should be for backgrounds, like the title screen background, fighting background, map background etc...)
                ["---2"] = null, // Separator
                ["Sounds"] = () => new UserControl(), // Replace with actual assets panel
                ["Music"] = () => new UserControl(), // Replace with actual assets panel  
                ["---3"] = null, // Separator
                ["Plugins"] = () => new UserControl(), // Replace with actual assets panel

            }
        );

        public AssetsManageWindow()
        {
            Width = 1400;
            Height = 800;
            Title = "Assets Management";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CreateComponents();
            Content = Body;
            LoadAssetsMenuOptions();
            Instance = this;
            EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.AssetsManager, this);
        }
        

        protected void CreateComponents()
        {

            Body = new Grid
            {
                //Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            };

            MenuPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Margin = new Avalonia.Thickness(10),
                //Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LightGray),
            };
            Body.Children.Add(MenuPanel);
            
            var config = new AssetsManagerMenuContext.Config
            {
                RegisterAssetsMenuOption = RegisterAssetsMenuOption,
                RegisterAssetsMenuSeparator = RegisterAssetsMenuSeparator,
                ShowAssetsPanel = ShowAssetsPanel,
                OpenCustom = OpenCustom
            };
            
            EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.AssetsManagerMenu, MenuPanel, new AssetsManagerMenuContext(config));
        }

        protected void LoadAssetsMenuOptions()
        {
            foreach (var option in _AssetsMenuOptions)
            {
                if(option.Key.StartsWith("---"))
                {
                    MenuPanel.Children.Add(new Separator
                    {
                        Margin = new Avalonia.Thickness(5),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                    });
                    continue;
                }
                var button = new Button
                {
                    Content = option.Key,
                    Margin = new Avalonia.Thickness(5),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                };
                button.Click += (s, e) => ShowAssetsPanel(option.Key);
                MenuPanel.Children.Add(button);
            }
            
        }
        
        /// <summary>
        /// Method to register a new assets menu option.
        /// </summary>
        /// <param name="key"> A unique key for the menu option. </param>
        /// <param name="panelFactory"> A factory function that creates the UserControl panel for the menu option. </param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        [ExposeToPlugin("AssetsManager.Menu")]
        public void RegisterAssetsMenuOption(string key, Func<UserControl> panelFactory)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }

            if (panelFactory == null)
            {
                throw new ArgumentNullException(nameof(panelFactory), "Panel factory cannot be null.");
            }

            if (_AssetsMenuOptions.ContainsKey(key))
            {
                Logger.Warning("Assets menu option with key '{key}' is already registered. Overwriting.", key);
            }

            _AssetsMenuOptions[key] = panelFactory;
            Logger.Debug("Registered assets menu option: {key}", key);
        }

        /// <summary>
        /// Method to register a separator in the assets menu.
        /// </summary>
        [ExposeToPlugin("AssetsManager.Menu")]
        public void RegisterAssetsMenuSeparator()
        {
            var separatorKey = $"---{_AssetsMenuOptions.Count(kvp => kvp.Key.StartsWith("---"))}";
            _AssetsMenuOptions[separatorKey] = null;
            Logger.Debug("Registered assets menu separator: {key}", separatorKey);
        }
        
        [ExposeToPlugin("AssetsManager.Menu")]
        public void ShowAssetsPanel(string key)
        {
            if (_AssetsMenuOptions.TryGetValue(key, out var panel))
            {
                Logger.Debug("Showing assets panel: {key}", key);
                if (AssetsPanel != null)
                {
                    Body.Children.Remove(AssetsPanel);
                }
                AssetsPanel = panel();
                Body.Children.Add(AssetsPanel);
                Grid.SetColumn(AssetsPanel, 1);
            }
            else
            {
                Logger.Debug("Assets panel not found: {key}", key);
            }
        }

        [ExposeToPlugin("AssetsManager.Menu")]
        public void OpenCustom(UserControl control)
        {
            if(control == null)
            {
                throw new ArgumentNullException(nameof(control), "Control cannot be null");
            }

            if(AssetsPanel != null)
            {
                Body.Children.Remove(AssetsPanel);
            }
            AssetsPanel = control;
            Body.Children.Add(AssetsPanel);
            Grid.SetColumn(AssetsPanel, 1);
        }
    }
}

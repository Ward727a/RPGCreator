using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor;

public class CharacterEditorWindowControl : UserControl
{
    
    #region Constants
    
    private const int ColumnIndexLeftPanel = 0;
    private const int ColumnIndexMainContent = 0;
    
    #endregion
    
    #region Events
    
    #endregion
    
    #region Properties
    
    public CharacterData Data;
    
    #endregion
    
    #region Components
    
    public Grid Body { get; private set; }
    
        #region LeftPanel
        public StackPanel LeftPanel { get; private set; }
        
        public Image CharacterSprite { get; private set; }
        public PathPicker CharacterSpritePicker { get; private set; }
        
        #endregion
        
        #region MainContent
        
        public TabControl MainContent { get; private set; }
        
        #endregion
        
        private StackPanel BottomPanel { get; set; }
        private Button SaveButton { get; set; }
    
    #endregion
    
    #region Constructors
    
    public CharacterEditorWindowControl(CharacterData characterData)
    {
        Data = characterData;
        
        CreateComponents();
        RegisterEvents();
        
        Content = Body;
        
        ReloadContent();
    }
    
    #endregion
    
    #region Methods

    private void CreateComponents()
    {

        Body = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*"),
            RowDefinitions = new RowDefinitions("*, Auto"),
        };
        
        CreateLeftPanel();
        CreateMainContent();
    }

    private void CreateLeftPanel()
    {
        // LeftPanel = new StackPanel()
        // {
        //     Orientation = Avalonia.Layout.Orientation.Vertical,
        //     HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        //     VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        // };
        //
        // Body.Children.Add(LeftPanel);
        // Grid.SetColumn(LeftPanel, ColumnIndexLeftPanel);
        //
        // CharacterSprite = new Image()
        // {
        //     Width = 128,
        //     Height = 128,
        //     Margin = new Thickness(5)
        // };
        // LeftPanel.Children.Add(CharacterSprite);
        // CharacterSpritePicker = new PathPicker()
        // {
        //     Margin = new Thickness(5),
        //     Title = "Select Sprite...",
        //     Width = 200
        // };
        // LeftPanel.Children.Add(CharacterSpritePicker);
        
    }

    private void CreateMainContent()
    {
        MainContent = new TabControl()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        
        Body.Children.Add(MainContent);
        Grid.SetColumn(MainContent, ColumnIndexMainContent);
        
        BottomPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(5)
        };
        Body.Children.Add(BottomPanel);
        Grid.SetRow(BottomPanel, 1);
        Grid.SetColumnSpan(BottomPanel, 3);
        SaveButton = new Button()
        {
            Content = "Save",
            Width = 100,
            Margin = new Thickness(5)
        };
        SaveButton.Click += OnSaveButtonClick;
        BottomPanel.Children.Add(SaveButton);

        MainContent.Items.Add(new TabItem()
        {
            Header = "Properties",
            Content = new CharacterPropertiesTab(Data)
        });
        MainContent.Items.Add(new TabItem()
        {
            Header = "Skills",
            Content = new CharacterSkillsTab(Data)
        });
        MainContent.Items.Add(new TabItem()
        {
            Header = "Equipment",
            Content = new CharacterEquipmentTab(Data)
        });
        MainContent.Items.Add(new TabItem()
        {
            Header = "Display",
            Content = new CharacterDisplayTab(Data)
        });
        MainContent.Items.Add(new TabItem()
        {
            Header = "Stats",
            Content = new CharacterStatsTab(Data)
        });
        MainContent.Items.Add(new TabItem()
        {
            Header = "Features",
            Content = new CharacterFeaturesTab(Data)
        });
        MainContent.Items.Add(new TabItem()
        {
            Header = "RP Informations",
            Content = new CharacterRPInfoTab(Data)
        });
    }

    private void RegisterEvents()
    {
        RegisterLeftEvents();
    }
    
    private void RegisterLeftEvents()
    {
    }

    private void ReloadContent()
    {
        //
        // if (!string.IsNullOrEmpty(Data.SpritePath) && File.Exists(Data.SpritePath))
        // {
        //     CharacterSprite.Source = new Avalonia.Media.Imaging.Bitmap(Data.SpritePath);
        // }
        // else
        // {
        //     CharacterSprite.Source = null;
        // }
    }

    #endregion

    #region Events Handlers

    private void OnCharacterSpritePickerChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
        if (e.NewValue is not List<string> paths) return;
        if (paths.Count == 0) return;
        
        var newPath = paths[0];
        if (string.IsNullOrEmpty(newPath)) return;
        
        if(File.Exists(newPath))
        {
            CharacterSprite.Source = new Avalonia.Media.Imaging.Bitmap(newPath);
        }
        else
        {
            // Handle the case where the file does not exist
            CharacterSprite.Source = null;
        }
    }
    
    private void OnSaveButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Logger.Info("Character '{characterName}' saved.", Data.Name);
        
        Logger.Debug("Character Data: {@characterData}", Data);

        HashSet<Ulid> animationSpritesheet = new HashSet<Ulid>();
        
        foreach (var animationsMappingValue in Data.AnimationsMapping.Values)
        {
            foreach (var ulid in animationsMappingValue.Animations.Values)
            {
                if (EngineServices.AssetsManager.TryResolveAsset(ulid, out AnimationDef? animationDef))
                {
                    EngineServices.AssetsManager.RegisterAsset(animationDef);
                    if (EngineServices.AssetsManager.TryGetPack("assets_pack", out var animPack))
                    {
                        animPack.AddOrUpdateAsset(animationDef);
                        animationSpritesheet.Add(animationDef.SpritesheetId);
                    }
                }
            }
        }

        foreach (var ulid in animationSpritesheet)
        {
            if (EngineServices.AssetsManager.TryResolveAsset(ulid, out SpritesheetDef? spriteSheet))
            {
                EngineServices.AssetsManager.RegisterAsset(spriteSheet);
                if (EngineServices.AssetsManager.TryGetPack("assets_pack", out var spritePack))
                {
                    spritePack.AddOrUpdateAsset(spriteSheet);
                }
            }
        }
        
        EngineServices.AssetsManager.RegisterAsset(Data);
        if (EngineServices.AssetsManager.TryGetPack("assets_pack", out var pack))
        {
            pack.AddOrUpdateAsset(Data);
        }
    }
    #endregion
    
}
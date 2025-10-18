using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using RPGCreator.Core;
using RPGCreator.Core.Type.Assets.Characters;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;
using Serilog;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor;

public class CharacterEditorWindowControl : UserControl
{
    
    #region Constants
    
    private const int ColumnIndexLeftPanel = 0;
    private const int ColumnIndexMainContent = 2;
    
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
        
        public Image CharacterPortrait { get; private set; }
        public PathPicker CharacterPortraitPicker { get; private set; }
        
        public Image CharacterSprite { get; private set; }
        public PathPicker CharacterSpritePicker { get; private set; }
        
        public TextBox CharacterName { get; private set; }
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
            ColumnDefinitions = new ColumnDefinitions("Auto, 4, *"),
            RowDefinitions = new RowDefinitions("*, Auto"),
        };
        
        CreateLeftPanel();
        CreateMainContent();
    }

    private void CreateLeftPanel()
    {
        LeftPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        Body.Children.Add(LeftPanel);
        Grid.SetColumn(LeftPanel, ColumnIndexLeftPanel);
        
        CharacterPortrait = new Image()
        {
            Width = 128,
            Height = 128,
            Margin = new Thickness(5)
        };
        LeftPanel.Children.Add(CharacterPortrait);
        CharacterPortraitPicker = new PathPicker()
        {
            Margin = new Thickness(5),
            Title = "Select Portrait...",
            Width = 200
        };
        LeftPanel.Children.Add(CharacterPortraitPicker);

        CharacterSprite = new Image()
        {
            Width = 128,
            Height = 128,
            Margin = new Thickness(5)
        };
        LeftPanel.Children.Add(CharacterSprite);
        CharacterSpritePicker = new PathPicker()
        {
            Margin = new Thickness(5),
            Title = "Select Sprite...",
            Width = 200
        };
        LeftPanel.Children.Add(CharacterSpritePicker);
        
        CharacterName = new TextBox()
        {
            Margin = new Thickness(5),
            Width = 200,
            Watermark = "Character Name..."
        };
        LeftPanel.Children.Add(CharacterName);
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
            Header = "Animations",
            Content = new CharacterAnimationsTab(Data)
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
        CharacterPortraitPicker.PropertyChanged += OnCharacterPortraitPickerChanged;
        CharacterSpritePicker.PropertyChanged += OnCharacterSpritePickerChanged;
        CharacterName.TextChanged += OnCharacterNameChanged;
    }

    private void ReloadContent()
    {
        CharacterName.Text = Data.Name;
        
        if (!string.IsNullOrEmpty(Data.PortraitPath) && File.Exists(Data.PortraitPath))
        {
            CharacterPortrait.Source = new Avalonia.Media.Imaging.Bitmap(Data.PortraitPath);
        }
        else
        {
            CharacterPortrait.Source = null;
        }

        if (!string.IsNullOrEmpty(Data.SpritePath) && File.Exists(Data.SpritePath))
        {
            CharacterSprite.Source = new Avalonia.Media.Imaging.Bitmap(Data.SpritePath);
        }
        else
        {
            CharacterSprite.Source = null;
        }
    }

    #endregion

    #region Events Handlers
    private void OnCharacterNameChanged(object? sender, TextChangedEventArgs e)
    {
        
        Data.Name = CharacterName.Text ?? string.Empty;
        
    }

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
            Data.SpritePath = newPath;
        }
        else
        {
            // Handle the case where the file does not exist
            CharacterSprite.Source = null;
            Data.SpritePath = null;
        }
    }

    private void OnCharacterPortraitPickerChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
        if (e.NewValue is not List<string> paths) return;
        if (paths.Count == 0) return;
        
        var newPath = paths[0];
        if (string.IsNullOrEmpty(newPath)) return;
        
        if(File.Exists(newPath))
        {
            CharacterPortrait.Source = new Avalonia.Media.Imaging.Bitmap(newPath);
            Data.PortraitPath = newPath;
        }
        else
        {
            // Handle the case where the file does not exist
            CharacterPortrait.Source = null;
            Data.PortraitPath = null;
        }
    }
    
    private void OnSaveButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Log.Information("Character '{characterName}' saved.", Data.Name);
        
        Log.Debug("Character Data: {@characterData}", Data);
        EngineCore.Instance.Managers.Assets.CharacterRegistry.Register(Data);
    }
    #endregion
    
}
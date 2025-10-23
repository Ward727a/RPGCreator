using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Type.Assets.Characters;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterDisplayTab : UserControl
{
    
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
    #endregion
    
    #region Components
    
    private StackPanel Body { get; set; }
    
    private Grid PortraitPanel { get; set; }
    private Image PortraitImage { get; set; }
    private PathPicker PortraitPicker { get; set; }
    
    #endregion
    
    #region Constructors
    public CharacterDisplayTab(CharacterData data)
    {
        Data = data;
        Name = "Display";
        CreateComponents();
        RegisterEvents();
        Content = Body;
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };

        PortraitPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        Body.Children.Add(PortraitPanel);
        
        PortraitImage = new Image()
        {
            Width = 64,
            Height = 64,
            Source = !string.IsNullOrWhiteSpace(Data.PortraitPath) ? new Avalonia.Media.Imaging.Bitmap(Data.PortraitPath) : null,
            Margin = new Avalonia.Thickness(0, 0, 10, 0),
        };
        PortraitPanel.Children.Add(PortraitImage);
        Grid.SetColumn(PortraitImage, 0);
        
        PortraitPicker = new PathPicker()
        {
            Title = "Select Portrait", 
            FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
            SelectedPathsText = Data.PortraitPath,
            AllowMultiple = false,
            UsePickerType = UsePickerTypes.OpenFile,
            VerticalAlignment = VerticalAlignment.Center,
        };
        PortraitPanel.Children.Add(PortraitPicker);
        Grid.SetColumn(PortraitPicker, 1);

        var animPicker = new PathPicker()
        {
            Title = "Select Animation",
            FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
            SelectedPathsText = Data.SpritePath,
            AllowMultiple = false,
            UsePickerType = UsePickerTypes.OpenFile,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        };
        Body.Children.Add(animPicker);
        
        var animPlayer = new Common.AnimationPreviewer();
        animPlayer.Margin = new Avalonia.Thickness(0, 10, 0, 0);
        animPlayer.FrameSize = new Size(48, 64);
        
        Body.Children.Add(animPlayer);
        
        animPicker.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
            if (e.NewValue is not List<string> paths) return;
            if (paths.Count == 0) return;
        
            var newPath = paths[0];
            if (string.IsNullOrEmpty(newPath)) return;
        
            if(File.Exists(newPath))
            {
                animPlayer.AnimationPath = (newPath);
                Data.SpritePath = newPath;
            }
        };
    }
    
    private void RegisterEvents()
    {
        PortraitPicker.PropertyChanged += OnCharacterPortraitPickerChanged;
    }
    
    #endregion
    
    #region Events Handlers
    private void OnCharacterPortraitPickerChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
        if (e.NewValue is not List<string> paths) return;
        if (paths.Count == 0) return;
        
        var newPath = paths[0];
        if (string.IsNullOrEmpty(newPath)) return;
        
        if(File.Exists(newPath))
        {
            PortraitImage.Source = new Avalonia.Media.Imaging.Bitmap(newPath);
            Data.PortraitPath = newPath;
        }
        else
        {
            // Handle the case where the file does not exist
            PortraitImage.Source = null;
            Data.PortraitPath = null;
        }
    }
    #endregion
}
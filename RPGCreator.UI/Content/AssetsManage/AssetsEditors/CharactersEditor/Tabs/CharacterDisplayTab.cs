using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using RPGCreator.Core;
using RPGCreator.Core.Type;
using RPGCreator.Core.Type.Assets.Characters;
using RPGCreator.Core.Type.Windows;
using RPGCreator.UI.Common;
using Serilog;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;


public class AnimationData
{
    public string Name;
    public string Path;
    public int Fps;
    public int OrderIndex;
    public bool IsDefault = false;

    public AnimationData(int fps, string name, int orderIndex, string path)
    {
        Fps = fps;
        Name = name;
        OrderIndex = orderIndex;
        Path = path;
    }
}


public class CharacterDisplayTab : UserControl
{
    
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
    private AnimationData? SelectedAnimationData = null;
    
    private List<ListBoxItem> Grid4AnimationsExpanders = new List<ListBoxItem>();
    private List<ListBoxItem> Grid8AnimationsExpanders = new List<ListBoxItem>();
    
    private List<string> BasicAnimationsNames = new List<string>()
    {
        "idle_down",
        "idle_left",
        "idle_right",
        "idle_up",
        "death_down",
        "death_left",
        "death_right",
        "death_up",
        "walk_down",
        "walk_left",
        "walk_right",
        "walk_up"
    };
    private List<string> FreeMovementAnimationsNames = new List<string>()
    {
        "idle_down_left",
        "idle_up_left",
        "idle_up_right",
        "idle_down_right",
        "death_down_left",
        "death_up_left",
        "death_up_right",
        "death_down_right",
        "walk_down_left",
        "walk_up_left",
        "walk_Up_right",
        "walk_Down_right"
    };
    
    private List<string> AvailableAnimationsNames = new List<string>();
    
    #endregion
    
    #region Components
    
    private StackPanel Body { get; set; }
    
    private Grid PortraitPanel { get; set; }
    private Image PortraitImage { get; set; }
    private PathPicker PortraitPicker { get; set; }
    
    private Expander AnimationExpander { get; set; }
    private ScrollBox AnimationScrollBox { get; set; }
    private StackPanel AnimationStackPanel { get; set; }
    
    private Grid AnimationTopGrid { get; set; }
    
    private Button AnimationBulkImportButton { get; set; }
    private TextBox AddAnimationLabel { get; set; }
    private Button AddAnimationButton { get; set; }
    private ComboBox AnimationTypeComboBox { get; set; }
    
    private Grid AnimationGrid { get; set; }
    private ScrollBox AnimationListScrollBox { get; set; }
    private ListBox AnimationList { get; set; }
    private StackPanel AnimationDetailsPanel { get; set; }
    private Grid AnimationDetailsGrid { get; set; }
    
    private AnimationPreviewer AnimationPreviewer { get; set; }
    
    private Button RemoveAnimationButton { get; set; }
    private PathPicker ImportAnimationButton { get; set; }
    
    private CheckBox AutoPlayCheckBox { get; set; }
    
    #endregion
    
    #region Constructors
    public CharacterDisplayTab(CharacterData data)
    {
        Data = data;
        Name = "Display";
        CreateComponents();
        CreateAnimationExpanders();
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
        RenderOptions.SetBitmapInterpolationMode(PortraitImage, Avalonia.Media.Imaging.BitmapInterpolationMode.None);
        
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

        AnimationExpander = new Expander()
        {
            Header = "Animations",
            IsExpanded = true,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        Body.Children.Add(AnimationExpander);
        
        AnimationScrollBox = new ScrollBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        AnimationExpander.Content = AnimationScrollBox;
        
        AnimationStackPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        AnimationScrollBox.Content = AnimationStackPanel;

        AnimationTopGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto, Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        AnimationStackPanel.Children.Add(AnimationTopGrid);

        AnimationBulkImportButton = new Button()
        {
            Content = "Bulk Import",
            Margin = new Avalonia.Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        AnimationTopGrid.Children.Add(AnimationBulkImportButton);
        Grid.SetColumn(AnimationBulkImportButton, 0);

        AnimationBulkImportButton.Click += (sender, args) =>
        {
            var bulkImportDialog = new ConfirmDialog()
            {
                Title = "Bulk Import Animations",
                MinWidth = 850,
                MinHeight = 400,
                CanMinimize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            bulkImportDialog.Content = new BulkAnimationImportControl((importedAnimations) =>
            {
                int orderIndex = AnimationList.Items.Count;
                foreach (var anim in importedAnimations)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = anim.Value.Name;
                    item.Tag = anim;
                    AnimationList.Items.Add(item);
                    orderIndex++;
                }
            });
            
            
            bulkImportDialog.ShowDialog(AssetsManageWindow.Instance);
        };
        
        AddAnimationLabel = new TextBox()
        {
            Watermark = "Animation Name",
            Margin = new Avalonia.Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        AnimationTopGrid.Children.Add(AddAnimationLabel);
        Grid.SetColumn(AddAnimationLabel, 1);
        
        AddAnimationButton = new Button()
        {
            Content = "Add Animation",
            Margin = new Avalonia.Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        AnimationTopGrid.Children.Add(AddAnimationButton);
        Grid.SetColumn(AddAnimationButton, 2);
        
        AnimationTypeComboBox = new ComboBox()
        {
            SelectedIndex = 0,
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        AnimationTypeComboBox.Items.Add(new ComboBoxItem()
        {
            Content = "4 Directions"
        });
        AnimationTypeComboBox.Items.Add(new ComboBoxItem()
        {
            Content = "8 Directions / Free movement"
        });
        AnimationTypeComboBox.SelectedIndex = 0;
        AnimationTopGrid.Children.Add(AnimationTypeComboBox);
        Grid.SetColumn(AnimationTypeComboBox, 3);
        
        var animSeparator = new Separator()
        {
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        AnimationStackPanel.Children.Add(animSeparator);

        AnimationGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AnimationStackPanel.Children.Add(AnimationGrid);
        
        AnimationListScrollBox = new ScrollBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Height = 400,
        };
        AnimationGrid.Children.Add(AnimationListScrollBox);
        Grid.SetColumn(AnimationListScrollBox, 0);
        
        AnimationList = new ListBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AnimationListScrollBox.Content = AnimationList;

        var separator = new Divider()
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = 200,
        };
        AnimationGrid.Children.Add(separator);
        Grid.SetColumn(separator, 1);
        
        AnimationDetailsPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(10, 0, 0, 0)
        };
        AnimationGrid.Children.Add(AnimationDetailsPanel);
        Grid.SetColumn(AnimationDetailsPanel, 2);

        AnimationDetailsGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, 5, *, 5, Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        AnimationDetailsPanel.Children.Add(AnimationDetailsGrid);   
        
        RemoveAnimationButton = new Button()
        {
            Content = "Remove Animation",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        AnimationDetailsGrid.Children.Add(RemoveAnimationButton);
        Grid.SetColumn(RemoveAnimationButton, 0);
        ImportAnimationButton = new PathPicker()
        {
            Title = "Select Animation",
            FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
            AllowMultiple = false,
            UsePickerType = UsePickerTypes.OpenFile,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            
        };
        AnimationDetailsGrid.Children.Add(ImportAnimationButton);
        Grid.SetColumn(ImportAnimationButton, 2);
        
        if (Application.Current.TryFindResource("ButtonPathPicker", out var themeObj)
              && themeObj is ControlTheme theme)
        {
            ImportAnimationButton.Theme = theme;
        }
        
        AutoPlayCheckBox = new CheckBox()
        {
            Content = "Auto Play",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        AnimationDetailsGrid.Children.Add(AutoPlayCheckBox);
        Grid.SetColumn(AutoPlayCheckBox, 4);

        var animationDetailSeparator = new Divider()
        {
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        AnimationDetailsPanel.Children.Add(animationDetailSeparator);
        
        AnimationPreviewer = new AnimationPreviewer()
        {
            FrameSize = new Size(48, 64),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        AnimationDetailsPanel.Children.Add(AnimationPreviewer);
        //
        // var animPicker = new PathPicker()
        // {
        //     Title = "Select Animation",
        //     FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
        //     SelectedPathsText = Data.SpritePath,
        //     AllowMultiple = false,
        //     UsePickerType = UsePickerTypes.OpenFile,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Avalonia.Thickness(0, 10, 0, 0)
        // };
        // Body.Children.Add(animPicker);
        //
        // var animPlayer = new Common.AnimationPreviewer();
        // animPlayer.Margin = new Avalonia.Thickness(0, 10, 0, 0);
        // animPlayer.FrameSize = new Size(48, 64);
        //
        // Body.Children.Add(animPlayer);
        //
        // animPicker.PropertyChanged += (s, e) =>
        // {
        //     if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
        //     if (e.NewValue is not List<string> paths) return;
        //     if (paths.Count == 0) return;
        //
        //     var newPath = paths[0];
        //     if (string.IsNullOrEmpty(newPath)) return;
        //
        //     if(File.Exists(newPath))
        //     {
        //         animPlayer.AnimationPath = (newPath);
        //         Data.SpritePath = newPath;
        //     }
        // };
        //
        // var idleAnimPicker = new PathPicker()
        // {
        //     Title = "Select Idle Animation",
        //     FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
        //     SelectedPathsText = "",
        //     AllowMultiple = false,
        //     UsePickerType = UsePickerTypes.OpenFile,
        //     VerticalAlignment = VerticalAlignment.Center,
        //     Margin = new Avalonia.Thickness(0, 10, 0, 0)
        // };
        // Body.Children.Add(idleAnimPicker);
        //
        // var idleAnimPlayer = new Common.AnimationPreviewer();
        // idleAnimPlayer.Margin = new Avalonia.Thickness(0, 10, 0, 0);
        // idleAnimPlayer.FrameSize = new Size(48, 64);
        //
        // Body.Children.Add(idleAnimPlayer);
        //
        // idleAnimPicker.PropertyChanged += (s, e) =>
        // {
        //     if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
        //     if (e.NewValue is not List<string> paths) return;
        //     if (paths.Count == 0) return;
        //
        //     var newPath = paths[0];
        //     if (string.IsNullOrEmpty(newPath)) return;
        //
        //     if (File.Exists(newPath))
        //     {
        //         // Handle idle animation path change
        //         idleAnimPlayer.AnimationPath = (newPath);
        //     }
        // };
        //
        // var saveAnimationButton = new Button()
        // {
        //     Content = "Save Animation",
        //     Width = 120,
        //     Height = 30,
        //     Margin = new Avalonia.Thickness(0, 10, 0, 0),
        //     HorizontalAlignment = HorizontalAlignment.Left
        // };
        // Body.Children.Add(saveAnimationButton);
        // saveAnimationButton.Click += (s, e) =>
        // {
        //     EngineCore.Instance.Events.OnDEBUG_RTPAnimationAtlasGenerated(animPlayer._animationInstance, idleAnimPlayer._animationInstance);
        // };
    }
    
    private void CreateAnimationExpanders()
    {

        int orderIndex = 0;
        foreach (var basicAnimationsName in BasicAnimationsNames)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = basicAnimationsName;
            item.Tag = new AnimationData(10, basicAnimationsName, orderIndex, ""){IsDefault = true};
            Grid4AnimationsExpanders.Add(item);
            AnimationList.Items.Add(item);
            orderIndex++;
        }

        foreach (var freeMovementAnimationsName in FreeMovementAnimationsNames)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = freeMovementAnimationsName;
            item.Tag = new AnimationData(10, freeMovementAnimationsName, orderIndex, ""){IsDefault = true};
            Grid8AnimationsExpanders.Add(item);
            orderIndex++;
        }
        
        
    }
    
    private void RegisterEvents()
    {
        PortraitPicker.PropertyChanged += OnCharacterPortraitPickerChanged;
        
        AnimationTypeComboBox.SelectionChanged += OnAnimationTypeChanged;
        AnimationList.SelectionChanged += OnAnimationSelectedChanged;
        
        ImportAnimationButton.PropertyChanged += OnImportAnimationButtonChanged;
        AnimationPreviewer.FpsChanged += OnFPSChanged;
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

    private void OnImportAnimationButtonChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
        if (e.NewValue is not List<string> paths) return;
        if (paths.Count == 0) return;
        if (SelectedAnimationData == null) return;
        
        var newPath = paths[0];
        if (string.IsNullOrEmpty(newPath)) return;
        
        if(File.Exists(newPath))
        {
            SelectedAnimationData.Path = newPath;
            AnimationPreviewer.AnimationPath = newPath;
        }
    }

    private void OnAnimationSelectedChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AnimationList.SelectedItem is not ListBoxItem selectedItem) return;
        if (selectedItem.Tag is not AnimationData animationData) return;

        SelectedAnimationData = animationData;
        AnimationPreviewer.Stop(false);
        AnimationPreviewer.ClearImage();
        AnimationPreviewer.UpdateFPS(animationData.Fps);

        if (animationData.IsDefault)
        {
            RemoveAnimationButton.IsEnabled = false;
        }
        else
        {
            RemoveAnimationButton.IsEnabled = true;
        }

        if (string.IsNullOrEmpty(animationData.Path)) return;
        if(!File.Exists(animationData.Path)) return;
        AnimationPreviewer.AnimationPath = animationData.Path;
        AnimationPreviewer.UpdateFrame(0);
        if(AutoPlayCheckBox.IsChecked.HasValue && AutoPlayCheckBox.IsChecked.Value)
            AnimationPreviewer.Play();
    }

    private void OnAnimationTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        if (AnimationTypeComboBox.SelectedIndex == 0)
        {
            // 4 Directions
            foreach (var item in Grid8AnimationsExpanders)
            {
                if (AnimationList.Items.Contains(item))
                {
                    AnimationList.Items.Remove(item);
                }
            }
        }
        else
        {
            // 8 Directions / Free movement
            foreach (var item in Grid8AnimationsExpanders)
            {
                if (!AnimationList.Items.Contains(item))
                {
                    AnimationList.Items.Add(item);
                }
            }
        }
        
    }

    private void OnFPSChanged(int newFps)
    {
        if (SelectedAnimationData == null) return;
        SelectedAnimationData.Fps = newFps;
    }
    #endregion
}
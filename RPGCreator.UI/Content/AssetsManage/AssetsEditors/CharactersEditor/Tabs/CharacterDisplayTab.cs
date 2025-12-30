using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using RPGCreator.Core.Types;
using RPGCreator.Core.Types.Windows;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.UI.Common;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;



public class CharacterDisplayTab : UserControl
{
    
    #region Events
    #endregion

    #region Properties

    private EDirection CurrentAnimationDirection = EDirection.None;
    private string CurrentAnimationName = string.Empty;
    
    private Button? CurrentDirectionButton = null;
    
    private IAssetScope AssetScope;
    
    public CharacterData Data;
    
    private Dictionary<string, DirectionalAnimationSet> DirectionalAnimations = new Dictionary<string, DirectionalAnimationSet>();
    
    private AnimationDef? SelectedAnimationData = null;
    
    private List<ListBoxItem> Grid4AnimationsExpanders = new List<ListBoxItem>();
    
    private List<string> BasicAnimationsNames = new List<string>()
    {
        "idle",
        "death",
        "walk"
    };
    
    private List<string> AvailableAnimationsNames = new List<string>();
    private Button _downLeftButton;
    private Button _downButton;
    private Button _downRightButton;
    private Button _leftButton;
    private Button _upButton;
    private Button _upRightButton;
    private Button _rightButton;
    private Button _upLeftButton;
    private Button _centerButton;
    private Grid _animDirBox;
    private Divider _animationDetailSeparator;

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
        AssetScope = EngineServices.AssetsManager.CreateAssetScope("CharacterDisplayTabScope");

        foreach (var animName in BasicAnimationsNames)
        {
            DirectionalAnimations.Add(animName, new DirectionalAnimationSet());
        }
        
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
                MinWidth = 1050,
                MinHeight = 400,
                Width = 1050,
                Height = 400,
                CanMinimize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual
            };
            
            List<string> existingAnimationNames = new List<string>();
            
            existingAnimationNames.AddRange(BasicAnimationsNames);
            existingAnimationNames.AddRange(AvailableAnimationsNames);
            
            bulkImportDialog.Content = new BulkAnimationImportControl((importedAnimations, bulkAssetScope) =>
            {
                int orderIndex = AnimationList.Items.Count;
                foreach (var anim in importedAnimations)
                {
                    var animName = anim.Key;
                    var animDef = anim.Value;
                    EngineServices.AssetsManager.TryResolveAsset(animDef.SpriteSheetId, out SpritesheetDef? spritesheetDef);
                    if (spritesheetDef == null)
                    {
                        Logger.Error("Failed to resolve spritesheet with ID {SpriteSheetId} for animation {AnimName}", animDef.SpriteSheetId, animName);
                        continue;
                    }
                    
                    bulkAssetScope.TransferTo(AssetScope, animDef);
                    bulkAssetScope.TransferTo(AssetScope, spritesheetDef);
                    
                    if(existingAnimationNames.Contains(animName))
                    {
                        // If it's already existing we need to get the ListBoxItem and update it
                        foreach (ListBoxItem existingItem in AnimationList.Items.ToList())
                        {
                            if (existingItem.Content.ToString() == animName)
                            {
                                existingItem.Tag = animDef;
                                break;
                            }
                        }
                        continue;
                    }
                    
                    ListBoxItem item = new ListBoxItem();
                    item.Content = anim.Value.Name;
                    item.Tag = anim;
                    AnimationList.Items.Add(item);
                    orderIndex++;
                }
                
                // Close the dialog
                bulkImportDialog.Close();
            }, existingAnimationNames);
            
            
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

        _animationDetailSeparator = new Divider()
        {
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        AnimationDetailsPanel.Children.Add(_animationDetailSeparator);

        _animDirBox = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, 2, Auto, 2, Auto"),
            ColumnDefinitions = new ColumnDefinitions("Auto, 2, Auto, 2, Auto"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AnimationDetailsPanel.Children.Add(_animDirBox);

        Button createDirButton(string buttonDir)
        {
            var button = new Button()
            {
                Content = buttonDir,
                Width = 40,
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            return button;
        }
        
        _downLeftButton = createDirButton("↙");
        _downLeftButton.IsEnabled = false;
        _downLeftButton.Tag = EDirection.DownLeft;
        _animDirBox.Children.Add(_downLeftButton);
        Grid.SetRow(_downLeftButton, 4);
        Grid.SetColumn(_downLeftButton, 0);
        
        _downButton = createDirButton("↓");
        _downButton.Tag = EDirection.Down;
        _animDirBox.Children.Add(_downButton);
        Grid.SetRow(_downButton, 4);
        Grid.SetColumn(_downButton, 2);
        
        _downRightButton = createDirButton("↘");
        _downRightButton.IsEnabled = false;
        _downRightButton.Tag = EDirection.DownRight;
        _animDirBox.Children.Add(_downRightButton);
        Grid.SetRow(_downRightButton, 4);
        Grid.SetColumn(_downRightButton, 4); 
        
        _leftButton = createDirButton("←");
        _leftButton.Tag = EDirection.Left;
        _animDirBox.Children.Add(_leftButton);
        Grid.SetRow(_leftButton, 2);
        Grid.SetColumn(_leftButton, 0);
        
        _upButton = createDirButton("↑");
        _upButton.Tag = EDirection.Up;
        _animDirBox.Children.Add(_upButton);
        Grid.SetRow(_upButton, 0);
        Grid.SetColumn(_upButton, 2);
        
        _upRightButton = createDirButton("↗");
        _upRightButton.IsEnabled = false;
        _upRightButton.Tag = EDirection.UpRight;
        _animDirBox.Children.Add(_upRightButton);
        Grid.SetRow(_upRightButton, 0);
        Grid.SetColumn(_upRightButton, 4);
        
        _rightButton = createDirButton("→");
        _rightButton.Tag = EDirection.Right;
        _animDirBox.Children.Add(_rightButton);
        Grid.SetRow(_rightButton, 2);
        Grid.SetColumn(_rightButton, 4);
        
        _upLeftButton = createDirButton("↖");
        _upLeftButton.IsEnabled = false;
        _upLeftButton.Tag = EDirection.UpLeft;
        _animDirBox.Children.Add(_upLeftButton);
        Grid.SetRow(_upLeftButton, 0);
        Grid.SetColumn(_upLeftButton, 0);
        
        _centerButton = createDirButton("●");
        _centerButton.Tag = EDirection.None;
        _animDirBox.Children.Add(_centerButton);
        Grid.SetRow(_centerButton, 2);
        Grid.SetColumn(_centerButton, 2);
        CurrentDirectionButton = _centerButton;
        _centerButton.Foreground = Brushes.Red;
        CurrentAnimationDirection = EDirection.None;
        
        AnimationPreviewer = new AnimationPreviewer()
        {
            FrameSize = new Size(48, 64),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        AnimationDetailsPanel.Children.Add(AnimationPreviewer);
    }
    
    private void CreateAnimationExpanders()
    {
        int orderIndex = 0;
        foreach (var basicAnimationsName in BasicAnimationsNames)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = basicAnimationsName;
            item.Tag = basicAnimationsName;
            Grid4AnimationsExpanders.Add(item);
            AnimationList.Items.Add(item);
            orderIndex++;
        }
    }
    
    private void RegisterEvents()
    {
        
        AddAnimationButton.Click += AddAnimationButtonOnClick;
        RemoveAnimationButton.Click += RemoveAnimationButtonOnClick;
        
        PortraitPicker.PropertyChanged += OnCharacterPortraitPickerChanged;
        
        AnimationTypeComboBox.SelectionChanged += OnAnimationTypeChanged;
        AnimationList.SelectionChanged += OnAnimationSelectedChanged;
        
        ImportAnimationButton.PropertyChanged += OnImportAnimationButtonChanged;
        AnimationPreviewer.FpsChanged += OnFPSChanged;
        
        _downLeftButton.Click += OnAnimationDirectionButtonClicked;
        _downButton.Click += OnAnimationDirectionButtonClicked;
        _downRightButton.Click += OnAnimationDirectionButtonClicked;
        _leftButton.Click += OnAnimationDirectionButtonClicked;
        _upButton.Click += OnAnimationDirectionButtonClicked;
        _upRightButton.Click += OnAnimationDirectionButtonClicked;
        _rightButton.Click += OnAnimationDirectionButtonClicked;
        _upLeftButton.Click += OnAnimationDirectionButtonClicked;
        _centerButton.Click += OnAnimationDirectionButtonClicked;
    }

    private void RemoveAnimationButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (AnimationList.SelectedItem is not ListBoxItem selectedItem) return;
        if (selectedItem.Tag is not string animationName) return;

        if (BasicAnimationsNames.Contains(animationName))
        {
            // Cannot remove basic animations
            return;
        }

        DirectionalAnimations.Remove(animationName);
        AvailableAnimationsNames.Remove(animationName);
        AnimationList.Items.Remove(selectedItem);
        
        if(CurrentAnimationName != animationName) return;
        
        SelectedAnimationData = null;
        AnimationPreviewer.Stop(false);
        AnimationPreviewer.ClearImage();
    }

    private void AddAnimationButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var newAnimationName = AddAnimationLabel.Text?.Trim();
        if (string.IsNullOrEmpty(newAnimationName)) return;
        if (DirectionalAnimations.ContainsKey(newAnimationName))
        {
            // Animation already exists
            return;
        }

        DirectionalAnimations.Add(newAnimationName, new DirectionalAnimationSet());
        
        ListBoxItem item = new ListBoxItem();
        item.Content = newAnimationName;
        item.Tag = newAnimationName;
        AnimationList.Items.Add(item);
        
        AvailableAnimationsNames.Add(newAnimationName);
        
        AddAnimationLabel.Text = string.Empty;
    }

    private void OnAnimationDirectionButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not EDirection direction) return;
        if(CurrentAnimationDirection == direction) return;

        if (CurrentDirectionButton != null)
        {
            CurrentDirectionButton.ClearValue(Button.ForegroundProperty);
        }
        button.Foreground = Brushes.Red;
        CurrentDirectionButton = button;
        
        CurrentAnimationDirection = direction;
        
        var directionalAnimationsSet = DirectionalAnimations[CurrentAnimationName];
        Ulid animationId = directionalAnimationsSet.GetAnimation(CurrentAnimationDirection);
        if (animationId == Ulid.Empty)
        {
            SelectedAnimationData = EngineServices.AssetsManager.CreateTransientAsset<AnimationDef>();
            SelectedAnimationData.Name = CurrentAnimationName;
            directionalAnimationsSet.SetAnimation(CurrentAnimationDirection, SelectedAnimationData.Unique);
        }
        else
        {
            if (!EngineServices.AssetsManager.TryResolveAsset<AnimationDef>(animationId, out var animationData))
            {
                Logger.Error("Failed to resolve animation with ID {AnimationId}", animationId);
                return;
            }
            SelectedAnimationData = animationData;
        }
        AnimationPreviewer.Stop(false);
        AnimationPreviewer.ClearImage();
        AnimationPreviewer.UpdateFPS(SelectedAnimationData.Fps);
        AnimationPreviewer.AnimationDefinition = SelectedAnimationData;
        AnimationPreviewer.UpdateFrame(0);
        if(AutoPlayCheckBox.IsChecked.HasValue && AutoPlayCheckBox.IsChecked.Value)
            AnimationPreviewer.Play();
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
        
        ImportAnimationButton.SuggestedStartPath = Path.GetDirectoryName(newPath) ?? string.Empty;
        
        if(File.Exists(newPath))
        {
            var tempSpriteSheetDef = EngineServices.AssetsManager.CreateTransientAsset<SpritesheetDef>();
            tempSpriteSheetDef.ImagePath = (newPath);
            tempSpriteSheetDef.FrameWidth = 48;
            tempSpriteSheetDef.FrameHeight = 64;

            SelectedAnimationData.FrameIndexes = tempSpriteSheetDef.GetAllRowIndexes(0);
            SelectedAnimationData.SpriteSheetId = tempSpriteSheetDef.Unique;
        
            AnimationPreviewer.AnimationDefinition = SelectedAnimationData;
        }
    }

    private void OnAnimationSelectedChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AnimationList.SelectedItem is not ListBoxItem selectedItem) return;
        if (selectedItem.Tag is not string animationName) return;

        CurrentAnimationName = animationName;
        
        if (!DirectionalAnimations.TryGetValue(animationName, out var directionalAnimationSet))
        {
            directionalAnimationSet = new DirectionalAnimationSet();
            DirectionalAnimations.Add(animationName, directionalAnimationSet);
        }
        
        Ulid animationId = directionalAnimationSet.GetAnimation(CurrentAnimationDirection);
        if (animationId == Ulid.Empty)
        {
            SelectedAnimationData = EngineServices.AssetsManager.CreateTransientAsset<AnimationDef>();
            SelectedAnimationData.Name = animationName;
            directionalAnimationSet.SetAnimation(CurrentAnimationDirection, SelectedAnimationData.Unique);
        }
        else
        {
            if (!EngineServices.AssetsManager.TryResolveAsset<AnimationDef>(animationId, out var animationData))
            {
                Logger.Error("Failed to resolve animation with ID {AnimationId}", animationId);
                return;
            }
            SelectedAnimationData = animationData;
        }
        
        
        AnimationPreviewer.Stop(false);
        AnimationPreviewer.ClearImage();
        AnimationPreviewer.UpdateFPS(SelectedAnimationData.Fps);

        if (BasicAnimationsNames.Contains(SelectedAnimationData.Name))
        {
            RemoveAnimationButton.IsEnabled = false;
        }
        else
        {
            RemoveAnimationButton.IsEnabled = true;
        }

        AnimationPreviewer.AnimationDefinition = SelectedAnimationData;
        AnimationPreviewer.UpdateFrame(0);
        if(AutoPlayCheckBox.IsChecked.HasValue && AutoPlayCheckBox.IsChecked.Value)
            AnimationPreviewer.Play();
    }

    private void OnAnimationTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        if (AnimationTypeComboBox.SelectedIndex == 0)
        {
            _upLeftButton.IsEnabled = false;
            _upRightButton.IsEnabled = false;
            _downLeftButton.IsEnabled = false;
            _downRightButton.IsEnabled = false; 
        }
        else
        {
            _upLeftButton.IsEnabled = true;
            _upRightButton.IsEnabled = true;
            _downLeftButton.IsEnabled = true;
            _downRightButton.IsEnabled = true; 
        }
        
    }

    private void OnFPSChanged(int newFps)
    {
        if (SelectedAnimationData == null) return;
        SelectedAnimationData.Fps = newFps;
    }
    #endregion
}
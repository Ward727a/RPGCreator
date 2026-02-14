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
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.UI.Common;
using Ursa.Controls;
using Size = Avalonia.Size;
using Thickness = Avalonia.Thickness;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;



public class CharacterDisplayTab : UserControl
{
    #region Properties

    private EntityDirection _currentAnimationDirection = EntityDirection.Center;
    private int _currentAnimationIdx = 0;
    
    private Button? _currentDirectionButton;
    
    private readonly IAssetScope _assetScope;

    private readonly CharacterData _data;
    
    private AnimationDef? _selectedAnimationData;

    private readonly List<string> _basicAnimationsNames =
    [
    ];
    
    private readonly List<string> _availableAnimationsNames = [];
    private Button _downLeftButton = null!;
    private Button _downButton = null!;
    private Button _downRightButton = null!;
    private Button _leftButton = null!;
    private Button _upButton = null!;
    private Button _upRightButton = null!;
    private Button _rightButton = null!;
    private Button _upLeftButton = null!;
    private Button _centerButton = null!;
    private Grid _animDirBox = null!;
    private Divider _animationDetailSeparator = null!;
    private string _currentAnimationName;

    #endregion
    
    #region Components
    
    private StackPanel Body { get; set; } = null!;

    private Grid PortraitPanel { get; set; } = null!;
    private Image PortraitImage { get; set; } = null!;
    private PathPicker PortraitPicker { get; set; } = null!;

    private Expander AnimationExpander { get; set; } = null!;
    private ScrollBox AnimationScrollBox { get; set; } = null!;
    private StackPanel AnimationStackPanel { get; set; } = null!;

    private Grid AnimationTopGrid { get; set; } = null!;

    private Button AnimationBulkImportButton { get; set; } = null!;
    private TextBox AddAnimationLabel { get; set; } = null!;
    private Button AddAnimationButton { get; set; } = null!;
    private ComboBox AnimationTypeComboBox { get; set; } = null!;

    private Grid AnimationGrid { get; set; } = null!;
    private ScrollBox AnimationListScrollBox { get; set; } = null!;
    private ListBox AnimationList { get; set; } = null!;
    private StackPanel AnimationDetailsPanel { get; set; } = null!;
    private Grid AnimationDetailsGrid { get; set; } = null!;

    private AnimationPreviewer AnimationPreviewer { get; set; } = null!;

    private Button RemoveAnimationButton { get; set; } = null!;
    private PathPicker ImportAnimationButton { get; set; } = null!;

    private CheckBox AutoPlayCheckBox { get; set; } = null!;

    #endregion
    
    #region Constructors
    public CharacterDisplayTab(CharacterData data)
    {
        _assetScope = EngineServices.AssetsManager.CreateAssetScope("CharacterDisplayTabScope");
        
        _data = data;
        Name = "Display";
        CreateComponents();
        _data.Features.CollectionChanged += (_, _) =>
        {
            // Clear existing animations
            AnimationList.Items.Clear();
            
            CreateAnimationExpanders();
        };
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
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10)
        };

        PortraitPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10)
        };
        Body.Children.Add(PortraitPanel);
        
        PortraitImage = new Image()
        {
            Width = 64,
            Height = 64,
            Source = !string.IsNullOrWhiteSpace(_data.PortraitPath) ? new Avalonia.Media.Imaging.Bitmap(_data.PortraitPath) : null,
            Margin = new Thickness(0, 0, 10, 0),
        };
        PortraitPanel.Children.Add(PortraitImage);
        Grid.SetColumn(PortraitImage, 0);
        RenderOptions.SetBitmapInterpolationMode(PortraitImage, Avalonia.Media.Imaging.BitmapInterpolationMode.None);
        
        PortraitPicker = new PathPicker()
        {
            Title = "Select Portrait", 
            FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
            SelectedPathsText = _data.PortraitPath,
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
            Margin = new Thickness(0, 20, 0, 0)
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
            Margin = new Thickness(10)
        };
        AnimationScrollBox.Content = AnimationStackPanel;

        AnimationTopGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto, Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        AnimationStackPanel.Children.Add(AnimationTopGrid);

        AnimationBulkImportButton = new Button()
        {
            Content = "Bulk Import",
            Margin = new Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        AnimationTopGrid.Children.Add(AnimationBulkImportButton);
        Grid.SetColumn(AnimationBulkImportButton, 0);

        AnimationBulkImportButton.Click += (_, _) =>
        {
            
            EditorUiServices.NotificationService.Error("WIP Feature", "This feature is currently unavailable as it is still in development. We apologize for the inconvenience.", new NotificationOptions(5000));
            return;
            
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
            
            existingAnimationNames.AddRange(_basicAnimationsNames);
            existingAnimationNames.AddRange(_availableAnimationsNames);
            
            bulkImportDialog.Content = new BulkAnimationImportControl((importedAnimations, bulkAssetScope) =>
            {
                foreach (var anim in importedAnimations)
                {
                    var animName = anim.Key;
                    var animDef = anim.Value;
                    EngineServices.AssetsManager.TryResolveAsset(animDef.SpritesheetId, out SpritesheetDef? spritesheetDef);
                    if (spritesheetDef == null)
                    {
                        Logger.Error("Failed to resolve spritesheet with ID {SpriteSheetId} for animation {AnimName}", animDef.SpritesheetId, animName);
                        continue;
                    }
                    
                    bulkAssetScope.TransferTo(_assetScope, animDef);
                    bulkAssetScope.TransferTo(_assetScope, spritesheetDef);
                    
                    if(existingAnimationNames.Contains(animName))
                    {
                        // If it already exists we need to get the ListBoxItem and update it
                        foreach (ListBoxItem? existingItem in AnimationList.Items.ToList())
                        {
                            if(existingItem?.Content == null)
                                continue;

                            if (existingItem.Content.ToString() != animName) continue;
                            
                            existingItem.Tag = animDef;
                            break;
                        }
                        continue;
                    }
                    
                    var item = new ListBoxItem
                    {
                        Content = anim.Value.Name,
                        Tag = anim
                    };
                    AnimationList.Items.Add(item);
                }
                
                // Close the dialog
                bulkImportDialog.Close();
            }, existingAnimationNames);
            
            
            bulkImportDialog.ShowDialog(AssetsManageWindow.Instance);
        };
        
        AddAnimationLabel = new TextBox()
        {
            Watermark = "Animation Name",
            Margin = new Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        AnimationTopGrid.Children.Add(AddAnimationLabel);
        Grid.SetColumn(AddAnimationLabel, 1);
        
        AddAnimationButton = new Button()
        {
            Content = "Add Animation",
            Margin = new Thickness(0, 0, 10, 0),
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
            Margin = new Thickness(0, 0, 0, 10),
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
            Margin = new Thickness(10, 0, 0, 0)
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
        
        if (Application.Current!.TryFindResource("ButtonPathPicker", out var themeObj)
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
            Margin = new Thickness(0, 5, 0, 5),
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

        Button CreateDirButton(string buttonDir)
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
        
        _downLeftButton = CreateDirButton("↙");
        _downLeftButton.IsEnabled = false;
        _downLeftButton.Tag = EntityDirection.DownLeft;
        _animDirBox.Children.Add(_downLeftButton);
        Grid.SetRow(_downLeftButton, 4);
        Grid.SetColumn(_downLeftButton, 0);
        
        _downButton = CreateDirButton("↓");
        _downButton.Tag = EntityDirection.Down;
        _animDirBox.Children.Add(_downButton);
        Grid.SetRow(_downButton, 4);
        Grid.SetColumn(_downButton, 2);
        
        _downRightButton = CreateDirButton("↘");
        _downRightButton.IsEnabled = false;
        _downRightButton.Tag = EntityDirection.DownRight;
        _animDirBox.Children.Add(_downRightButton);
        Grid.SetRow(_downRightButton, 4);
        Grid.SetColumn(_downRightButton, 4); 
        
        _leftButton = CreateDirButton("←");
        _leftButton.Tag = EntityDirection.Left;
        _animDirBox.Children.Add(_leftButton);
        Grid.SetRow(_leftButton, 2);
        Grid.SetColumn(_leftButton, 0);
        
        _upButton = CreateDirButton("↑");
        _upButton.Tag = EntityDirection.Up;
        _animDirBox.Children.Add(_upButton);
        Grid.SetRow(_upButton, 0);
        Grid.SetColumn(_upButton, 2);
        
        _upRightButton = CreateDirButton("↗");
        _upRightButton.IsEnabled = false;
        _upRightButton.Tag = EntityDirection.UpRight;
        _animDirBox.Children.Add(_upRightButton);
        Grid.SetRow(_upRightButton, 0);
        Grid.SetColumn(_upRightButton, 4);
        
        _rightButton = CreateDirButton("→");
        _rightButton.Tag = EntityDirection.Right;
        _animDirBox.Children.Add(_rightButton);
        Grid.SetRow(_rightButton, 2);
        Grid.SetColumn(_rightButton, 4);
        
        _upLeftButton = CreateDirButton("↖");
        _upLeftButton.IsEnabled = false;
        _upLeftButton.Tag = EntityDirection.UpLeft;
        _animDirBox.Children.Add(_upLeftButton);
        Grid.SetRow(_upLeftButton, 0);
        Grid.SetColumn(_upLeftButton, 0);
        
        _centerButton = CreateDirButton("●");
        _centerButton.Tag = EntityDirection.Center;
        _animDirBox.Children.Add(_centerButton);
        Grid.SetRow(_centerButton, 2);
        Grid.SetColumn(_centerButton, 2);
        _currentDirectionButton = _centerButton;
        _centerButton.Foreground = Brushes.Red;
        _currentAnimationDirection = EntityDirection.Center;
        
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
        foreach (var featureData in _data.Features)
        {
            var urn = featureData.FeatureUrn;

            var animations = EngineServices.FeaturesManager.GetEntityRequiredAnimations(urn);
            
            foreach (var anim in animations)
            {
                
                ListBoxItem item = new ListBoxItem
                {
                    Content = anim.AnimDisplayName,
                    Tag = anim
                };
                AnimationList.Items.Add(item);
                _basicAnimationsNames.Add(anim.AnimDisplayName);
                ToolTip.SetTip(item, $"From feature: {urn}");
                ToolTip.SetPlacement(item, PlacementMode.RightEdgeAlignedBottom);
            }
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

        if (_basicAnimationsNames.Contains(animationName))
        {
            // Cannot remove basic animations
            return;
        }

        // _data.AnimationsMapping.Remove(animationName);
        _availableAnimationsNames.Remove(animationName);
        AnimationList.Items.Remove(selectedItem);
        
        if(_currentAnimationName != animationName) return;
        
        _selectedAnimationData = null;
        AnimationPreviewer.Stop(false);
        AnimationPreviewer.ClearImage();
    }

    private void AddAnimationButtonOnClick(object? sender, RoutedEventArgs e)
    {
        var newAnimationName = AddAnimationLabel.Text?.Trim();
        if (string.IsNullOrEmpty(newAnimationName)) return;
        // if (_data.AnimationsMapping.ContainsKey(newAnimationName))
        // {
        //     // Animation already exists
        //     return;
        // }

        // _data.AnimationsMapping.Add(newAnimationName, new DirectionalAnimationSet());
        
        ListBoxItem item = new ListBoxItem
        {
            Content = newAnimationName,
            Tag = newAnimationName
        };
        AnimationList.Items.Add(item);
        
        _availableAnimationsNames.Add(newAnimationName);
        
        AddAnimationLabel.Text = string.Empty;
    }

    private void OnAnimationDirectionButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.Tag is not EntityDirection direction) return;
        if(_currentAnimationDirection == direction) return;

        if (_currentDirectionButton != null)
        {
            _currentDirectionButton.ClearValue(ForegroundProperty);
        }
        button.Foreground = Brushes.Red;
        _currentDirectionButton = button;
        
        _currentAnimationDirection = direction;
        
        if(_currentAnimationIdx < 0)
            return;
        
        var directionalAnimationsSet = _data.AnimationsMapping[_currentAnimationIdx];
        Ulid animationId = directionalAnimationsSet.GetAnimation(_currentAnimationDirection);
        if (animationId == Ulid.Empty)
        {
            _selectedAnimationData = EngineServices.AssetsManager.CreateAsset<AnimationDef>();
            _selectedAnimationData.Name = _currentAnimationName;
            directionalAnimationsSet.SetAnimation(_currentAnimationDirection, _selectedAnimationData.Unique);
        }
        else
        {
            if (!EngineServices.AssetsManager.TryResolveAsset<AnimationDef>(animationId, out var animationData))
            {
                Logger.Error("Failed to resolve animation with ID {AnimationId}", animationId);
                return;
            }
            _selectedAnimationData = animationData;
        }
        AnimationPreviewer.Stop(false);
        AnimationPreviewer.ClearImage();
        AnimationPreviewer.UpdateFPS(_selectedAnimationData.Fps);
        AnimationPreviewer.AnimationDefinition = _selectedAnimationData;
        AnimationPreviewer.UpdateFrame();
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
            _data.PortraitPath = newPath;
        }
        else
        {
            // Handle the case where the file does not exist
            PortraitImage.Source = null;
            _data.PortraitPath = null;
        }
    }

    private void OnImportAnimationButtonChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(PathPicker.SelectedPaths)) return;
        if (e.NewValue is not List<string> paths) return;
        if (paths.Count == 0) return;
        if (_selectedAnimationData == null) return;
        
        var newPath = paths[0];
        if (string.IsNullOrEmpty(newPath)) return;
        
        ImportAnimationButton.SuggestedStartPath = Path.GetDirectoryName(newPath) ?? string.Empty;
        
        if(File.Exists(newPath))
        {
            var tempSpriteSheetDef = EngineServices.AssetsManager.CreateTransientAsset<SpritesheetDef>();
            tempSpriteSheetDef.ImagePath = (newPath);
            tempSpriteSheetDef.FrameWidth = 16;
            tempSpriteSheetDef.FrameHeight = 32;
            tempSpriteSheetDef.CalculateValues();

            _selectedAnimationData.FrameIndexes = tempSpriteSheetDef.GetAllRowIndexes(0);
            _selectedAnimationData.SpritesheetId = tempSpriteSheetDef.Unique;
        
            AnimationPreviewer.AnimationDefinition = _selectedAnimationData;
        }
    }

    private void OnAnimationSelectedChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AnimationList.SelectedItem is not ListBoxItem selectedItem) return;
        if (selectedItem.Tag is not EntityFeatureAnimationRequirement animation) return;

        _currentAnimationName = animation.AnimDisplayName;
        _currentAnimationIdx = EngineServices.ECS.StateRegistry.GetActionId(animation.AnimUrn);
        
        if (!_data.AnimationsMapping.TryGetValue(_currentAnimationIdx, out var directionalAnimationSet))
        {
            directionalAnimationSet = new DirectionalAnimationSet();
            _data.AnimationsMapping.Add(_currentAnimationIdx, directionalAnimationSet);
        }
        
        Ulid animationId = directionalAnimationSet.GetAnimation(_currentAnimationDirection);
        if (animationId == Ulid.Empty)
        {
            _selectedAnimationData = EngineServices.AssetsManager.CreateAsset<AnimationDef>();
            _selectedAnimationData.Name = _currentAnimationName;
            directionalAnimationSet.SetAnimation(_currentAnimationDirection, _selectedAnimationData.Unique);
        }
        else
        {
            if (!EngineServices.AssetsManager.TryResolveAsset<AnimationDef>(animationId, out var animationData))
            {
                Logger.Error("Failed to resolve animation with ID {AnimationId}", animationId);
                return;
            }
            _selectedAnimationData = animationData;
        }
        
        SetAnimationPossibleDirection(animation.Direction);
        
        AnimationPreviewer.Stop(false);
        AnimationPreviewer.ClearImage();
        AnimationPreviewer.UpdateFPS(_selectedAnimationData.Fps);

        RemoveAnimationButton.IsEnabled = !_basicAnimationsNames.Contains(_selectedAnimationData.Name);

        AnimationPreviewer.AnimationDefinition = _selectedAnimationData;
        AnimationPreviewer.UpdateFrame();
        if(AutoPlayCheckBox.IsChecked.HasValue && AutoPlayCheckBox.IsChecked.Value)
            AnimationPreviewer.Play();
    }

    private void SetAnimationPossibleDirection(AnimationDirection direction)
    {
        bool four = direction >= AnimationDirection.FourDirections;
        bool eight = direction >= AnimationDirection.EightDirections;
        
        _upButton.IsEnabled = four;
        _downButton.IsEnabled = four;
        _leftButton.IsEnabled = four;
        _rightButton.IsEnabled = four;
        _upLeftButton.IsEnabled = eight;
        _upRightButton.IsEnabled = eight;
        _downLeftButton.IsEnabled = eight;
        _downRightButton.IsEnabled = eight; 
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
        if (_selectedAnimationData == null) return;
        _selectedAnimationData.Fps = newFps;
    }
    #endregion
}
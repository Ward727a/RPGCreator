using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.UI.Common;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class BulkAnimationImportControl : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    public Action<Dictionary<string, AnimationDef>, IAssetScope> OnImport;
    #endregion
    
    #region Properties
    
    // Need to separate those by the file name later if multiple images are allowed
    // Because right now, if multiple images are allowed, the map will be overridden, and row 1 on image 1 will conflict with row 1 on image 2
    public Dictionary<string, Dictionary<int, string>> RowToAnimationNameMap { get; private set; } = new Dictionary<string, Dictionary<int, string>>();
    
    private int _selectedRow = -1;
    private string _currentImagePath = string.Empty;
    private List<string> _animationNames = new List<string>();
    private List<string> _usedAnimationNames = new List<string>();
    private Dictionary<string, ListBoxItem> _assignedAnimationsItems = new();
    private Dictionary<string, Dictionary<int, Bitmap>> _imageRowCache = new();
    private Dictionary<string, string> _imageCache = new();

    private IAssetScope AssetScope;
    
    #endregion
    
    #region Components
    
    private Grid Body { get; set; }
    private Grid LeftGrid { get; set; }
    
    private AnimationPreviewer Previewer { get; set; }

    private ScrollBox ScrollList;
    private StackPanel ScrollListContent;
    private ListBox AssignedAnimationsListBox;
    
    private Grid TopPanel { get; set; }
    private PathPicker ImportButton { get; set; }
    private ComboBox ImageListBox { get; set; }
    private ComboBox AssignedToInput { get; set; }
    
    private StackPanel SpritesheetPanel { get; set; }
    private MoveableCanvas SpritesheetCanvas { get; set; }
    private Image SpritesheetImage { get; set; }
    private Canvas SpritesheetOverlayCanvas { get; set; }
    private TextBlock SpritesheetText { get; set; }
    
    private DataGrid AnimationsDataGrid { get; set; }
    
    private StackPanel ButtonsPanel { get; set; }
    private Button ImportAllButton { get; set; }
    private Button CancelButton { get; set; }
    
    #endregion
    
    #region Constructors

    public BulkAnimationImportControl(Action<Dictionary<string, AnimationDef>, IAssetScope> onImport, List<string> animationNames)
    {
        _animationNames = animationNames;
        OnImport = onImport;
        CreateComponents();
        RegisterEvents();
        Content = Body;
        AssetScope = EngineServices.AssetsManager.CreateAssetScope("BulkAnimationImportScope");
    }
    
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *, 0,auto"),
            RowDefinitions = new RowDefinitions("*, Auto")
        };
        
        ScrollList = new ScrollBox()
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Body.Children.Add(ScrollList);
        Grid.SetColumn(ScrollList, 0);
        
        ScrollListContent = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Top
        };
        ScrollList.Content = ScrollListContent;

        ScrollListContent.Children.Add(new TextBlock()
        {
            Text = "Assigned Animations(0)"
        });
        
        AssignedAnimationsListBox = new ListBox()
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        ScrollListContent.Children.Add(AssignedAnimationsListBox);

        LeftGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *, *"),
        };
        Body.Children.Add(LeftGrid);
        Grid.SetColumn(LeftGrid, 1);
        
        Previewer = new AnimationPreviewer()
        {
            FrameSize = new Size(42, 64),
        };
        Body.Children.Add(Previewer);
        Grid.SetColumn(Previewer, 3);
        
        TopPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, Auto, *"),
            Margin = new Thickness(5),
        };
        LeftGrid.Children.Add(TopPanel);
        Grid.SetRow(TopPanel, 0);
        
        ImportButton = new PathPicker()
        {
            Title = "Import new image",
            FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
            AllowMultiple = false,
            UsePickerType = UsePickerTypes.OpenFile,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        TopPanel.Children.Add(ImportButton);
        
        if (Application.Current.TryFindResource("ButtonPathPicker", out var themeObj)
            && themeObj is ControlTheme theme)
        {
            ImportButton.Theme = theme;
        }
        
        ImageListBox = new ComboBox()
        {
            Margin = new Thickness(5),
            IsTextSearchEnabled = true,
        };
        TopPanel.Children.Add(ImageListBox);
        Grid.SetColumn(ImageListBox, 1);
        
        AssignedToInput = new ComboBox()
        {
            PlaceholderText = "Assign to animation...",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AssignedToInput.Items.Add(new ComboBoxItem()
        {
            Content = "None",
            Tag = null
        });
        foreach (var animationName in _animationNames)
        {
            AssignedToInput.Items.Add(new ComboBoxItem()
            {
                Content = animationName,
                Tag = animationName
            });
        }
        TopPanel.Children.Add(AssignedToInput);
        Grid.SetColumn(AssignedToInput, 2);

        SpritesheetPanel = new StackPanel();
        LeftGrid.Children.Add(SpritesheetPanel);
        Grid.SetRow(SpritesheetPanel, 1);

        SpritesheetCanvas = new MoveableCanvas()
        {
            Width = 400,
            Height = 320,
            ClipToBounds = true,
        };
        SpritesheetPanel.Children.Add(SpritesheetCanvas);
        
        SpritesheetImage = new Image();
        SpritesheetCanvas.AddMoveableElement(SpritesheetImage);
        
        SpritesheetOverlayCanvas = new Canvas();
        SpritesheetCanvas.AddMoveableElement(SpritesheetOverlayCanvas);
        
        SpritesheetText = new TextBlock()
        {
            Text = "Click to select animation row",
            Margin = new Thickness(5),
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.Gray,
        };
        SpritesheetPanel.Children.Add(SpritesheetText);

        AnimationsDataGrid = new DataGrid();
        LeftGrid.Children.Add(AnimationsDataGrid);
        Grid.SetRow(AnimationsDataGrid, 2);
        
        ButtonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(5),
        };
        Body.Children.Add(ButtonsPanel);
        Grid.SetRow(ButtonsPanel, 1);
        Grid.SetColumn(ButtonsPanel, 3);
        
        ImportAllButton = new Button()
        {
            Content = "Import All",
            Margin = new Thickness(5),
        };
        ButtonsPanel.Children.Add(ImportAllButton);
        
        CancelButton = new Button()
        {
            Content = "Cancel",
            Margin = new Thickness(5),
        };
        ButtonsPanel.Children.Add(CancelButton);
    }

    private Bitmap GetOrCreateRowBitmap(string imagePath, int rowIndex)
    {
        if (!_imageRowCache.ContainsKey(imagePath))
        {
            _imageRowCache[imagePath] = new Dictionary<int, Bitmap>();
        }

        if (_imageRowCache[imagePath].ContainsKey(rowIndex))
        {
            return _imageRowCache[imagePath][rowIndex];
        }

        var bitmap = new Bitmap(imagePath);
        var cropped = new CroppedBitmap(bitmap, new PixelRect(0, rowIndex * 64, (int)bitmap.Size.Width, 64));
        var bmp = ImageUtil.ConvertCroppedBitmapToBitmap(cropped);
        _imageRowCache[imagePath][rowIndex] = bmp;
        return bmp;
    }

    private void RegisterEvents()
    {
        ImportButton.PropertyChanged += OnImportNewImage;
        SpritesheetCanvas.CanvasBody.PointerPressed += CanvasBodyOnPointerPressed;
        ImageListBox.SelectionChanged += OnImageSelectionChanged;
        AssignedToInput.SelectionChanged += OnAssignedToInputOnSelectionChanged;
        AssignedAnimationsListBox.SelectionChanged += OnAssignedAnimationsListBoxOnSelectionChanged;
        ImportAllButton.Click += (s, e) =>
        {
            var animationsToImport = new Dictionary<string, AnimationDef>();
            foreach (var (imagePath, rowMap) in RowToAnimationNameMap)
            {
                foreach (var (rowIndex, animationName) in rowMap)
                {
                    var spritesheetDef = EngineServices.AssetsManager.CreateTransientAsset<SpritesheetDef>(AssetScope);
                    spritesheetDef.ImagePath = imagePath;
                    spritesheetDef.FrameWidth = 48;
                    spritesheetDef.FrameHeight = 64;
                    
                    var animationDef = EngineServices.AssetsManager.CreateTransientAsset<AnimationDef>(AssetScope);
                    animationDef.SpritesheetId = spritesheetDef.Unique;
                    animationDef.FrameIndexes = spritesheetDef.GetAllRowIndexes(rowIndex);
                    animationsToImport[animationName] = animationDef;
                }
            }
            OnImport?.Invoke(animationsToImport, AssetScope);
        };
    }

    private void SelectRow(int rowIndex)
    {
        // Clear previous overlays
        SpritesheetOverlayCanvas.Children.Clear();
        
        SpritesheetText.Text = $"Selected Row: {rowIndex}";
        
        // Draw overlay rectangle on the selected row
        var overlayRect = new Rectangle()
        {
            Width = SpritesheetImage.Source?.Size.Width ?? 0,
            Height = 64,
            Fill = new SolidColorBrush(Color.FromArgb(50, 2, 155, 000)),
        };
        Canvas.SetLeft(overlayRect, 0);
        Canvas.SetTop(overlayRect, rowIndex * 64);
        SpritesheetOverlayCanvas.Children.Add(overlayRect);
        _selectedRow = rowIndex;
    }
    
    private void LoadPreviewerForCurrentSelection()
    {
        if (string.IsNullOrWhiteSpace(_currentImagePath) || _selectedRow < 0) return;
        var spritesheetDef = EngineServices.AssetsManager.CreateTransientAsset<SpritesheetDef>(AssetScope);
        spritesheetDef.ImagePath = _currentImagePath;
        spritesheetDef.FrameWidth = 48;
        spritesheetDef.FrameHeight = 64;
        var animationDef = EngineServices.AssetsManager.CreateTransientAsset<AnimationDef>(AssetScope);
        animationDef.SpritesheetId = spritesheetDef.Unique;
        animationDef.FrameIndexes = spritesheetDef.GetAllRowIndexes(_selectedRow);
        
        // Previewer.AnimationPath = bitmapPath;
        Previewer.Stop(false);
        Previewer.ClearImage();
        Previewer.AnimationDefinition = animationDef;
        Previewer.UpdateFrame();
        Previewer.Play();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        AssetScope.Dispose();
    }

    #endregion

    #region Events Handlers
    private void OnAssignedToInputOnSelectionChanged(object? s, SelectionChangedEventArgs e)
    {
        var _selectedAnimation = AssignedToInput.SelectedItem as ComboBoxItem;
        var _selectedAnimationName = _selectedAnimation?.Tag as string;

        if (_selectedAnimationName == null)
        {
            // Clear assignment for the selected row
            if (!string.IsNullOrWhiteSpace(_currentImagePath) && RowToAnimationNameMap.ContainsKey(_currentImagePath) && RowToAnimationNameMap[_currentImagePath].ContainsKey(_selectedRow))
            {
                var existingAnimationName = RowToAnimationNameMap[_currentImagePath][_selectedRow];
                _usedAnimationNames.Remove(existingAnimationName);
                RowToAnimationNameMap[_currentImagePath].Remove(_selectedRow);

                // Clear selection in the AssignedAnimationsListBox
                _assignedAnimationsItems[existingAnimationName].IsSelected = false;
                AssignedAnimationsListBox.Items.Remove(_assignedAnimationsItems[existingAnimationName]);
                _assignedAnimationsItems.Remove(existingAnimationName);
            }

            return;
        }

        if (_usedAnimationNames.Contains(_selectedAnimationName))
        {
            Logger.Error("Animation name '{AnimationName}' is already assigned to another row.", _selectedAnimationName);
            // Animation name already used, ignore selection
            return;
        }

        if (!string.IsNullOrWhiteSpace(_currentImagePath))
        {
            if (!RowToAnimationNameMap.ContainsKey(_currentImagePath))
            {
                RowToAnimationNameMap[_currentImagePath] = new Dictionary<int, string>();
            }
        }

        if (RowToAnimationNameMap[_currentImagePath].ContainsKey(_selectedRow))
        {
            var existingAnimationName = RowToAnimationNameMap[_currentImagePath][_selectedRow];
            _usedAnimationNames.Remove(existingAnimationName);
            _assignedAnimationsItems[existingAnimationName].IsSelected = false;
            AssignedAnimationsListBox.Items.Remove(_assignedAnimationsItems[existingAnimationName]);
            _assignedAnimationsItems.Remove(existingAnimationName);
        }

        if (_selectedRow >= 0)
        {
            RowToAnimationNameMap[_currentImagePath][_selectedRow] = _selectedAnimationName ?? $"animation_{_selectedRow}";

            _usedAnimationNames.Add(_selectedAnimationName);
            _assignedAnimationsItems.TryAdd(_selectedAnimationName, new ListBoxItem() { Content = _selectedAnimationName, Tag = (_currentImagePath, _selectedRow) });
            AssignedAnimationsListBox.Items.Add(_assignedAnimationsItems[_selectedAnimationName]);
        }
    }
    private void OnAssignedAnimationsListBoxOnSelectionChanged(object? s, SelectionChangedEventArgs e)
    {
        if (AssignedAnimationsListBox.SelectedItem is ListBoxItem selectedItem)
        {
            var (imagePath, rowIndex) = ((string, int))selectedItem.Tag;

            if (imagePath != _currentImagePath)
            {
                // Change selected image in ImageListBox
                for (int i = 0; i < ImageListBox.Items.Count; i++)
                {
                    if (ImageListBox.Items[i] is ComboBoxItem item && item.Tag as string == imagePath)
                    {
                        ImageListBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            SelectRow(rowIndex);

            LoadPreviewerForCurrentSelection();

            // Update AssignedToInput selection based on the selected row
            if (_selectedRow >= 0 && RowToAnimationNameMap.ContainsKey(_currentImagePath) && RowToAnimationNameMap[_currentImagePath].ContainsKey(_selectedRow))
            {
                var animationName = RowToAnimationNameMap[_currentImagePath][_selectedRow];
                foreach (ComboBoxItem item in AssignedToInput.Items)
                {
                    if (item.Tag as string == animationName)
                    {
                        AssignedToInput.SelectedItem = item;
                        break;
                    }
                }
            }
        }
    }

    private void OnImportNewImage(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != PathPicker.SelectedPathsProperty) return;
        var paths = e.NewValue as List<string>;
        if (paths is null || paths.Count == 0) return;
        
        var imagePath = paths[0];
        _currentImagePath = imagePath;

        SpritesheetImage.Source = new Bitmap(imagePath);
        var item = new ComboBoxItem()
        {
            Content = System.IO.Path.GetFileName(imagePath),
            Tag = imagePath
        };
        ToolTip.SetTip(item, imagePath);
        ImageListBox.Items.Add(item);
        ImageListBox.SelectedIndex = ImageListBox.Items.Count - 1;
    }
    private void CanvasBodyOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if(e.GetCurrentPoint(SpritesheetCanvas.CanvasBody).Properties.IsLeftButtonPressed)
        {
            // Normalize click position to be a multiple of frame size (42x64)
            var position = e.GetPosition(SpritesheetImage);
            var frameX = (int)(position.X / 42);
            var frameY = (int)(position.Y / 64);
            
            SelectRow(frameY);
            
            if (!string.IsNullOrWhiteSpace(_currentImagePath))
            {
                if (!RowToAnimationNameMap.ContainsKey(_currentImagePath))
                {
                    RowToAnimationNameMap[_currentImagePath] = new Dictionary<int, string>();
                }
            }
            
            // Update AssignedToInput selection based on the selected row
            if(_selectedRow >= 0 && 
               RowToAnimationNameMap.ContainsKey(_currentImagePath) && 
               RowToAnimationNameMap[_currentImagePath].ContainsKey(_selectedRow))
            {
                var animationName = RowToAnimationNameMap[_currentImagePath][_selectedRow];
                foreach (ComboBoxItem item in AssignedToInput.Items)
                {
                    if (item.Tag as string == animationName)
                    {
                        AssignedToInput.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                AssignedToInput.SelectedItem = null;
            }
            LoadPreviewerForCurrentSelection();
        }
    }
    
    private void OnImageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ImageListBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var imagePath = selectedItem.Tag as string;
            if (imagePath != null)
            {
                _currentImagePath = imagePath;
                SpritesheetImage.Source = new Bitmap(imagePath);
                // Clear previous overlays and text
                SpritesheetOverlayCanvas.Children.Clear();
                SpritesheetText.Text = "Click to select animation row";
                _selectedRow = -1;
                AssignedToInput.SelectedItem = null;
                AssignedToInput.SelectedIndex = -1;
                Previewer.Stop(false);
                Previewer.ClearImage();
            }
        }
    }
    #endregion
}
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using RPGCreator.Core.Type.Assets;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor.RuleEditor;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor;

//TODO: Add rules management (add, edit, delete)
//TODO: Add preview of the autotilings with the rules applied
//TODO: Remove select rectangle when the group is changed

public class AutotileEditorRuleEditorWindowControl : UserControl
{
    private Tileset _tileset;
    private GroupItemControl _selectedGroupItem;

    // All the "null!" are just to suppress the nullability warnings, as these fields will be initialized in the CreateComponents method.
    private Grid _mainGrid = null!;

    private Grid _leftBarGrid = null!;
    private Grid _previewGrid = null!;

    private ComboBox _groupSelector = null!;
    private StackPanel _leftBarPanel = null!;

    private Canvas _tileSelectMainCanvas = null!;
    private Canvas _tileSelectSubCanvas = null!;
    private Canvas _tileSelectShadowCanvas = null!;
    private Image _tileSelectImage = null!;
    private Rectangle? _tileSelectRect;

    private Grid _leftBarButtonGrid = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;

    private ListBox _leftBarRuleListBox = null!;

    private Canvas _previewMainCanvas = null!;
    private Canvas _previewSubCanvas = null!;

    public AutotileEditorRuleEditorWindowControl(Tileset tileset)
    {
        _tileset = tileset ?? throw new ArgumentNullException(nameof(tileset), "Tileset cannot be null");

        CreateComponents();

        this.Content = _mainGrid;

        RefreshGroupSelector();
    }

    private void CreateComponents()
    {
        _mainGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
        };

        _leftBarGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
        };
        _mainGrid.Children.Add(_leftBarGrid);

        CreateLeftContent();

        _previewGrid = new Grid()
        {
            Margin = new Avalonia.Thickness(5),
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        _mainGrid.Children.Add(_previewGrid);
        Grid.SetColumn(_previewGrid, 1);

        CreatePreviewContent();
    }

    private void CreateLeftContent()
    {
        _groupSelector = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(5),
        };
        _leftBarGrid.Children.Add(_groupSelector);
        _groupSelector.SelectionChanged += OnGroupSelectorSelectionChanged;

        _leftBarPanel = new StackPanel()
        {
            Spacing = 5,
        };
        _leftBarGrid.Children.Add(_leftBarPanel);
        Grid.SetRow(_leftBarPanel, 1);

        _tileSelectMainCanvas = new Canvas()
        {
            Width = 400,
            Height = 400,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            Margin = new Thickness(5),
            ClipToBounds = true
        };
        _leftBarPanel.Children.Add(_tileSelectMainCanvas);
        _tileSelectMainCanvas.PointerPressed += OnMovingRoot;
        _tileSelectMainCanvas.PointerMoved += OnMovingRoot;
        _tileSelectMainCanvas.PointerReleased += OnMovingRootEnd;
        
        _tileSelectSubCanvas = new Canvas()
        {
            Width = 400,
            Height = 400,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
        };
        _tileSelectMainCanvas.Children.Add(_tileSelectSubCanvas);
        _tileSelectSubCanvas.PointerPressed += OnSelectTileset;
        
        _tileSelectImage = new Image()
        {
            Source = _tileset.GetBitmap(),
            Stretch = Avalonia.Media.Stretch.Uniform,
        };
        _tileSelectSubCanvas.Children.Add(_tileSelectImage);

        _tileSelectShadowCanvas = new Canvas()
        {
            Width = 400,
            Height = 400,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
        };
        _tileSelectSubCanvas.Children.Add(_tileSelectShadowCanvas);
        
        _leftBarPanel.Children.Add(new Separator());

        _leftBarButtonGrid = new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            ColumnDefinitions = new ColumnDefinitions("Auto, Auto, Auto"),
        };
        _leftBarPanel.Children.Add(_leftBarButtonGrid);

        _addButton = new Button()
        {
            Content = "Add",
            Margin = new Thickness(5),
        };
        _leftBarButtonGrid.Children.Add(_addButton);
        Grid.SetColumn(_addButton, 0);

        _editButton = new Button()
        {
            Content = "Edit",
            Margin = new Thickness(5),
        };
        _leftBarButtonGrid.Children.Add(_editButton);
        Grid.SetColumn(_editButton, 1);

        _deleteButton = new Button()
        {
            Content = "Delete",
            Margin = new Thickness(5),
        };
        _leftBarButtonGrid.Children.Add(_deleteButton);
        Grid.SetColumn(_deleteButton, 2);

        _leftBarRuleListBox = new ListBox()
        {
            Margin = new Thickness(5),
        };
        _leftBarPanel.Children.Add(_leftBarRuleListBox);
    }

    private void CreatePreviewContent()
    {
        _previewMainCanvas = new Canvas()
        {
            Height = 256,
            Width = 256,
            ClipToBounds = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            Margin = new Thickness(5),
        };
        _previewGrid.Children.Add(_previewMainCanvas);

        _previewSubCanvas = new Canvas()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
        };
        _previewMainCanvas.Children.Add(_previewSubCanvas);
    }

    private void RefreshGroupSelector()
    {
        _groupSelector.Items.Clear();
        foreach (var autotilesGroup in _tileset.Groups)
        {
            var groupItem = new GroupItemControl(autotilesGroup);
            _groupSelector.Items.Add(groupItem);
        }
    }
    
    private void OnGroupSelectorSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_groupSelector.SelectedItem is GroupItemControl selectedGroup)
        {
            _selectedGroupItem = selectedGroup;
            Console.WriteLine($"Selected group: {selectedGroup.Group.Name}");
            
            // Add shadow on the tile that are not concerned by the group
            var shadowBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
            var maxCol = _tileset.GetBitmap().Size.Width / _tileset.tile_width;
            var maxRow = _tileset.GetBitmap().Size.Height / _tileset.tile_height;
            
            _tileSelectShadowCanvas.Children.Clear();
            
            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    
                    if(selectedGroup.Group.HasTileAt(new(col, row)))
                    {
                        // If the tile is part of the group, we don't draw a shadow
                        continue;
                    }
                    
                    var rect = new Rectangle
                    {
                        Width = _tileset.tile_width,
                        Height = _tileset.tile_height,
                        Fill = shadowBrush,
                        Opacity = .7,
                    };
                    
                    Canvas.SetLeft(rect, col * _tileset.tile_width);
                    Canvas.SetTop(rect, row * _tileset.tile_height);
                    
                    _tileSelectShadowCanvas.Children.Add(rect);
                }
            }
            
            _leftBarRuleListBox.Items.Clear();
            
            
        }
    }
    private void OnSelectTileset(object? sender, PointerPressedEventArgs e)
    {
        // This event handler is triggered when a tileset is selected (Left-click)
        if (e.GetCurrentPoint(_tileSelectSubCanvas).Properties.IsLeftButtonPressed)
        {
            var position = e.GetPosition(_tileSelectSubCanvas);
            int tileWidth = _tileset.tile_width;
            int tileHeight = _tileset.tile_height;

            int tileCol = (int)(position.X / tileWidth);
            int tileRow = (int)(position.Y / tileHeight);
            Console.WriteLine($"Tileset selected at position: col:{tileCol} row:{tileRow}");

            if (!_selectedGroupItem.Group.HasTileAt(new(tileCol, tileRow)))
                return;
            
            // Draw a rectangle around the selected tile
            if (_tileSelectRect == null)
            {
                _tileSelectRect = new Rectangle
                {
                    Width = tileWidth,
                    Height = tileHeight,
                    Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red),
                    StrokeThickness = 2,
                    Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent)
                };
                _tileSelectSubCanvas.Children.Add(_tileSelectRect);
            }

            _tileSelectRect.Width = tileWidth;
            _tileSelectRect.Height = tileHeight;
            
            Canvas.SetLeft(_tileSelectRect, tileCol * tileWidth);
            Canvas.SetTop(_tileSelectRect, tileRow * tileHeight);
        }
    }
    private void OnMovingRoot(object? sender, PointerEventArgs e)
    {
        // This event handler is triggered when the root tileset canvas is moved (Right-click and drag)
        if (!e.GetCurrentPoint(_tileSelectMainCanvas).Properties.IsRightButtonPressed) return;
        
        var CurrentPointOfInner = NewInnerPosition;

        if (!IsMovingRoot)
        {
            // If the left button is pressed, we start moving the root tileset canvas
            IsMovingRoot = true;
            LastMousePosition = e.GetPosition(_tileSelectMainCanvas);
            Console.WriteLine("Root tileset canvas movement started.");
            return;
        }

        // If the left button is pressed, we can handle the movement logic here
        var position = e.GetPosition(_tileSelectMainCanvas);

        // Check if the mouse has moved
        if (LastMousePosition == position)
        {
            // If the mouse has not moved, we do nothing
            return;
        }

        var newPosition = new Point(position.X - LastMousePosition.X, position.Y - LastMousePosition.Y) +
                          CurrentPointOfInner;

        // Lock the new position to be within the bounds of the InnerTilesetCanvas
        newPosition = newPosition.WithX(
            Math.Min(
                0,
                Math.Max(
                    newPosition.X,
                    (_tileset.GetBitmap().Size.Width - _tileSelectMainCanvas.Width) * -1)
            )
        );
        newPosition = newPosition.WithY(
            Math.Min(
                0,
                Math.Max(
                    newPosition.Y,
                    (_tileset.GetBitmap().Size.Height - _tileSelectMainCanvas.Height) * -1)
            )
        );

        Console.WriteLine($"Root tileset canvas moved to position: {newPosition}");

        Canvas.SetLeft(_tileSelectSubCanvas, newPosition.X);
        Canvas.SetTop(_tileSelectSubCanvas, newPosition.Y);

        HasMovedRoot = true;
    }

    public Point LastMousePosition { get; set; }

    public bool HasMovedRoot { get; set; }

    private void OnMovingRootEnd(object? sender, PointerReleasedEventArgs e)
    {
        // This event handler is triggered when the root tileset canvas movement ends (Right-click released)
        if (e.GetCurrentPoint(_tileSelectMainCanvas).Properties.IsRightButtonPressed || !IsMovingRoot) return;
        
        NewInnerPosition = new Point(Canvas.GetLeft(_tileSelectSubCanvas), Canvas.GetTop(_tileSelectSubCanvas));
        // If the right button is released, we stop moving the root tileset canvas
        IsMovingRoot = false;
        Console.WriteLine("Root tileset canvas movement ended.");
    }

    public bool IsMovingRoot { get; set; }

    public Point NewInnerPosition { get; set; }
}
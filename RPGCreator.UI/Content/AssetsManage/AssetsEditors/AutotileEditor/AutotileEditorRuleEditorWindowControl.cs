using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RPGCreator.Core;
using RPGCreator.Core.Types.Assets;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor.RuleEditor;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor;

//TODO: The rule system still needs to be worked on, this is just a basic buggy implementation to get started.

public class AutotileEditorRuleEditorWindowControl : UserControl
{
    private AutoTilesetDef _tilesetDefinition;
    private AutotileGroupDef _autotileGroupDefinition;
    private GroupItemControl? _selectedGroupItem;
    private AutotileDef? _selectedAutotilingDefinition;
    private AutotileRule? _selectedRule;

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
    private TileLayerDefinition _previewFakeLayer = null!; // This will be used ONLY for the rule system, not to display the autotilings
    
    private StackPanel _ruleEditorPanel = null!;
    private ComboBox _ruleTypeComboBox = null!;
    private TextBox _ruleNameTextBox = null!;
    private Grid _ruleGrid = null!;
    private ComboBox _ruleSideComboBox = null!;
    private TextBox _ruleTagsTextBox = null!;
    private TextBox _ruleDescriptionTextBox = null!;
    private Button _ruleConfirmButton = null!;

    public AutotileEditorRuleEditorWindowControl(AutoTilesetDef autoTilesetDefinition)
    {
        _tilesetDefinition = autoTilesetDefinition ?? throw new ArgumentNullException(nameof(autoTilesetDefinition), "AutoTilesetDefinition cannot be null");

        CreateComponents();
        RegisterEvents();

        this.Content = _mainGrid;

        RefreshGroupSelector();
    }

    #region UI Methods
    
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
        CreateRuleEditorContent();
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
            Source = _selectedGroupItem?.GroupDefinition.TilesetDef.GetBitmap(),
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
        _previewFakeLayer = new TileLayerDefinition();
    }

    private void CreateRuleEditorContent()
    {
        _ruleEditorPanel = new StackPanel()
        {
            
            IsVisible = false, // Initially hidden, can be shown when a rule is selected
            
        };
        _previewGrid.Children.Add(_ruleEditorPanel);
        
        _ruleTypeComboBox = new ComboBox()
        {
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _ruleEditorPanel.Children.Add(_ruleTypeComboBox);
        _ruleTypeComboBox.Items.Add("Whitelist"); // In this context, Whitelist means the rule allows the tiles to be used if they match the rule
        _ruleTypeComboBox.Items.Add("Blacklist"); // In this context, Blacklist means the rule prevents the tiles from being used if they match the rule
        _ruleTypeComboBox.SelectedIndex = 0; // Default to Whitelist
        
        _ruleNameTextBox = new TextBox()
        {
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Watermark = "Enter rule name",
        };
        _ruleEditorPanel.Children.Add(_ruleNameTextBox);
        
        _ruleGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        _ruleEditorPanel.Children.Add(_ruleGrid);
        
        _ruleSideComboBox = new ComboBox()
        {
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _ruleGrid.Children.Add(_ruleSideComboBox);
        Enum.GetNames(typeof(ERulePos)).ToList().ForEach(x => _ruleSideComboBox.Items.Add(x));
        _ruleSideComboBox.SelectedIndex = 0; // Default to Left
        
        _ruleTagsTextBox = new TextBox()
        {
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Watermark = "Enter tag(s) (comma \",\" separated if multiple)",
        };
        _ruleGrid.Children.Add(_ruleTagsTextBox);
        Grid.SetColumn(_ruleTagsTextBox, 1);
        
        _ruleDescriptionTextBox = new TextBox()
        {
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Watermark = "Enter rule description",
        };
        _ruleEditorPanel.Children.Add(_ruleDescriptionTextBox);
        
        _ruleConfirmButton = new Button()
        {
            Content = "Confirm",
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        _ruleEditorPanel.Children.Add(_ruleConfirmButton);
    }
    
    #endregion
    
    private void RefreshGroupSelector()
    {
        _groupSelector.Items.Clear();
        foreach (var autotilesGroup in _tilesetDefinition.AutotileGroups)
        {
            var groupItem = new GroupItemControl(autotilesGroup);
            _groupSelector.Items.Add(groupItem);
        }
    }
    
    private void OnGroupSelectorSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_groupSelector.SelectedItem is GroupItemControl selectedGroup)
        {
            if (_selectedGroupItem != null)
            {
                // _selectedGroupItem.GroupDefinition.UncombineGroupTags();
            }
            _selectedGroupItem = selectedGroup;
            _autotileGroupDefinition = _selectedGroupItem.GroupDefinition;
            // _selectedGroupItem.GroupDefinition.CombineGroupTags();
            Console.WriteLine($"Selected groupInstance: {selectedGroup.GroupDefinition.Name}");
            _tileSelectImage.Source = _autotileGroupDefinition.TilesetDef?.GetBitmap();
            
            // Add shadow on the tile that are not concerned by the groupInstance
            var shadowBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
            var maxCol = _autotileGroupDefinition.TilesetDef?.GetBitmap().Size.Width / _tilesetDefinition.TileWidth;
            var maxRow = _autotileGroupDefinition.TilesetDef?.GetBitmap().Size.Height / _tilesetDefinition.TileHeight;
            
            _tileSelectShadowCanvas.Children.Clear();
            
            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    
                    if(selectedGroup.GroupDefinition.HasTile(new(col, row)))
                    {
                        // If the tile is part of the groupInstance, we don't draw a shadow
                        continue;
                    }
                    
                    var rect = new Rectangle
                    {
                        Width = _tilesetDefinition.TileWidth,
                        Height = _tilesetDefinition.TileHeight,
                        Fill = shadowBrush,
                        Opacity = .7,
                    };
                    
                    Canvas.SetLeft(rect, col * _tilesetDefinition.TileWidth);
                    Canvas.SetTop(rect, row * _tilesetDefinition.TileHeight);
                    
                    _tileSelectShadowCanvas.Children.Add(rect);
                }
            }
            
            _leftBarRuleListBox.Items.Clear();
        }
    }
    private void OnSelectTileset(object? sender, PointerPressedEventArgs e)
    {
        if (_selectedGroupItem == null)
            return;
        // This event handler is triggered when a tileset is selected (Left-click)
        if (e.GetCurrentPoint(_tileSelectSubCanvas).Properties.IsLeftButtonPressed)
        {
            var position = e.GetPosition(_tileSelectSubCanvas);
            int tileWidth = _tilesetDefinition.TileWidth;
            int tileHeight = _tilesetDefinition.TileHeight;

            int tileCol = (int)(position.X / tileWidth);
            int tileRow = (int)(position.Y / tileHeight);
            Console.WriteLine($"Tileset selected at position: col:{tileCol} row:{tileRow}");

            if (!_autotileGroupDefinition.HasTile(new(tileCol, tileRow)))
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

            _selectedAutotilingDefinition = _autotileGroupDefinition.GetDirectTileAt(new(tileCol, tileRow));
            
            if (_selectedAutotilingDefinition == null)
            {
                Console.WriteLine("Selected autotiling is null, this should not happen.");
                return;
            }
            
            // Populate the left bar rule list box with the rules of the selected autotiling
            _leftBarRuleListBox.Items.Clear();
            foreach (var rule in _selectedAutotilingDefinition.Rules)
            {
                _leftBarRuleListBox.Items.Add(rule.Name);
            }
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
        if (_autotileGroupDefinition.TilesetDef == null)
            return;
        // Lock the new position to be within the bounds of the InnerTilesetCanvas
        newPosition = newPosition.WithX(
            Math.Min(
                0,
                Math.Max(
                    newPosition.X,
                    (_autotileGroupDefinition.TilesetDef.GetBitmap().Size.Width - _tileSelectMainCanvas.Width) * -1)
            )
        );
        newPosition = newPosition.WithY(
            Math.Min(
                0,
                Math.Max(
                    newPosition.Y,
                    (_autotileGroupDefinition.TilesetDef.GetBitmap().Size.Height - _tileSelectMainCanvas.Height) * -1)
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

    
    private void OnClickOnPreview(object? sender, PointerPressedEventArgs e)
    {

        if (_selectedAutotilingDefinition == null)
            return;

        if (_selectedGroupItem == null)
            return;
        
        // This event handler is triggered when the user clicks on the preview canvas
        if (e.GetCurrentPoint(_previewMainCanvas).Properties.IsLeftButtonPressed)
        {
            var clickPosition = e.GetPosition(_previewMainCanvas);
            
            int tileWidth = _tilesetDefinition.TileWidth;
            int tileHeight = _tilesetDefinition.TileHeight;
            int tileCol = (int)(clickPosition.X / tileWidth);
            int tileRow = (int)(clickPosition.Y / tileHeight);
            int tileX = tileCol * tileWidth;
            int tileY = tileRow * tileHeight;
            
            Console.WriteLine($"Preview clicked at position: col:{tileCol} row:{tileRow}");

            // Check if the clicked position is within the bounds of the preview canvas
            if (tileX < 0 || tileY < 0 || tileX >= _previewMainCanvas.Width || tileY >= _previewMainCanvas.Height)
            {
                Console.WriteLine("Clicked position is out of bounds of the preview canvas.");
                return;
            }
            
            // Create a new rectangle to represent the clicked tile (ONLY FOR TESTING PURPOSES)
            var clickedTileRect = new Rectangle
            {
                Width = tileWidth,
                Height = tileHeight,
                Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red),
                StrokeThickness = 2,
                Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent)
            };
            EngineCore.Instance.Scheduler.WaitSecond(3f, ()=>
            {
                // Remove the rectangle after 3 seconds
                _previewSubCanvas.Children.Remove(clickedTileRect);
            });
            Canvas.SetLeft(clickedTileRect, tileX);
            Canvas.SetTop(clickedTileRect, tileY);
            _previewSubCanvas.Children.Add(clickedTileRect);

            if (_autotileGroupDefinition.BaseTile == null)
                return;
            
            var correspondingTile = _selectedGroupItem.GroupDefinition.GetTileAt(_previewFakeLayer, new (tileX, tileY));
            
            if (correspondingTile == null)
            {
                Console.WriteLine("Base tile is null, this should not happen.");
                return;
            }

            var cropped = new CroppedBitmap(correspondingTile.TilesetDef.GetBitmap(), new PixelRect(
                correspondingTile.PositionInTileset.X * tileWidth,
                correspondingTile.PositionInTileset.Y * tileHeight,
                tileWidth,
                tileHeight));
            
            // Place the base tile at the clicked position in the preview
            var baseTileRect = new Image()
            {
                Width = tileWidth,
                Height = tileHeight,
                Source = cropped,
                Opacity = 0.5 // Semi-transparent to indicate it's a base tile
            };
            Canvas.SetLeft(baseTileRect, tileX);
            Canvas.SetTop(baseTileRect, tileY);
            
            if (_previewFakeLayer.TryGetElement(new Core.Types.Internal.Point(tileX, tileY), out var tile))
            {
                // Remove the existing tile at the clicked position
                _previewFakeLayer.TryRemoveElement(new Core.Types.Internal.Point(tileX, tileY), out _);
            }
            
            _previewFakeLayer.AddElement(correspondingTile, new Core.Types.Internal.Point(tileX, tileY));

            RefreshPreview();
            Console.WriteLine($"Base tile placed at position: col:{tileCol} row:{tileRow}");
        }
    }
    
    private void RefreshPreview()
    {
        if (_selectedAutotilingDefinition == null)
        {
            Console.WriteLine("No autotiling selected, cannot refresh preview.");
            return;
        }
        
        EngineCore.Instance.Managers.Assets.TileLayerFactory.Create(_previewFakeLayer).Draw(null);
    }

    private void RegisterEvents()
    {
        RegisterLeftBarEvents();
        RegisterRuleEditEvents();
        RegisterPreviewEvents();
    }

    private void RegisterPreviewEvents()
    {
        _previewMainCanvas.PointerPressed += OnClickOnPreview;
    }

    private void RegisterLeftBarEvents()
    {
        _addButton.Click += AddButtonOnClick;
        _editButton.Click += EditButtonOnClick;
        _deleteButton.Click += DeleteButtonOnClick;
    }

    private void AddButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedAutotilingDefinition == null)
            return;
        _selectedRule = null;
        _previewMainCanvas.IsVisible = false;
        _ruleEditorPanel.IsVisible = true;
        _ruleTypeComboBox.SelectedIndex = 0; // Default to Whitelist
        _ruleNameTextBox.Text = string.Empty;
        _ruleTagsTextBox.Text = string.Empty;
        _ruleDescriptionTextBox.Text = string.Empty;
        _ruleSideComboBox.SelectedIndex = 0; // Default to TOP_LEFT
    }
    private void EditButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (_leftBarRuleListBox.SelectedItem == null)
            return;

        // Get the selected rule from the list box
        var selectedRuleName = _leftBarRuleListBox.SelectedItem.ToString();
        if (string.IsNullOrWhiteSpace(selectedRuleName))
            return;

        _selectedRule = _selectedAutotilingDefinition.Rules.FirstOrDefault(r => r.Name == selectedRuleName);
        if (_selectedRule == null)
            return;

        // Populate the rule editor with the selected rule's data
        _ruleTypeComboBox.SelectedIndex = _selectedRule.Type == ERuleType.WHITELIST ? 0 : 1;
        _ruleNameTextBox.Text = _selectedRule.Name;
        _ruleTagsTextBox.Text = string.Join(", ", _selectedRule.Tags);
        _ruleDescriptionTextBox.Text = _selectedRule.Description;
        _ruleSideComboBox.SelectedIndex = (int)_selectedRule.Side;

        // Show the rule editor panel
        _previewMainCanvas.IsVisible = false;
        _ruleEditorPanel.IsVisible = true;
    }
    private void DeleteButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (_leftBarRuleListBox.SelectedItem == null)
            return;

        // Get the selected rule from the list box
        var selectedRuleName = _leftBarRuleListBox.SelectedItem.ToString();
        if (string.IsNullOrWhiteSpace(selectedRuleName))
            return;

        // Find the rule in the selected autotiling
        var ruleToDelete = _selectedAutotilingDefinition.Rules.FirstOrDefault(r => r.Name == selectedRuleName);
        if (ruleToDelete == null)
            return;

        // Remove the rule from the autotiling
        _selectedAutotilingDefinition.Rules.Remove(ruleToDelete);

        // Remove the rule from the list box
        _leftBarRuleListBox.Items.Remove(selectedRuleName);

        if (_selectedRule == null)
            return;

        if (_selectedRule.Name != selectedRuleName) return;
        
        // If the deleted rule was the selected rule, clear the selection
        _selectedRule = null;
        _leftBarRuleListBox.SelectedItem = null;
        _leftBarRuleListBox.SelectedIndex = -1;
        _ruleEditorPanel.IsVisible = false;
        _previewMainCanvas.IsVisible = true;
    }

    private void RegisterRuleEditEvents()
    {
        _ruleConfirmButton.Click += RuleConfirmButtonOnClick;
    }

    private void RuleConfirmButtonOnClick(object? sender, RoutedEventArgs e)
    {

        var Tags = _ruleTagsTextBox.Text;
        var Name = _ruleNameTextBox.Text;
        var Description = _ruleDescriptionTextBox.Text;
        
        if (string.IsNullOrWhiteSpace(Name))
        {
            // Show an error message or handle the case where the name is empty
            Console.WriteLine("Rule name cannot be empty.");
            return;
        }
        if (string.IsNullOrWhiteSpace(Tags))
        {
            // Show an error message or handle the case where the tags are empty
            Console.WriteLine("Rule tags cannot be empty.");
            return;
        }
        
        if (_selectedRule == null)
        {
            // If no rule is selected, we create a new one
            _selectedRule = new AutotileRule()
            {
                Name = Name,
                Tags = Tags.Split(',').Select(t => t.Trim()).ToList(),
                Description = Description?? string.Empty,
                Type = _ruleTypeComboBox.SelectedIndex == 0 ? ERuleType.WHITELIST : ERuleType.BLACKLIST,
                Side = (ERulePos)_ruleSideComboBox.SelectedIndex
            };
            // Add to selected tiling
            _selectedAutotilingDefinition.AddRule(_selectedRule);
            // Add to the list box
            _leftBarRuleListBox.Items.Add(_selectedRule.Name);
        }
        else
        {
            // If a rule is selected, we update it
            // We get the old name first to compare it with the new name, and check if we need to update the list box
            var oldName = _selectedRule.Name;
            _selectedRule.Name = Name;
            
            if(!string.IsNullOrWhiteSpace(oldName) && oldName != Name)
            {
                // If the name has changed, we update the list box
                var index = _leftBarRuleListBox.Items.IndexOf(oldName);
                if (index >= 0)
                {
                    _leftBarRuleListBox.Items[index] = Name;
                }
            }
            _selectedRule.Tags = Tags.Split(',').Select(t => t.Trim()).ToList();
            _selectedRule.Description = Description;
            _selectedRule.Type = _ruleTypeComboBox.SelectedIndex == 0 ? ERuleType.WHITELIST : ERuleType.BLACKLIST;
            _selectedRule.Side = (ERulePos)_ruleSideComboBox.SelectedIndex;
        }
        
        // Clear the rule editor panel
        _ruleEditorPanel.IsVisible = false;
        _previewMainCanvas.IsVisible = true;
    }
}
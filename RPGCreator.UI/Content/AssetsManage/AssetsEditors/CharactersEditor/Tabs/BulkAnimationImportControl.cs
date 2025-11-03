using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using RPGCreator.UI.Common;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class BulkAnimationImportControl : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    public Action<Dictionary<string, AnimationData>> OnImport;
    #endregion
    
    #region Properties
    
    // Need to separate those by the file name later if multiple images are allowed
    // Because right now, if multiple images are allowed, the map will be overridden, and row 1 on image 1 will conflict with row 1 on image 2
    public Dictionary<int, string> RowToAnimationNameMap { get; private set; } = new Dictionary<int, string>();
    
    private int _selectedRow = -1;
    
    #endregion
    
    #region Components
    
    private Grid Body { get; set; }
    private Grid LeftGrid { get; set; }
    
    private AnimationPreviewer Previewer { get; set; }
    
    private Grid TopPanel { get; set; }
    private PathPicker ImportButton { get; set; }
    private ComboBox ImageListBox { get; set; }
    private TextBox AssignedToInput { get; set; }
    
    private StackPanel SpritesheetPanel { get; set; }
    private MoveableCanvas SpritesheetCanvas { get; set; }
    private Image SpritesheetImage { get; set; }
    private Canvas SpritesheetOverlayCanvas { get; set; }
    private TextBlock SpritesheetText { get; set; }
    
    private DataGrid AnimationsDataGrid { get; set; }
    
    #endregion
    
    #region Constructors

    public BulkAnimationImportControl(Action<Dictionary<string, AnimationData>> onImport)
    {
        OnImport = onImport;
        CreateComponents();
        RegisterEvents();
        Content = Body;
    }
    
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, 0,auto"),
            RowDefinitions = new RowDefinitions("Auto, *")
        };

        LeftGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *, *"),
        };
        Body.Children.Add(LeftGrid);
        Grid.SetColumn(LeftGrid, 0);
        
        Previewer = new AnimationPreviewer()
        {
            FrameSize = new Size(42, 64),
        };
        Body.Children.Add(Previewer);
        Grid.SetColumn(Previewer, 2);
        
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
        
        AssignedToInput = new TextBox()
        {
            Watermark = "Assign to animation",
        };
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
    }


    private void RegisterEvents()
    {
        ImportButton.PropertyChanged += OnImportNewImage;
        SpritesheetCanvas.CanvasBody.PointerPressed += CanvasBodyOnPointerPressed;
        ImageListBox.SelectionChanged += OnImageSelectionChanged;
        AssignedToInput.TextChanged += (s, e) =>
        {
            if(_selectedRow >= 0)
            {
                RowToAnimationNameMap[_selectedRow] = AssignedToInput.Text;
            }
        };
    }

    private void OnImportNewImage(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != PathPicker.SelectedPathsProperty) return;
        var paths = e.NewValue as List<string>;
        if (paths is null || paths.Count == 0) return;
        
        var imagePath = paths[0];

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
            SpritesheetText.Text = $"Selected Row: {frameY}";
            
            // Clear previous overlays
            SpritesheetOverlayCanvas.Children.Clear();
            
            // Draw overlay rectangle on the selected row
            var overlayRect = new Rectangle()
            {
                Width = SpritesheetImage.Source?.Size.Width ?? 0,
                Height = 64,
                Fill = new SolidColorBrush(Color.FromArgb(50, 2, 155, 000)),
            };
            Canvas.SetLeft(overlayRect, 0);
            Canvas.SetTop(overlayRect, frameY * 64);
            SpritesheetOverlayCanvas.Children.Add(overlayRect);
            _selectedRow = frameY;
            
            if(RowToAnimationNameMap.ContainsKey(_selectedRow))
            {
                AssignedToInput.Text = RowToAnimationNameMap[_selectedRow];
            }
            else
            {
                AssignedToInput.Text = "";
            }
        }
    }
    
    private void OnImageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ImageListBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var imagePath = selectedItem.Tag as string;
            if (imagePath != null)
            {
                SpritesheetImage.Source = new Bitmap(imagePath);
                // Clear previous overlays and text
                SpritesheetOverlayCanvas.Children.Clear();
                SpritesheetText.Text = "Click to select animation row";
            }
        }
    }

    #endregion

    #region Events Handlers
    #endregion
}
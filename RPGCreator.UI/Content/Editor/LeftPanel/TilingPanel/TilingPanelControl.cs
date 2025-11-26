using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Runtimes.Context;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Editor.Context;
using Serilog;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;


public class SetOptionItem : UserControl
{
    public Ulid AssetId { get; set; }
    public string Name { get; set; }

    #region Components
    
    private Grid? _body;
    private Image? _previewImage;
    private TextBlock? _nameText;
    
    #endregion
    
    public SetOptionItem(ITilesetDef definition)
    {
        AssetId = definition.Unique;
        Name = definition.Name;
        CreateComponents();
        if (_previewImage != null) _previewImage.Source = definition.GetBitmap();
        Content = _body;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanelTilesetItem, this);
    }
    
    private void CreateComponents()
    {
        _body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("32, 10, *"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        _previewImage = new Image
        {
            Width = 32,
            Height = 32,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        _body.Children.Add(_previewImage);
        Grid.SetColumn(_previewImage, 0);
        
        _nameText = new TextBlock
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Text = Name
        };
        _body.Children.Add(_nameText);
        Grid.SetColumn(_nameText, 2);
    }
}

public class TilingPanelControl : UserControl
{
    
    private MapEditorContext _context;

    private AssetScope _scope;
    
    #region Components
    
    private StackPanel? _body;
    private Divider? _topDivider;
    private ComboBox? _setSelector;
    private Expander? _tileOptionsExpander;
    private StackPanel? _tileOptionsBody;
    private Image? _previewImage;
    
    #endregion
    
    public TilingPanelControl(MapEditorContext ctx)
    {
        _scope = EngineCore.Instance.Managers.Assets.CreateAssetScope("TilingPanelControl");
        _context = ctx;
        CreateComponents();
        RegisterEvents();
        Content = _body;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanel, this, _context);
    }
    
    private void CreateComponents()
    {
        _body = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };
        
        _topDivider = new Divider
        {
            Orientation =  Avalonia.Layout.Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            Content = "Tiling Options"
        };
        _body.Children.Add(_topDivider);
        
        _setSelector = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 5),
            PlaceholderText = "Select Tileset..."
        };
        _body.Children.Add(_setSelector);
        
        _tileOptionsExpander = new Expander
        {
            Header = "Tile Options",
            IsExpanded = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        _body.Children.Add(_tileOptionsExpander);
        _tileOptionsBody = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        _tileOptionsExpander.Content = _tileOptionsBody;
        
        _previewImage = new Image
        {
            Width = 128,
            Height = 128,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
        };
        _body.Children.Add(_previewImage);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Log.Debug("[TilingPanel] Loading tileset options...");
        var searchResults = EngineCore.Instance.Managers.Assets.SearchAllPacks<ITilesetDef>();
        foreach (var result in searchResults)
        {
            var def = _scope.Load<ITilesetDef>(result.AssetId);
            AddTilesetOption(def);
            Log.Debug("[TilingPanel] Added tileset option from search: {0}", def.Name);
        }
        base.OnLoaded(e);
    }

    private void RegisterEvents()
    {
        _setSelector.SelectionChanged += SetSelectorOnSelectionChanged;
    }


    public void AddTilesetOption(ITilesetDef definition)
    {
        _setSelector?.Items.Add(new SetOptionItem(definition));
    }
    
    #region EventsHandler
    private void SetSelectorOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        if (_setSelector?.SelectedItem is SetOptionItem selectedItem)
        {
            Log.Debug("[TilingPanel] Selected tileset: {0}", selectedItem.Name);
            var def = _scope.Load<ITilesetDef>(selectedItem.AssetId);
            if (_previewImage != null)
                _previewImage.Source = def.GetBitmap();
        }
        
    }
    #endregion
}
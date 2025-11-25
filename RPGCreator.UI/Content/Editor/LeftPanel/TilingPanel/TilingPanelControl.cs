using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.ModuleSDK.UIModule;
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
        Height = 200;
        Background = Brushes.AliceBlue;
        CreateComponents();
        if (_previewImage != null) _previewImage.Source = definition.GetBitmap();
        Content = _body;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanelTilesetItem, this);
    }

    private void CreateComponents()
    {
        _body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("64, 10, *"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5)
        };
        _previewImage = new Image
        {
            Width = 64,
            Height = 64,
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
    
    private InEditorContext _context;
    
    #region Components
    
    private StackPanel? _body;
    private Divider? _divider;
    private ComboBox? _setSelector;
    
    #endregion
    
    public TilingPanelControl(InEditorContext ctx)
    {
        _context = ctx;
        CreateComponents();
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
        
        _divider = new Divider
        {
            Orientation =  Avalonia.Layout.Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            Content = "Tiling Options"
        };
        _body.Children.Add(_divider);
        
        _setSelector = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 5),
            PlaceholderText = "Select Tileset..."
        };
        _body.Children.Add(_setSelector);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Log.Debug("[TilingPanel] Loading tileset options...");
        var searchResults = EngineCore.Instance.Managers.Assets.SearchAllPacks<ITilesetDef>();
        foreach (var result in searchResults)
        {
            if(EngineCore.Instance.Managers.Assets.TryResolveAsset(result.AssetId, out ITilesetDef? definition))
            {
                AddTilesetOption(definition);
                Log.Debug("[TilingPanel] Added tileset option from search: {0}", definition.Name);
            }
        }
        base.OnLoaded(e);
    }

    private void RegisterEvents()
    {
    }

    public void AddTilesetOption(ITilesetDef definition)
    {
        _setSelector?.Items.Add(new SetOptionItem(definition));
    }
}
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.UI.Common.CharacterCommonComponents;
using RPGCreator.UI.Common.TilesetsCommonComponents;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;
using RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;
using RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;
using RPGCreator.UI.Content.Editor.Tabs;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class EditorLeftPanelControl : UserControl
{
    #region Components

    private Grid? _contentBody;
    private ScrollViewer? _scrollViewer;
    private Grid? _scrollBody;
    private TabControl? _tabControl;

    private ToolSettingsControl _toolSettings;
    private TabItem _noneItem;

    private Control _payloadSpace;

    private static Dictionary<string, Control> _components = new();

    private TilesetExplorer? _tilesetPayload;
    private CharacterExplorer? _characterSelectorPayload;
    
    #endregion
    
    public EditorLeftPanelControl()
    {
        CreateComponents();
        Content = _contentBody;
        RegisterEvents();
        LinkToExtension();
    }
    
    private void CreateComponents()
    {
        _contentBody = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto, *, Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Width = 350,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        
        _scrollBody = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
            RowSpacing = 4,
            ColumnDefinitions = new ColumnDefinitions("*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinWidth = 300,
        };
        _contentBody.Children.Add(_scrollBody);
        Grid.SetRow(_scrollBody, 1);
        
        _tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        _scrollBody.Children.Add(_tabControl);

        _noneItem = new TabItem()
        {
            Header = "None",
            IsVisible = false,
        };
        
        _tabControl.Items.Add(_noneItem);
        
        _tabControl.Items.Add(new TabItem()
        {
            Content = new MapLevelTab(),
            Header = "Maps",
            IsVisible = true,
        });
        
        _tabControl.Items.Add(new TabItem()
        {
            Content = new LayerEditor(),
            Header = "Layers",
            IsVisible = true
        });
        
        // Add basics components
        // AddComponent("tiling", new TilingPanelControl());
        // AddComponent("entities", new EntitiesPanelControl());

        _payloadSpace = new TextBlock()
        {
            Text = "Select a tool to see its selector here.",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
        };
        
        _scrollBody.Children.Add(_payloadSpace);
        Grid.SetRow(_payloadSpace, 1);
        
        _toolSettings = new ToolSettingsControl();
        _contentBody.Children.Add(_toolSettings);
        Grid.SetRow(_toolSettings, 2);
        
    }
    
    private void RegisterEvents()
    {
        _tabControl.SelectionChanged += (s, e) =>
        {
            if((e.RemovedItems.Count > 0))
                if (e.RemovedItems[0] is TabItem item)
                {
                    item.PointerPressed -= HideOnClick;
                }

            if (e.AddedItems.Count <= 0) return;

            if (e.AddedItems[0] is not TabItem newItem) return;
            
            newItem.PointerPressed += HideOnClick;
            newItem.Cursor = new Cursor(StandardCursorType.Hand);
            _scrollBody.RowDefinitions = new RowDefinitions("*, *");
        };

        GlobalStates.ToolState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IToolState.ActiveTool))
            {
                var activeTool = GlobalStates.ToolState.ActiveTool;
                if (activeTool != null)
                {
                    if (activeTool.PayloadType != EPayloadType.Custom)
                    {

                        if (_tilesetPayload == null)
                        {
                            _tilesetPayload = new TilesetExplorer(
                                EngineServices.AssetsManager.CreateAssetScope("editor_left_panel"), 
                                tilesetType: TilesetExplorer.TilesetType.NonAutotileOnly)
                            {
                                Margin = new Thickness(4, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                            };
                            _tilesetPayload.TileSelected += def =>
                            {
                                GlobalStates.ToolState.Payload = def;
                            };
                        }
                        
                        if(_characterSelectorPayload == null)
                        {
                            _characterSelectorPayload = new CharacterExplorer(
                                EngineServices.AssetsManager.CreateAssetScope("editor_left_panel"))
                            {
                                Margin = new Thickness(4, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                            };
                            _characterSelectorPayload.CharacterSelected += def =>
                            {
                                GlobalStates.ToolState.Payload = def;
                            };
                        }
                        
                        switch (activeTool.PayloadType)
                        {
                            case EPayloadType.SimpleTile:
                                _tilesetPayload.SetTilesetType(TilesetExplorer.TilesetType.NonAutotileOnly);
                                ShowPayloadControl(_tilesetPayload);
                                break;
                            case EPayloadType.AutoTile:
                                _tilesetPayload.SetTilesetType(TilesetExplorer.TilesetType.AutotileOnly);
                                ShowPayloadControl(_tilesetPayload);
                                break;
                            case EPayloadType.AllTiles:
                                _tilesetPayload.SetTilesetType(TilesetExplorer.TilesetType.All);
                                ShowPayloadControl(_tilesetPayload);
                                break;
                            case EPayloadType.Character:
                                ShowPayloadControl(_characterSelectorPayload);
                                break;
                            default:
                                ShowPayloadControl(new TextBlock()
                                {
                                    Text = "Sorry, this payload type is not supported yet.",
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    TextAlignment = TextAlignment.Center,
                                });
                                break;
                        };

                        if (activeTool.PayloadType == EPayloadType.Custom)
                        {
                            var customControlObj = activeTool.GetCustomPayloadUiControl();
                            if (customControlObj is Control customControl)
                            {
                                ShowPayloadControl(customControl);
                            }
                            else
                                ShowPayloadControl(new TextBlock()
                                {
                                    Text = "Sorry, the custom payload control given by the current selected tool is not a valid Avalonia control.",
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    TextAlignment = TextAlignment.Center,
                                });
                        }
                    }
                }
                else
                {
                    ShowPayloadControl(new TextBlock()
                    {
                        Text = "Select a tool to see its selector here.",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                    });
                }
            }
        };
    }
    
    private void ShowPayloadControl(Control control)
    {
        _payloadSpace.IsVisible = true;
        if (_payloadSpace != control)
        {
            _scrollBody.Children.Remove(_payloadSpace);
            _payloadSpace = control;
            _scrollBody.Children.Add(_payloadSpace);
            Grid.SetRow(_payloadSpace, 1);
        }
    }

    private void HideOnClick(object? sender, PointerPressedEventArgs e)
    {
        if (_tabControl.Items[0] is TabItem item)
        {
            Dispatcher.UIThread.Post(() =>
            {
                item.IsSelected = true;
                _scrollBody.RowDefinitions = new RowDefinitions("Auto, *");
            }
            );
        }
    }

    private void LinkToExtension()
    {
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanel, this);
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelComponents, _scrollBody, null);
    }
}
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;
using RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;
using RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;
using RPGCreator.UI.Content.Editor.Tabs;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class EditorLeftPanelControl : UserControl
{
    
    #region Components

    private TabControl? _tabControl;
    private ScrollViewer? _scrollViewer;
    private Grid? _body;

    private static Dictionary<string, Control> _components = new();
    
    #endregion
    
    public EditorLeftPanelControl()
    {
        CreateComponents();
        RegisterEvents();
        Content = _tabControl;
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanel, this);
    }
    
    private void CreateComponents()
    {
        _tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        
        _scrollViewer = new ScrollViewer
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        _tabControl.Items.Add(new TabItem()
        {
            Content = _scrollViewer,
            Header = "Tool Properties"
        });
        _body = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10),
            MinWidth = 300,
        };
        _scrollViewer.Content = _body;

        _tabControl.Items.Add(new TabItem()
        {
            Content = new MapEditor(),
            Header = "Map Editor"
        });
        _tabControl.Items.Add(new TabItem()
        {
            Content = new MapLevelTab(),
            Header = "Map Levels"
        });
        
        
        // Add basics components
        AddComponent("none", new NonePanelControl());
        AddComponent("tiling", new TilingPanelControl());
        AddComponent("entities", new EntitiesPanelControl());
        
        // Show default component
        RuntimeServices.OnceServiceReady((ILayerService ls) =>
        {
            ls.OnLayerSelected += (int layerIndex) =>
            {
                var selected = RuntimeServices.LayerService.GetSelectedLayer();

                if (selected is TileLayerDefinition or AutoLayerDefinition)
                    ShowComponent("tiling");
                else if (selected is EntityLayerDefinition)
                    ShowComponent("entities");
                else
                    ShowComponent("none");
            };
        });
        ShowComponent("tiling");
        
        
        var config = new EditorLeftPanelComponentsContext.Config
        {
            AddComponent = AddComponent,
            RemoveComponent = RemoveComponent,
            ShowComponent = ShowComponent,
            HideComponent = HideComponent
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelComponents, _body, new EditorLeftPanelComponentsContext(config));
    }
    
    [ExposeToPlugin("EditorLeftPanel.Components")]
    public void AddComponent(string key, Control component)
    {
        if (_components.ContainsKey(key))
            return;
        _components.Add(key, component);
    }
    
    [ExposeToPlugin("EditorLeftPanel.Components")]
    public void RemoveComponent(string key)
    {
        if (!_components.ContainsKey(key))
            return;
        _components.Remove(key);
    }
    
    [ExposeToPlugin("EditorLeftPanel.Components")]
    public void ShowComponent(string key)
    {
        if (!_components.ContainsKey(key))
            return;
        var component = _components[key];
        if (!_body!.Children.Contains(component))
        {
            HideComponent();
            _body.Children.Add(component);
        }
    }

    [ExposeToPlugin("EditorLeftPanel.Components")]
    public void HideComponent()
    {
        _body.Children.Clear();
    }

    private void RegisterEvents()
    {
        
    }
    
}
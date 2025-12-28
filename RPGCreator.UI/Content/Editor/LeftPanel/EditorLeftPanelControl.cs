using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using RPGCreator.Core.Contexts;
using RPGCreator.Core.ModuleSDK.Attributes;
using RPGCreator.Core.Runtimes.Context;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;
using RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;
using RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;
using RPGCreator.UI.Content.Editor.Tabs;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class EditorLeftPanelControl : UserControl
{
    
    #region Fields

    private MapEditorContext _context;
    
    #endregion
    
    #region Components

    private TabControl? _tabControl;
    private ScrollViewer? _scrollViewer;
    private StackPanel? _body;

    private static Dictionary<string, Control> _components = new();
    
    #endregion
    
    public EditorLeftPanelControl(MapEditorContext ctx)
    {
        _context = ctx;
        CreateComponents();
        RegisterEvents();
        Content = _tabControl;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanel, this, _context);
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
        _body = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10),
            MinWidth = 300,
        };
        _scrollViewer.Content = _body;

        _tabControl.Items.Add(new TabItem()
        {
            Content = new MapEditor(_context),
            Header = "Map Editor"
        });
        _tabControl.Items.Add(new TabItem()
        {
            Content = new MapLevelTab(_context),
            Header = "Map Levels"
        });
        
        
        // Add basics components
        AddComponent("none", new NonePanelControl(_context));
        AddComponent("tiling", new TilingPanelControl(_context));
        AddComponent("entities", new EntitiesPanelControl(_context));
        
        // Show default component
        ShowComponent("tiling");
        
        
        var config = new EditorLeftPanelComponentsContext.Config
        {
            AddComponent = AddComponent,
            RemoveComponent = RemoveComponent,
            ShowComponent = ShowComponent,
            HideComponent = HideComponent
        };
        
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelComponents, _body, new EditorLeftPanelComponentsContext(config));
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
            _body.Children.Add(component);
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
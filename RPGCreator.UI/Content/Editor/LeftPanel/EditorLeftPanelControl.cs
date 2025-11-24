using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Contexts;
using RPGCreator.Core.ModuleSDK.Attributes;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;
using RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;
using RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class EditorLeftPanelControl : UserControl
{
    
    #region Fields

    private InEditorContext _context;
    
    #endregion
    
    #region Components

    private StackPanel? _body;

    private static Dictionary<string, Control> _components = new();
    
    #endregion
    
    public EditorLeftPanelControl(InEditorContext ctx)
    {
        _context = ctx;
        CreateComponents();
        RegisterEvents();
        Content = _body;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanel, this, _context);
    }
    
    private void CreateComponents()
    {
        _body = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10),
            MinWidth = 300,
        };
        
        // Add basics components
        AddComponent("none", new NonePanelControl(_context));
        AddComponent("tiling", new TilingPanelControl(_context));
        AddComponent("entities", new EntitiesPanelControl(_context));
        
        // Show default component
        ShowComponent("none");
        
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelComponents, _body, new EditorLeftPanelComponentsContext(
            AddComponent,
            RemoveComponent,
            ShowComponent,
            HideComponent
        ));
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
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.ModuleSDK.UIModule;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class EditorLeftPanelControl : UserControl
{
    
    #region Components

    private StackPanel? _body;

    private static Dictionary<string, Control> _components = new();
    
    #endregion
    
    public EditorLeftPanelControl()
    {
        CreateComponents();
        RegisterEvents();
        Content = _body;
    }
    
    private void CreateComponents()
    {
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanel, this, _components);
        _body = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10)
        };
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelBody, _body, _components);
    }

    private void RegisterEvents()
    {
        
    }
    
}
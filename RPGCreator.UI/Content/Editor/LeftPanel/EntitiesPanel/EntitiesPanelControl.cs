using Avalonia.Controls;
using RPGCreator.SDK.Modules.UIModule;

namespace RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;

public class EntitiesPanelControl : UserControl
{
    
    public EntitiesPanelControl()
    {
        CreateComponents();
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelEntitiesPanel, this);
    }
    
    private void CreateComponents()
    {
        
    }
}
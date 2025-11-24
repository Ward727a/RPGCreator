using Avalonia.Controls;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;

public class EntitiesPanelControl : UserControl
{
    private InEditorContext _context;
    
    public EntitiesPanelControl(InEditorContext ctx)
    {
        _context = ctx;
        CreateComponents();
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelEntitiesPanel, this, _context);
    }
    
    private void CreateComponents()
    {
        
    }
}
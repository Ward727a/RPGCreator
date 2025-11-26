using Avalonia.Controls;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Runtimes.Context;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;

public class EntitiesPanelControl : UserControl
{
    private MapEditorContext _context;
    
    public EntitiesPanelControl(MapEditorContext ctx)
    {
        _context = ctx;
        CreateComponents();
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelEntitiesPanel, this, _context);
    }
    
    private void CreateComponents()
    {
        
    }
}
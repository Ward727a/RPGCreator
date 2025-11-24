using Avalonia.Controls;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;

public class TilingPanelControl : UserControl
{
    
    private InEditorContext _context;
    
    public TilingPanelControl(InEditorContext ctx)
    {
        _context = ctx;
        CreateComponents();
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanel, this, _context);
    }
    
    private void CreateComponents()
    {
        
    }
}
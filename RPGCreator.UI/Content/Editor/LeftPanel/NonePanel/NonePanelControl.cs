using Avalonia.Controls;
using RPGCreator.Core.Runtimes.Context;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.SDK.Modules.UIModule;

namespace RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;

public class NonePanelControl : UserControl
{

    private MapEditorContext _context;
    private TextBlock textBlock;
    
    public NonePanelControl(MapEditorContext context)
    {
        _context = context;
        CreateComponents();
        Content = textBlock;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelNonePanel, this, _context);
    }

    private void CreateComponents()
    {
        textBlock = new TextBlock
        {
            Text = "No panel selected.",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }
    
}
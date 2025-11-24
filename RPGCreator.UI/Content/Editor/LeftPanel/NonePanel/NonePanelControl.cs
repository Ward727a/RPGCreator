using Avalonia.Controls;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;

public class NonePanelControl : UserControl
{

    private InEditorContext _context;
    private TextBlock textBlock;
    
    public NonePanelControl(InEditorContext context)
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
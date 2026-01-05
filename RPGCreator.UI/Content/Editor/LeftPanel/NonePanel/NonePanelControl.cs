using Avalonia.Controls;
using RPGCreator.SDK.Modules.UIModule;

namespace RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;

public class NonePanelControl : UserControl
{

    private TextBlock textBlock;
    
    public NonePanelControl()
    {
        CreateComponents();
        Content = textBlock;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelNonePanel, this);
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
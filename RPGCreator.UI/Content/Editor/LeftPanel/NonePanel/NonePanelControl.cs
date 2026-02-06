using Avalonia.Controls;
using RPGCreator.SDK;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.UI.UiService;

namespace RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;

public class NonePanelControl : UserControl
{

    private TextBlock textBlock;
    
    public NonePanelControl()
    {
        CreateComponents();
        Content = textBlock;
        UiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelNonePanel, this);
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
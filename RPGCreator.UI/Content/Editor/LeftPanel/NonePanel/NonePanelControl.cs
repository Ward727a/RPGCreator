
using Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using Dock.Settings;
using RPGCreator.SDK;
using RPGCreator.SDK.Modules.UIModule;

namespace RPGCreator.UI.Content.Editor.LeftPanel.NonePanel;

public class NonePanelControl : UserControl
{

    // All commented code here need to move for the GameViewport panel.
    // This is just a demo and test of how the DockControl works.
    
    private TextBlock textBlock;

    // private DockControl _dockControl;
    // private Factory _dockFactory;
    
    public NonePanelControl()
    {
        // _dockControl = new DockControl();
        // _dockFactory = new Factory();
        CreateComponents();
        Content = textBlock;
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelNonePanel, this);
    }

    private void CreateComponents()
    {
        //
        // var documentDock = new DocumentDock
        // {
        //     Id = "Documents",
        //     IsCollapsable = false,
        //     CanCreateDocument = false,
        //     CanClose = false,
        //     CloseButtonShowMode = DocumentCloseButtonShowMode.Never,
        //     CanCloseLastDockable = false,
        // };
        // documentDock.DocumentFactory = () =>
        // {
        //     var index = documentDock.VisibleDockables?.Count ?? 0;
        //     return new Document
        //     {
        //         Id = $"Doc{index + 1}",
        //         Title = $"Document {index + 1}",
        //         Content = new TextBox { Text = $"Document {index + 1}", AcceptsReturn = true }
        //     };
        // };
        // DockSettings.UseOwnerForFloatingWindows = true;
        // var document = new Document()
        // {
        //     Id = "Doc1",
        //     Title = "Document 1",
        //     CanClose = false,
        //     CanFloat = false
        // };
        //
        // documentDock.VisibleDockables = _dockFactory.CreateList<IDockable>(document);
        // documentDock.ActiveDockable = document;
        // documentDock.EmptyContent = "No Tool selected.";
        //
        // var leftTool = new Tool { Id = "Tool1", Title = "Tool 1" };
        // var bottomTool = new Tool { Id = "Tool2", Title = "Output" };
        //
        // var mainLayout = new ProportionalDock
        // {
        //     Orientation = Orientation.Horizontal,
        //     VisibleDockables = _dockFactory.CreateList<IDockable>(
        //         documentDock),
        // };
        //
        //
        // var root = _dockFactory.CreateRootDock();
        // root.VisibleDockables = _dockFactory.CreateList<IDockable>(mainLayout);
        // root.DefaultDockable = mainLayout;
        //
        // _dockFactory.InitLayout(root);
        // _dockControl.Factory = _dockFactory;
        // _dockControl.Layout = root;
        //
        // this.Content = _dockControl;
        
        textBlock = new TextBlock
        {
            Text = "No panel selected.",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }
    
}
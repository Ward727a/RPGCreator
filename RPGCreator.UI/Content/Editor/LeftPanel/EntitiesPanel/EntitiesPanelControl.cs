using System.ComponentModel;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.UI.Common.Modal.Browser;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Editor.LeftPanel.EntitiesPanel;

public partial class EntitiesPanelControl : UserControl
{

    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<EntitiesPanelControl>();


    public IEntityDefinition? SelectedEntityDefinition;
    
    public Grid BodyGrid { get; private set; }
    public ScrollViewer BodyScroll { get; private set; }
    public StackPanel LayersBody { get; private set; }
    
    public Button ChangeEntityButton { get; private set; }
    public TextBlock SelectedEntityText { get; private set; }

    public StackPanel MainBody;
    
    public EntitiesPanelControl()
    {
        CreateComponents();
        RegisterEvents();
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelEntitiesPanel, this, MainBody);
    }
    
    private void CreateComponents()
    {
        BodyGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        
        BodyScroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        
        LayersBody = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };
        
        BodyScroll.Content = LayersBody;
        BodyGrid.Children.Add(BodyScroll);
        
        ChangeEntityButton = new Button
        {
            Content = "Change Entity",
            Margin = App.style.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        LayersBody.Children.Add(ChangeEntityButton);
        
        SelectedEntityText = new TextBlock
        {
            Inlines = new InlineCollection(),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            TextAlignment = Avalonia.Media.TextAlignment.Center
        };
        UpdateSelectedEntityText();
        
        LayersBody.Children.Add(SelectedEntityText);
        
        LayersBody.Children.Add(new Divider());
        
        MainBody = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };
        LayersBody.Children.Add(MainBody);
        
        Content = BodyGrid;
    }
    
    private void UpdateSelectedEntityText()
    {
        if(SelectedEntityText.Inlines == null)
            return;
        SelectedEntityText.Inlines.Clear();
        SelectedEntityText.Inlines.AddRange([new Run()
        {
            Foreground = Brushes.Gray,
            FontWeight = FontWeight.Bold,
            Text = "Selected Entity: "
        }, new Run()
        {
            Text = SelectedEntityDefinition != null ? SelectedEntityDefinition.Name : "None"
        }]);
    }

    private void RegisterEvents()
    {
        ChangeEntityButton.Click += OnChangeEntityButtonClicked;
    }

    private async void OnChangeEntityButtonClicked(object? sender, RoutedEventArgs e)
    {
        var entityBrowser = new EntitiesBrowser();

        var result = await EditorUiServices.DialogService.ConfirmAsync("Select Entity", entityBrowser,
            new DialogStyle((128 + 50) * 3, (128 + 50) * 3 + 100, SizeToContent: DialogSizeToContent.WidthOnly));

        if (result && entityBrowser.SelectedEntityDefinition != null)
        {
            SelectedEntityDefinition = entityBrowser.SelectedEntityDefinition;
            GlobalStates.BrushState.CurrentMode = BrushMode.Entities;
            GlobalStates.BrushState.CurrentObjectToPaint = new EntitySpawner(SelectedEntityDefinition, Vector2.Zero);
            Logger.Debug("Entity selected! {entityName}", args: SelectedEntityDefinition.Name);
            UpdateSelectedEntityText();
        }
    }

    public event PropertyChangingEventHandler? PropertyChanging;
    public new event PropertyChangedEventHandler? PropertyChanged;
}
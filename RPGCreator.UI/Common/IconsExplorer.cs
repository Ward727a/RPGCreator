using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;
using RPGCreator.Core;
using RPGCreator.Core.Types.Editor;

namespace RPGCreator.UI.Common;

public class IconsExplorerItem : UserControl
{
    public IconMeta IconMeta { get; set; }

    public string IconName => IconMeta.Name;


    public Grid Root;
    public StackPanel HoverOverlay;
    public Icon IconControl { get; set; }
    public TextBlock IconNameTextBlock { get; set; }
    public TextBlock IconAuthorTextBlock { get; set; }
    
    public IconsExplorerItem(IconMeta meta)
    {
        IconMeta = meta;
        CreateComponents();
        
        PointerEntered += (s, e) =>
        {
            HoverOverlay.Opacity = 0.8;
        };
        PointerExited += (s, e) =>
        {
            HoverOverlay.Opacity = 0.0;
        };
        Background = Brushes.Transparent;
    }

    private void CreateComponents()
    {
        Root = new Grid();
        IconControl = new Icon
        {
            Value = IconMeta.IconValue,
            Width = 64,
            Height = 64,
            FontSize = 24,
        };
        HoverOverlay = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Background = null,
            Opacity = 0.0,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Root.Children.Add(IconControl);
        Root.Children.Add(HoverOverlay);
        
        IconNameTextBlock = new TextBlock
        {
            Text = IconMeta.Name,
            FontSize = 12,
            Foreground = Avalonia.Media.Brushes.White,
            Margin = new Thickness(4, 2, 4, 0),
            TextWrapping = TextWrapping.Wrap,
        };
        IconAuthorTextBlock = new TextBlock
        {
            Text = $"by {IconMeta.Author}",
            FontSize = 10,
            Foreground = Avalonia.Media.Brushes.LightGray,
            Margin = new Thickness(4, 0, 4, 4),
            TextWrapping = TextWrapping.Wrap,
        };
        HoverOverlay.Children.Add(IconNameTextBlock);
        HoverOverlay.Children.Add(IconAuthorTextBlock);
        
        Content = Root;
    }
}

public class IconsExplorer : Window
{
    
    private Expander _licenseExpander;
    private TextBox _searchBox;
    private WrapPanel _iconsPanel;
    
    public IconsExplorer()
    {
        CreateComponents();
        RegisterEvents();
        Width = 400;
        Height = 300;
        Title = "RPG Creator - Icons Explorer";
    }

    private void CreateComponents()
    {
        var mainPanel = new DockPanel();

        #if DEBUG
        _searchBox = new TextBox
        {
            Watermark = "Search icons...",
            Margin = new Thickness(5),
        };
        DockPanel.SetDock(_searchBox, Dock.Top);
        mainPanel.Children.Add(_searchBox);

        _licenseExpander = new Expander
        {
            Header = "Icon License Information",
            IsExpanded = false,
            Margin = new Thickness(5),
            Content = new TextBlock
            {
                Text = EngineCore.Instance.Icons.IconsLicense,
                TextWrapping = TextWrapping.Wrap,
            }
        };
        DockPanel.SetDock(_licenseExpander, Dock.Bottom);
        mainPanel.Children.Add(_licenseExpander);

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Margin = new Thickness(5),
        };

        _iconsPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            ItemWidth = 64,
            ItemHeight = 64,
        };
        scrollViewer.Content = _iconsPanel;

        mainPanel.Children.Add(scrollViewer);
        #else
        var _error = new TextBlock
        {
            Text = "Icons Explorer is only available in Debug builds.",
            Foreground = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        mainPanel.Children.Add(_error);
        #endif
        Content = mainPanel;
    }

    private void RegisterEvents()
    {
    }

    private void LoadIcons()
    {
        #if DEBUG
        _iconsPanel.Children.Clear();
        var icons = EngineCore.Instance.Icons;
        foreach (var iconMeta in icons.IconsMeta)
        {
            var iconItem = new IconsExplorerItem(iconMeta);
            _iconsPanel.Children.Add(iconItem);
        }
        #endif
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        LoadIcons();
        
    }
}
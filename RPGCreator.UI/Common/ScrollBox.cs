using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using ScrollViewer = Avalonia.Controls.ScrollViewer;
using UserControl = Avalonia.Controls.UserControl;

namespace RPGCreator.Core.Types;

public class ScrollBox : UserControl
{
    public new Control? Content
    {
        get => scroller.Content as Control;
        set => scroller.Content = value;
    }
    private ScrollViewer scroller;
    private DockPanel BackgroundGrid;

    public ScrollBox()
    {
        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };

        scroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        root.Children.Add(scroller);
        base.Content = root;
    }
}
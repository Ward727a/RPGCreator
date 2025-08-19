using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using RPGCreator.UI;
using ScrollViewer = Avalonia.Controls.ScrollViewer;
using UserControl = Avalonia.Controls.UserControl;

namespace RPGCreator.Core.Type;

public class ScrollBox : UserControl
{
    public new Control? Content
    {
        get => scroller.Content as Control;
        set => scroller.Content = value;
    }
    private ScrollViewer scroller;
    private Border BackgroundGrid;

    public ScrollBox()
    {
        BackgroundGrid = new Border
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        
        scroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        
        BackgroundGrid.Child = (scroller);
        base.Content = BackgroundGrid;
    }
}
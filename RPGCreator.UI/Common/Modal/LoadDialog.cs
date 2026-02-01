using Avalonia.Controls;
using RPGCreator.UI;

namespace RPGCreator.Core.Types.Windows;

public class LoadDialog : Window
{
    private StackPanel PanelContent;
    private ProgressBar LoadingBar;

    public LoadDialog(
        string title = "Loading, please wait...",
        string message = "Loading, please wait...",
        bool showProgressText = false,
        bool isIndeterminate = true)
    {

        PanelContent = new StackPanel()
        {
            Margin = App.style.Margin
        };
        
        Title = title;
        Content = PanelContent;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var messageBlock = new TextBlock
        {
            Text = message
        };
        PanelContent?.Children.Add(messageBlock);
        LoadingBar = new ProgressBar()
        {
            ShowProgressText = showProgressText,
            IsIndeterminate = isIndeterminate,
        };
        PanelContent?.Children.Add(LoadingBar);

    }
    
    public void SetProgress(double value)
    {
        LoadingBar.Value = value;
    }
}
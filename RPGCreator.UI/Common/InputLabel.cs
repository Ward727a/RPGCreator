using Avalonia.Controls;
using Avalonia.Layout;

namespace RPGCreator.Core.Types;

public class InputLabel : UserControl
{
    public Control InputControl;

    private TextBlock? label;
    
    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (label == null) return;
            label.Text = value;
        }
    }
    
    public InputLabel(string text, Control inputControl, string firstColumnSize = "Auto")
    {
        InputControl = inputControl;
        _text = text;
        Content = new StackPanel();
            
        var grid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions($"{firstColumnSize}, *"),
            RowDefinitions = new RowDefinitions("Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        (Content as StackPanel)?.Children.Add(grid);
        label = new TextBlock
        {
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 10, 0)
        };
        grid.Children.Add(label);
        Grid.SetColumn(label, 0);
        grid.Children.Add(InputControl);
        Grid.SetColumn(InputControl, 1);
        
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Center;
        
    }
}
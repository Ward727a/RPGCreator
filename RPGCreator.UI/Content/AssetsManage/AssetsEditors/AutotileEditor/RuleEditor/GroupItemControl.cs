using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.Tilesets;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor.RuleEditor;

public class GroupItemControl : UserControl
{
    public AutotileGroup Group { get; private set; }

    private StackPanel _body = null!;
    private TextBlock _nameTextBlock = null!;
    
    public GroupItemControl(AutotileGroup group)
    {
        Group = group;

        CreateComponents();
        this.Content = _body;
    }

    private void CreateComponents()
    {

        _body = new StackPanel()
        {
            Orientation = Orientation.Horizontal
        };
        
        _nameTextBlock = new TextBlock()
        {
            Text = Group.Name,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
        };
        _body.Children.Add(_nameTextBlock);

    }
    
}
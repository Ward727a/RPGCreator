using Avalonia.Controls;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor.Components;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor;

public class AutoLayerEditorControl : UserControl
{
    
    private IntRefListControl? _intRefListControl;

    private Grid? _body;
    
    public AutoLayerEditorControl()
    {
        CreateComponents();
        Content = _body;
    }
    
    private void CreateComponents()
    {

        _body = new Grid()
        {
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("auto"),
        };
        
        _intRefListControl = new IntRefListControl();
        _body.Children.Add(_intRefListControl);
    }
    
}
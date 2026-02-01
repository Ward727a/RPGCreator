using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor.Components;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor;

public class AutoLayerEditorControl : UserControl
{
    
    private IntRefListControl? _intRefListControl;
    private IntGridSetListControl? _intGridSetListControl;

    private Grid? _body;
    
    public AutoLayerEditorControl()
    {
        CreateComponents();
        Content = _body;
        RegisterEvents();
    }
    
    private void CreateComponents()
    {

        _body = new Grid()
        {
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("300, Auto, *, 5"),
        };
        
        _intRefListControl = new IntRefListControl();
        _body.Children.Add(_intRefListControl);
        _intRefListControl.MenuPanel.IsEnabled = false;

        var divider = new Divider()
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            // Set height to match the parent grid
            Height = double.NaN,
        };
        Grid.SetColumn(divider, 1);
        _body.Children.Add(divider);
        
        _intGridSetListControl = new IntGridSetListControl();
        Grid.SetColumn(_intGridSetListControl, 2);
        _body.Children.Add(_intGridSetListControl);
    }

    private void RegisterEvents()
    {
        _intGridSetListControl.OnTilesetSelected += OnTilesetSelected;
    }

    private void OnTilesetSelected(IntGridTilesetDef obj)
    {
        _intRefListControl.LoadIntRefsFromTileset(obj);
        _intRefListControl.MenuPanel.IsEnabled = true;
    }
}
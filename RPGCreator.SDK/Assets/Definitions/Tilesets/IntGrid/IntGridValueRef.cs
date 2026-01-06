using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

[SerializingType("IntGridValueRef")]
public partial class IntGridValueRef : ObservableObject
{
    [ObservableProperty]
    private int _value;

    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private Color _color;
    
    [ObservableProperty]
    private string _iconPath;
    
    [ObservableProperty]
    private TileData _defaultTileData;
}
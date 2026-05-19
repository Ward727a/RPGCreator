using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.Types.Records;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

[EngineType("rpgc", "sdk", "int_grid_value_ref")]
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
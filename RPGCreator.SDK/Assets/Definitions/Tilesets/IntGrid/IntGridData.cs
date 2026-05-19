using RPGCreator.SDK.Common.Attributes;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

[EngineType("rpgc", "sdk", "int_grid_data")]
public class IntGridData
{
    public IntGridValueRef IntGridRef { get; set; }
    public IntGridTilesetDef IntGridTilesetDef { get; set; }
}
using RPGCreator.SDK.Attributes;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

[SerializingType("IntGridData")]
public class IntGridData
{
    public IntGridValueRef IntGridRef { get; set; }
    public IntGridTilesetDef IntGridTilesetDef { get; set; }
}
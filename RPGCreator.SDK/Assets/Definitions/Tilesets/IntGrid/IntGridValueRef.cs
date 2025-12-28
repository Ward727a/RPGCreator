using System.Drawing;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.Core.Types.Map;

public partial class IntGridValueRef
{
    public int Value;

    public string Name;
    
    public Color Color;
    
    public string IconPath;
    
    public TileData DefaultTileData;
}
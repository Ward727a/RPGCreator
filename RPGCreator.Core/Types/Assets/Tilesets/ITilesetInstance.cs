using Avalonia.Media.Imaging;
using RPGCreator.Core.Types.Internal;
using SkiaSharp;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public interface ITilesetInstance 
{
    public ITilesetDef Definition { get; }
    
    public bool HasTile(int row, int column);
    public bool HasTile(Point rowColumn);
    public ITileDef? GetTileAt(int row, int column);
}
using Avalonia.Media.Imaging;
using RPGCreator.Core.Types.Internal;
using Serilog;
using SkiaSharp;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class AutoTilesetInstance : ITilesetInstance
{

    private AutoTilesetDef _def;
    public ITilesetDef Definition => _def;
    
    public AutoTilesetInstance()
    {
    }

    public AutoTilesetInstance(AutoTilesetDef tilesetDef)
    {
        _def = tilesetDef;
    }

    public bool HasTile(int row, int column)
    {
        // Check if the autotilings contain a tile at the specified row and column.
        return _def.AutotileGroups.Any(at => at.HasTile(new Point(row, column)));
    }
    public bool HasTile(Point rowColumn)
    {
        return HasTile(rowColumn.X, rowColumn.Y);
    }

    public ITileDef? GetTileAt(int row, int column)
    {
        if (!_def.AutotileGroups.Any(at => at.HasTile(new Point(row, column)))) return null;
        
        var autotiling = _def.AutotileGroups.First(at => at.HasTile(new Point(row, column)));
        return autotiling.GetTileAt(null, new Point(row, column)); // Pass null for the layer as we don't have a layer context here.
    }

    public List<ITilesetDef> GetUsedTilesets()
    {
        // Returns a list of tilesets used in this autotileset.
        return _def.AutotileGroups.Select(autotileGroupInstance => autotileGroupInstance.BaseTile.TilesetDef).Distinct().ToList();
    }
}
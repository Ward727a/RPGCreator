using System.Diagnostics.CodeAnalysis;
using Avalonia;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Types;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Common.Helpers;

public static class IntGridTilesetHelper
{
    
    public static ITilesetDef? GetIntRefTilesetDef(IntGridValueRef intRef, AssetScope scope)
    {
        var tilesetUlid = intRef.DefaultTileData.TilesetId;

        if (tilesetUlid == Ulid.Empty)
        {
            return null;
        }

        return scope.Load<ITilesetDef>(tilesetUlid);
    }
    
    private static bool TryGetIntRefTilesetDef(IntGridValueRef intRef, AssetScope scope, [NotNullWhen(true)]out ITilesetDef? tileset)
    {
        var tilesetUlid = intRef.DefaultTileData.TilesetId;

        if (tilesetUlid == Ulid.Empty)
        {
            tileset = null;
            return false;
        }

        tileset = scope.Load<ITilesetDef>(tilesetUlid);
        return tileset != null;
    }
    
    public static UnifiedCroppedImage? GetIntRefDefaultTileImage(IntGridValueRef intRef, AssetScope scope)
    {
        if (!TryGetIntRefTilesetDef(intRef, scope, out var tileset))
        {
            return null;
        }

        var tileX = (int)intRef.DefaultTileData.TilePosition.X;
        var tileY = (int)intRef.DefaultTileData.TilePosition.Y;
        var tileW = tileset.TileWidth;
        var tileH = tileset.TileHeight;

        var tilesetImage = new UnifiedImage(tileset.ImagePath);
        
        PixelRect rect = new PixelRect(
            tileX,
            tileY,
            tileW,
            tileH
        );
        
        return tilesetImage.GetCroppedImage(rect);
    }
    
    public static ITileDef? GetIntRefDefaultTileDef(IntGridValueRef intRef, AssetScope scope)
    {
        if (!TryGetIntRefTilesetDef(intRef, scope, out var tileset))
        {
            return null;
        }

        var tileX = (int)intRef.DefaultTileData.TilePosition.X / tileset.TileWidth;
        var tileY = (int)intRef.DefaultTileData.TilePosition.Y / tileset.TileHeight;

        return tileset.GetTileAt(tileX, tileY);
    }
}
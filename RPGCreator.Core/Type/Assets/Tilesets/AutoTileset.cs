using Avalonia.Media.Imaging;
using RPGCreator.Core.Type.Internal;
using SkiaSharp;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class AutoTileset : ImageAsset, ITileset, ISerializable, IDeserializable
{
    public const int MaxTilesByRow = 8; // Maximum number of tiles in a row for the tileset family
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public List<AutotileGroup> AutotileGroups { get; set; } = new(); // List of autotilings for this tileset

    public AutoTileset()
    {
        Type = TYPE.TILESETS; // Set the type of this asset to AutoTileset
    }
    
    public bool HasTile(int row, int column)
    {
        // Check if the autotilings contain a tile at the specified row and column.
        return AutotileGroups.Any(at => at.HasTile(new Point(row, column)));
    }
    public bool HasTile(Point rowColumn)
    {
        return HasTile(rowColumn.X, rowColumn.Y);
    }

    public ITileable? GetTileAt(int row, int column)
    {
        if (!AutotileGroups.Any(at => at.HasTile(new Point(row, column)))) return null;
        
        var autotiling = AutotileGroups.First(at => at.HasTile(new Point(row, column)));
        return autotiling.GetTileAt(null, new Point(row, column)); // Pass null for the layer as we don't have a layer context here.
    }

    public SKBitmap GetSKBitmap()
    {
        SKBitmap combinedBitmap = new SKBitmap(
            TileWidth * MaxTilesByRow, // Width of the combined bitmap
            TileHeight * (AutotileGroups.Count / MaxTilesByRow + 1) // Height based on the number of tiles
        );
        
        var maxX = TileWidth * MaxTilesByRow; // Assuming a maximum of 8 tiles horizontally
        var currentX = 0;
        var currentY = 0; // Current Y position in the combined bitmap
        foreach (var tilesPair in AutotileGroups)
        {
            var tile = tilesPair.BaseTile;
            
            var imagePath = tile.Tileset.ImagePath;
            
            if(!File.Exists(imagePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Tileset image at {imagePath} does not exist.");
                Console.ResetColor();
                continue; // Skip this tile if the image does not exist.
            }
            
            var bitmap = SKBitmap.Decode(imagePath);
            if (bitmap == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to decode bitmap from {imagePath}.");
                Console.ResetColor();
                continue; // Skip this tile if the bitmap could not be decoded.
            }
            
            // Here we will crop the bitmap to the tile size & position and add it to the combined bitmap.
            var tilePosX = tile.PositionInTileset.X * TileWidth;
            var tilePosY = tile.PositionInTileset.Y * TileHeight;
            var tileRect = new SKRectI(tilePosX, tilePosY, tilePosX + TileWidth, tilePosY + TileHeight);
            var tileNewPos = new SKRectI(currentX, currentY, currentX + TileWidth, currentY + TileHeight);

            using var canvas = new SKCanvas(combinedBitmap);
            
            // Draw the tile bitmap onto the combined bitmap at the correct position
            canvas.DrawBitmap(bitmap, tileRect, tileNewPos);
                
            currentX += TileWidth; // Move to the next tile position horizontally
            if (currentX >= maxX)
            {
                currentX = 0;
                currentY += TileHeight; // Move to the next row if we reach the maximum width
            }
        }

#if DEBUG
        // If in debug mode, save the combined bitmap to a file for inspection
        var debugFilePath = Path.Combine(Environment.CurrentDirectory, "combined_tileset_debug.png");
        using (var debugStream = File.OpenWrite(debugFilePath))
        {
            combinedBitmap.Encode(debugStream, SKEncodedImageFormat.Png, 100);
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Combined tileset bitmap saved to {debugFilePath}");
        Console.ResetColor();
#endif
        
        return combinedBitmap;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(AutoTileset));
        AddBaseSerialization(info);
        info.AddValue("TileWidth", TileWidth);
        info.AddValue("TileHeight", TileHeight);
        info.AddValue("AutotileGroups", AutotileGroups);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        LoadBaseSerialization(info);
        info.TryGetValue("TileWidth", out int tileWidth, 32, "Tile width not found or invalid (Set to 32 by default).");
        info.TryGetValue("TileHeight", out int tileHeight, 32, "Tile height not found or invalid (Set to 32 by default).");
        info.TryGetList("AutotileGroups", out List<AutotileGroup> autotileGroups, new List<AutotileGroup>(), "Autotile groups not found or invalid (Set to empty list by default).");

        TileWidth = tileWidth;
        TileHeight = tileHeight;
        AutotileGroups = autotileGroups;
    }

    public override Bitmap GetBitmap(bool forceReload = false)
    {
        if(_BitmapCache != null)
            return _BitmapCache; // Return the cached bitmap if it exists
        
        var combinedBitmap = GetSKBitmap(); // Use the SkiaSharp method to get the combined bitmap
        
        // Convert the SkiaSharp bitmap to Avalonia's Bitmap
        using var stream = new MemoryStream();
        combinedBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        stream.Seek(0, SeekOrigin.Begin);
        _BitmapCache = new Bitmap(stream);
        return _BitmapCache; // Return the Avalonia bitmap
    }

    public List<Tileset> GetUsedTilesets()
    {
        // Returns a list of tilesets used in this autotileset.
        return AutotileGroups.Select(at => at.BaseTile.Tileset).Distinct().ToList();
    }
}
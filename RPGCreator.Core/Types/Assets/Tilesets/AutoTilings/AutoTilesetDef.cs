using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Types.Internal;
using Serilog;
using SkiaSharp;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class AutoTilesetDef : ImageAsset, ITilesetDef
{
    public const int MaxTilesByRow = 8; // Maximum number of tiles in a row for the tileset family
    
    public event Action? ImageChanged;
    public Ulid Unique { get; private set; }
    public URN Urn => new URN("tileset", $"{Name}@{Unique.ToString()}");

    public string ImagePath { get; }
    public string Name { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public Bitmap? BitmapCache { get; }
    public List<AutotileGroupDef> AutotileGroups { get; set; } = new(); // List of autotilings for this tileset
    public Texture2D GetTexture(GraphicsDevice graphicsDevice)
    {
        throw new NotImplementedException();
    }

    public AutoTilesetDef(string name = "", int tileWidth = 32, int tileHeight = 32, string imagePath = "")
    {
        Unique = Ulid.NewUlid();
        Name = Name;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        ImagePath = imagePath;
        
        // Set default image dimensions based on tile size
        ImageWidth = tileWidth * MaxTilesByRow; // Assuming a maximum of 8 tiles horizontally
        ImageHeight = tileHeight * (AutotileGroups.Count / MaxTilesByRow + 1); // Height based on the number of tiles
    }
    
    public Bitmap GetSimpleBitmap()
    {
        throw new NotImplementedException();
    }

    public ITileDef GetTileAt(int col, int row)
    {
        // Check if the autotilings contain a tile at the specified row and column.
        return AutotileGroups
            .Select(at => at.GetTileAt(null, new Point(row, col))) // Pass null for the layer as we don't have a layer context here.
            .FirstOrDefault(tile => tile != null) ?? throw new InvalidOperationException($"No tile found at ({row}, {col}) in autotilings.");
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

    public SKBitmap GetSKBitmap()
    {
        SKBitmap combinedBitmap = new SKBitmap(
            TileWidth * MaxTilesByRow, // Width of the combined bitmap
            TileHeight * (AutotileGroups.Count / MaxTilesByRow + 1) // Height based on the number of tiles
        );
        
        var maxX = TileWidth * MaxTilesByRow; // Assuming a maximum of 8 tiles horizontally
        var currentX = 0;
        var currentY = 0; // Current Y position in the combined bitmap
        foreach (var autotileGroup in AutotileGroups)
        {
            var tile = autotileGroup.BaseTile;

            if (tile == null)
            {
                Log.Warning("Autotile groupInstance {GroupName} has no base tile.", autotileGroup.Name);
                continue; // Skip this autotile groupInstance if it has no base tile.
            }
            
            var imagePath = tile.TilesetDef.ImagePath;
            
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
        SerializationInfo info = new SerializationInfo(typeof(AutoTilesetInstance));
        AddBaseSerialization(info);
        info.AddValue("TileWidth", TileWidth);
        info.AddValue("TileHeight", TileHeight);
        info.AddValue("AutotileGroups", AutotileGroups);
        return info;
    }

    public void SetObjectData(Serializer.DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("unique", out Ulid unique, Ulid.Empty);
        info.TryGetValue("name", out string name, "UNKNOWN ASSET");
        info.TryGetValue("TileWidth", out int tileWidth, 32);
        info.TryGetValue("TileHeight", out int tileHeight, 32);
        info.TryGetList("AutotileGroups", out List<AutotileGroupDef> autotileGroups);

        Unique = unique;
        Name = name;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        AutotileGroups = autotileGroups;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
}
using Avalonia;
using Avalonia.Media.Imaging;
using RPGCreator.Core.Type.Map;
using SkiaSharp;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets;

/*
 *
 * NTileset.cs
 * ===========
 * This class represents a tileset asset in the RPG Creator framework.
 *
 * Note: This class is the new version of Tileset.cs, which was previously used.
 *      It is designed to be more efficient and easier to use.
 *      It combines the logic of the old Tileset system, with the new Autotiling system.
 *
 * DevNote:
 * For now, this class is still in development and is not fully implemented.
 * Right now I'm still just trying to get the basic functionality working.
 * [Ward727, 26/07/2025]
 * 
 */
public enum ERulePos
{
   TOP_LEFT,
   TOP,
   TOP_RIGHT,
   LEFT,
   RIGHT,
   BOTTOM_LEFT,
   BOTTOM,
   BOTTOM_RIGHT,
}

public enum ERuleType
{
   WHITELIST,
   BLACKLIST,
}

public interface ITileable
{
    public Point UV { get; }
    public Point Position { get; } // Position in the tileset grid (row by column)
    public NTileset Tileset { get; } // The tileset this tile belongs to

    /// <summary>
    /// This method returns a drawable tile based on the current tileable object.<br/>
    /// It's mainly useful for autotiles or tiles that need to be drawn differently based on the context.<br/>
    /// If the tileable object is a simple tile, it will just return a copy of itself.
    /// </summary>
    /// <param name="layer">The map layer where this tile will be drawn</param>
    /// <param name="position">The position where this tile will be drawn</param>
    /// <returns></returns>
    public ITileable? GetDrawableTile(MapLayer? layer = null, Point? position = null);
    public ITileable GetCopy(); // Returns a copy of the tileable object.
}

public class NTile : ITileable
{
    public Point UV { get; }
    public Point Position { get; }
    public NTileset Tileset { get; }
    
    public NTile()
    {
    }

    public NTile(Point uv, Point position, NTileset tileset)
    {
        UV = uv;
        Position = position;
        Tileset = tileset;
    }
    
    public ITileable? GetDrawableTile(MapLayer? layer = null, Point? position = null)
    {

        return GetCopy();

    }

    public ITileable GetCopy()
    {

        return new NTile(UV, Position, Tileset);

    }
    /// <summary>
    /// This bitmap SHOULD NOT be used directly for rendering.<br/>
    /// For rendering, use the GetTileAt method of the corresponding TilesetFamily or NTileset.<br/>
    /// This bitmap it could be used in the editor or for other purposes.<br/>
    /// </summary>
    /// <remarks>Not for in-game rendering.</remarks>
    protected virtual SKBitmap GetTileBitmap()
    {
        var imageToCrop = SKBitmap.Decode(Tileset.ImagePath);
        
        var cropRegion = new SKRectI(
            UV.X,
            UV.Y,
            UV.X + Tileset.TileWidth,
            UV.Y + Tileset.TileHeight
        );
        var drawRegion = new SKRectI(
            0,
            0,
            Tileset.TileWidth,
            Tileset.TileHeight
        );
        
        var tileBitmap = new SKBitmap(Tileset.TileWidth, Tileset.TileHeight);
        using var canvas = new SKCanvas(tileBitmap);
        
        canvas.DrawBitmap(imageToCrop, cropRegion, drawRegion);
        
        return tileBitmap;
    }

    protected virtual CroppedBitmap GetTileBmpAvalonia()
    {
        var tilesetBitmap = new Bitmap(Tileset.ImagePath);
        
        var cropRegion = new PixelRect(
            UV.X,
            UV.Y,
            UV.X + Tileset.TileWidth,
            UV.Y + Tileset.TileHeight
        );

        var croppedBitmap = new CroppedBitmap(tilesetBitmap, cropRegion);
        return croppedBitmap;
    }
}

public class NAutotileRules : ISerializable, IDeserializable
{
    public Ulid ID { get; private set; } = Ulid.NewUlid();
    public string Name = "";
    public string Description = "";
    public ERuleType Type = ERuleType.WHITELIST;
    public ERulePos Side = ERulePos.TOP_LEFT; // Default position, can be changed later
    public List<string> Tags = [];
    
    public NAutotileRules()
    {
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(Autotile_rule));
        info.AddValue("ID", ID);
        info.AddValue("Name", Name);
        info.AddValue("Description", Description);
        info.AddValue("Type", Type);
        info.AddValue("Side", Side);
        info.AddValue("Tags", Tags);
        return info;
    }

    public void SetObjectData(SerializationInfo info)
    {
        info.TryGetValue("ID", out Ulid ID);
        info.TryGetValue("Name", out Name);
        info.TryGetValue("Description", out Description);
        info.TryGetValue("Type", out Type);
        info.TryGetValue("Side", out Side);
        info.TryGetList("Tags", out Tags);
            
        this.ID = ID;
    }
}

public class NAutotile : ITileable, ISerializable, IDeserializable
{
    public List<NAutotileRules> Rules = new(); // List of rules for this autotile
    public List<string> Tags = []; // Tags for this autotile, can be used for filtering or categorization

    public Point UV { get; set; }
    public Point Position { get; set; }
    public NTileset Tileset { get; set; }
    private bool hasCheckedRule = false; // Flag to check if the rules have been checked already, to avoid unnecessary checks
    public AutotileGroup AutotileGroup { get; }

    public NAutotile()
    {
    }
    
    public NAutotile(Point uv, Point position, NTileset tileset)
    {
        UV = uv;
        Position = position;
        Tileset = tileset;
    }

    public ITileable? GetDrawableTile(MapLayer? layer = null, Point? position = null)
    {
        if (layer == null || position == null)
            return null;

        if (RespectRules(layer, position.Value))
            return GetCopy();

        var tile = AutotileGroup.GetTileAt(layer, position.Value);
        return tile?.GetCopy();
    }
    
    public ITileable GetCopy()
    {
        // Create a copy of the autotile with the same UV, Position and Tileset
        return new NAutotile(UV, Position, Tileset)
        {
            Rules = new List<NAutotileRules>(Rules), // Copy the rules
            Tags = new List<string>(Tags) // Copy the tags
        };
    }

    public bool AddRule(NAutotileRules rule)
    {
        if (Rules.Contains(rule))
            return false;
        
        Rules.Add(rule);
        return true;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(NAutotile));
        info.AddValue("UV", UV);
        info.AddValue("Position", Position);
        info.AddValue("Tileset", Tileset.Unique);
        info.AddValue("Rules", Rules);
        info.AddValue("Tags", Tags);
        return info;
    }

    public void SetObjectData(SerializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("UV", out Point uv, new Point(0, 0), "UV not found or invalid (Set to (0, 0) by default).");
        info.TryGetValue("Position", out Point position, new Point(0, 0), "Position not found or invalid (Set to (0, 0) by default).");
        info.TryGetValue("Tileset", out Ulid tilesetUnique, Ulid.NewUlid(), "Tileset not found or invalid (Set to new Ulid by default).");
        info.TryGetList("Rules", out List<NAutotileRules> rules, [], "Rules not found or invalid (Set to empty list by default).");
        info.TryGetList("Tags", out List<string> tags, [], "Tags not found or invalid (Set to empty list by default).");

        UV = uv;
        Position = position;
        Rules = rules ?? [];
        Tags = tags ?? [];

        void OnEditedProjectOnOnProjectLoaded()
        {
            // When the project is loaded, we need to get the tileset from the project data
            Tileset = EngineCore.Instance.Data.EditedProject.GetAssetsType<NTileset>(BaseAsset.TYPE.TILESETS)
                .FirstOrDefault(t => t.Unique == tilesetUnique) ?? throw new Exception($"Tileset with unique ID {tilesetUnique} not found in the project.");
            
            EngineCore.Instance.Data.EditedProject.OnProjectLoaded -= OnEditedProjectOnOnProjectLoaded; // Unsubscribe from the event to avoid memory leaks
        }

        EngineCore.Instance.Data.EditedProject.OnProjectLoaded += OnEditedProjectOnOnProjectLoaded;
    }

    //TODO: Need to implement some parallel processing for this method, as it can be slow if there are many rules.
    /// <summary>
    /// Checks if the autotile respects the rules defined for it based on the position in the given MapLayer.<br/>
    /// The autotile is considered valid if it matches the rules defined for its position.<br/>
    /// The rules can be either a whitelist or a blacklist.<br/>
    /// A whitelist means that the autotile must match the rules to be valid, while a blacklist means that the autotile must not match the rules to be valid.<br/>
    /// The position is relative to the autotile's position in the MapLayer.<br/>
    /// The autotile is considered valid if it matches all the rules defined for its position.<br/>
    /// If there are no rules defined for the autotile, it is considered valid by default.
    /// </summary>
    /// <remarks>
    /// This method is still in development and may not be fully optimized.<br/><br/>
    /// <b>Notable point:</b> it does not currently support parallel processing, which may lead to performance issues if there are many rules.
    /// </remarks>
    public bool RespectRules(MapLayer? layer, Point position)
    {
        if (layer == null)
            return false;
        if(Rules.Count == 0)
            return true; // If there are no rules, the autotile is always valid

        foreach (var rule in Rules)
        {
            var rulePosition = rule.Side switch
            {
                ERulePos.TOP_LEFT => new Point(position.X - 1, position.Y - 1),
                ERulePos.TOP => new Point(position.X, position.Y - 1),
                ERulePos.TOP_RIGHT => new Point(position.X + 1, position.Y - 1),
                ERulePos.LEFT => new Point(position.X - 1, position.Y),
                ERulePos.RIGHT => new Point(position.X + 1, position.Y),
                ERulePos.BOTTOM_LEFT => new Point(position.X - 1, position.Y + 1),
                ERulePos.BOTTOM => new Point(position.X, position.Y + 1),
                ERulePos.BOTTOM_RIGHT => new Point(position.X + 1, position.Y + 1),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var ruleType = rule.Type;
            
            var tileAtRulePosition = layer.GetTileAt(rulePosition.X, rulePosition.Y);

            if (tileAtRulePosition == null)
            {
                if (ruleType == ERuleType.WHITELIST)
                {
                    return false; // If the rule is a whitelist and the tile at the rule position is null, the autotile is invalid
                }
                continue;
            }
            
            if(tileAtRulePosition is NTile)
            {
                // If the rule is a whitelist and the tile at the rule position is not in the autotile's tags, the autotile is invalid
                if (ruleType == ERuleType.WHITELIST)
                {
                    return false; // The autotile does not match the rule
                }

                continue;
            }
                
            if (tileAtRulePosition is NAutotile autotile)
            {
                if(autotile.Tags.Count == 0)
                {
                    if (ruleType == ERuleType.WHITELIST)
                    {
                        return false; // If the autotile has no tags, it doesn't match the rule
                    }
                    continue;
                }

                switch (ruleType)
                {
                    case ERuleType.WHITELIST:
                    {
                        // If the rule has a tag that is not in the auto-tile's tags, the autotile is invalid
                        if (rule.Tags.Any(t => !autotile.Tags.Contains(t)))
                        {
                            return false; // If the auto-tile has any tag that does not match the rule, it is invalid
                        }

                        continue;
                    }
                    case ERuleType.BLACKLIST:
                    {
                        if (autotile.Tags.Any(t => rule.Tags.Contains(t)))
                        {
                            return false; // If the auto-tile has any tag that matches the rule, it is invalid
                        }

                        continue;
                    }
                    default:
                        return false; // If the rule type is not recognized, we consider the auto-tile invalid
                }
            }
        }

        return true;
    }
}

public class AutotileGroup : ISerializable, IDeserializable
{
    public string Name { get; set; } = "Unnamed Autotile Group"; // Name of the autotile group
    public Ulid Unique { get; private set; } = Ulid.NewUlid();
    public List<Point> AffectedTiles; // List of affected tiles by this autotiling
    public List<NAutotile> Autotiles;
    public ITileable BaseTile; // The base tile for this autotile
    public List<string> GroupTags = []; // Tags for this autotile group, can be used for filtering or categorization
    public ITileset Tileset => BaseTile.Tileset; // The tileset this autotile group belongs to

    public AutotileGroup()
    {
    }

    public bool AddTile(ITileable tile)
    {
        if (tile == null || AffectedTiles.Contains(tile.Position))
            return false; // If the tile is null or already in the list, we can't add it
        
        AffectedTiles.Add(tile.Position);
        Autotiles.Add(new NAutotile(tile.UV, tile.Position, tile.Tileset));
        return true; // Successfully added the tile
    }

    public ITileable? GetTileAt(MapLayer? layer, Point position)
    {
        var tile = Autotiles.FirstOrDefault(autotile => autotile.RespectRules(layer, position));
        return tile ??
               BaseTile;
    }

    public ITileable? GetDirectTileAt(Point position)
    {
        return Autotiles.FirstOrDefault(autotile => autotile.Position.X == position.X && autotile.Position.Y == position.Y);
    }

    public bool RemoveTile(NAutotile tile)
    {
        if (!Autotiles.Contains(tile))
        {
            return false;
        }
        
        AffectedTiles.Remove(tile.Position);
        Autotiles.Remove(tile);
        return true;
    }

    public bool RemoveTile(Point position)
    {
        if (!HasTile(position))
            return false;

        var tileToRemove = GetDirectTileAt(position);
        if (tileToRemove == null)
            return false;
        
        return RemoveTile((NAutotile)tileToRemove);
    }

    public bool SetBaseTile(ITileable tile)
    {
        if (!Autotiles.Contains(tile))
            return false;
        BaseTile = tile;
        return true;
    }
    
    public bool HasTile(Point position)
    {
        return AffectedTiles.Contains(position);
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(AutotileGroup));
        info.AddValue("Unique", Unique);
        info.AddValue("AffectedTiles", AffectedTiles);
        info.AddValue("Autotiles", Autotiles);
        info.AddValue("BaseTile", BaseTile);
        return info;
    }

    public void SetObjectData(SerializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Unique", out Ulid unique, Ulid.NewUlid(), "Unique ID not found or invalid (Set to new Ulid by default).");
        info.TryGetList("AffectedTiles", out List<Point> affectedTiles, [], "Affected tiles not found or invalid (Set to empty list by default).");
        info.TryGetList("Autotiles", out List<NAutotile> autotiles, [], "Autotiles not found or invalid (Set to empty list by default).");
        info.TryGetValue("BaseTile", out ITileable baseTile, null, "Base tile not found or invalid (Set to null by default).");

        Unique = unique;
        AffectedTiles = affectedTiles;
        Autotiles = autotiles;
        BaseTile = baseTile ?? throw new InvalidOperationException("Base tile cannot be null.");
    }
}

public interface ITileset
{
    public string Name { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }

    /// <summary>
    /// Define if the tileset is a simple tileset or not.<br/>
    /// A simple tileset is a tileset that does not use autotiling.<br/>
    /// A "complex" tileset is a tileset that uses autotiling.
    /// </summary>
    public bool IsSimple { get; }
    
    public bool HasTile(int row, int column);
    public bool HasTile(Point rowColumn);
    public ITileable? GetTileAt(int row, int column);
    public SKBitmap GetSKBitmap();
    public Bitmap GetBitmap(bool forceReload = false);
}

public class NAutoTileset : ImageAsset, ITileset, ISerializable, IDeserializable
{
    public const int MaxTilesByRow = 8; // Maximum number of tiles in a row for the tileset family
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public bool IsSimple { get; } = true;
    public List<AutotileGroup> AutotileGroups { get; set; } = new(); // List of autotilings for this tileset
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
            var tilePosX = tile.Position.X * TileWidth;
            var tilePosY = tile.Position.Y * TileHeight;
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
        throw new NotImplementedException();
    }

    public void SetObjectData(SerializationInfo info)
    {
        throw new NotImplementedException();
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

    public List<NTileset> GetUsedTilesets()
    {
        // Returns a list of tilesets used in this autotileset.
        return AutotileGroups.Select(at => at.BaseTile.Tileset).Distinct().ToList();
    }
}

public class NTileset : ImageAsset, ITileset, ISerializable, IDeserializable
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public bool IsSimple { get; set; } = true; // Indicates if the tileset is a simple tileset (no autotiling)
 
    public NTileset() : base()
    {
        Type = TYPE.TILESETS;
    }
    
    public NTileset(string name) : base(name)
    {
        Type = TYPE.TILESETS;
    }
    
    public NTileset(string name, int tileWidth, int tileHeight, string imagePath) : base(name)
    {
        Type = TYPE.TILESETS;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        ImagePath = imagePath;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(NTileset));
        info.AddValue("Unique", Unique);
        info.AddValue("Name", Name);
        info.AddValue("ImagePath", ImagePath);
        info.AddValue("TileWidth", TileWidth);
        info.AddValue("TileHeight", TileHeight);
        info.AddValue("IsSimple", IsSimple);
        return info;
    }

    public void SetObjectData(SerializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Unique", out Ulid unique, Ulid.NewUlid(), "Unique ID not found or invalid (Set to new Ulid by default).");
        info.TryGetValue("Name", out string name, "Unnamed Tileset", "Name not found or invalid (Set to 'Unnamed Tileset' by default).");
        info.TryGetValue("ImagePath", out string imagePath, string.Empty, "Image path not found or invalid (Set to empty string by default).");
        info.TryGetValue("TileWidth", out int tileWidth, 32, "Tile width not found or invalid (Set to 32 by default).");
        info.TryGetValue("TileHeight", out int tileHeight, 32, "Tile height not found or invalid (Set to 32 by default).");
        info.TryGetValue("IsSimple", out bool isSimple, true, "Is simple tileset not found or invalid (Set to true by default).");

        Unique = unique;
        Name = name;
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        IsSimple = isSimple;
    }
    
    public ITileable? GetTileAt(int row, int column)
    {
        var uv = new Point(row * TileWidth, column * TileHeight);
        var position = new Point(row, column);
        
        return new NTile(
            uv,
            position,
            this
        );
    }
    
    public bool HasTile(int row, int column)
    {
        // In a simple tileset, we can always assume the tile exists if the row and column are within bounds.
        return row >= 0 && column >= 0 && row < (ImageWidth / TileWidth) && column < (ImageHeight / TileHeight);
    }
    
    public bool HasTile(Point rowColumn)
    {
        return HasTile(rowColumn.X, rowColumn.Y);
    }

    public SKBitmap GetSKBitmap()
    {
        return SKBitmap.Decode(ImagePath); // If the tileset is simple, return the bitmap of the tileset image.
    }

    public Bitmap GetSimpleBitmap()
    {
        return new Bitmap(ImagePath);
    }
    
    public override Bitmap GetBitmap(bool forceReload = false)
    {
        if(_BitmapCache != null)
            return _BitmapCache;
        
        return GetSimpleBitmap();
        
        // If the tileset is not single, we need to combine the images of all tiles inside the autotiling and return a single bitmap.
    }
}

public class TilesetFamily : BaseAsset
{
    public const int MaxTilesByRow = 8; // Maximum number of tiles in a row for the tileset family
    public List<NTileset> Tilesets { get; set; } = [];
    public bool IsMono { get; set; } = true; // Indicates if the tileset family is mono (single tileset) or not
    public Dictionary<Point, ITileable> TilesMapping { get; set; } = []; // Mapping of tiles in the tileset family

    /// <summary>
    /// Generates a mapping of tiles in the tileset family.<br/>
    /// This method only works if the tileset family is not mono (i.e., it contains multiple tilesets).<br/>
    /// If any tileset in the family is simple, it will log an error message and skip that tileset (As simple tilesets doesn't contain auto-tiling).
    /// </summary>
    public void GenerateMapping()
    {
        // TilesMapping.Clear();
        // if (IsMono)
        //     return; // If the tileset family is mono, we don't need to generate a mapping as we just have to get it from the single tileset.
        //
        // foreach (var tileset in Tilesets)
        // {
        //     if (tileset.IsSimple)
        //     {
        //         Console.ForegroundColor = ConsoleColor.Red;
        //         Console.WriteLine($"Tileset {tileset.Name} is simple, as the tileset family isn't mono, this is a problem.");
        //         Console.ResetColor();
        //         continue; // If the tileset is simple, we can't generate a mapping for it.
        //     }
        //     
        //     foreach (var autotile in tileset.GetAutotilings())
        //     {
        //         // Add the base tile to the mapping
        //         TilesMapping[autotile.BaseTile.Position] = autotile.BaseTile;
        //     }
        // }
    }

    public ITileable? GetTileAt(Point at)
    {
        if (IsMono)
        {
            return Tilesets[0].GetTileAt(at.X, at.Y);
        }
        
        if (TilesMapping.TryGetValue(at, out var tile))
        {
            return tile;
        }
        
        return null; // If the tile is not found in the mapping, return null.
    }
    
    public bool HasTile(Point at)
    {
        if (IsMono)
            return Tilesets[0].HasTile(at.X, at.Y);
        
        return TilesMapping.ContainsKey(at); // Check if the tile exists in the mapping.
    }

    public SKBitmap GetBitmap()
    {
        if (IsMono)
        {
            return SKBitmap.Decode(Tilesets[0].ImagePath);
        }
        
        // If the tileset family is not mono, we need to combine the images of all tiles inside the TilesMapping and return a single bitmap.
        
        SKBitmap combinedBitmap = new SKBitmap(
            TileWidth * MaxTilesByRow, // Width of the combined bitmap
            TileHeight * (TilesMapping.Count / MaxTilesByRow + 1) // Height based on the number of tiles
        );
        
        var maxX = TileWidth * MaxTilesByRow; // Assuming a maximum of 8 tiles horizontally
        var currentX = 0;
        var currentY = 0; // Current Y position in the combined bitmap
        
        foreach (var tilesPair in TilesMapping)
        {
            var tile = tilesPair.Value;
            
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
            var tilePosX = tile.Position.X * TileWidth;
            var tilePosY = tile.Position.Y * TileHeight;
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

    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
}
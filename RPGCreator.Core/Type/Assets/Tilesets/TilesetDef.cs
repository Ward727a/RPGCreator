using Avalonia.Media.Imaging;
using RPGCreator.Core.Type.Internal;
using SkiaSharp;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class TilesetDef : ImageAsset, ITilesetDef
{
    public event Action? ImageChanged;
    public URN Urn { get; private set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public Bitmap? BitmapCache => _BitmapCache;

    public TilesetDef()
    {
        Type = TYPE.TILESETS;
    }
    
    public TilesetDef(string imagePath, string name, int tileWidth = 32, int tileHeight = 32)
    {
        Unique = Ulid.NewUlid();
        Name = name;
        Urn = new URN("tileset" , $"{name}@{Unique}");
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Type = TYPE.TILESETS;
    }
    
    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(TilesetDef));

        info.AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Urn), Urn)
            .AddValue(nameof(ImagePath), ImagePath)
            .AddValue(nameof(TileWidth), TileWidth)
            .AddValue(nameof(TileHeight), TileHeight);
        
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.Empty, $"{nameof(TilesetDef)}.{nameof(Unique)} not found or invalid (Set to Ulid.Empty by default).");
        info.TryGetValue(nameof(Name), out string name, string.Empty, $"{nameof(TilesetDef)}.{nameof(Name)} not found or invalid (Set to empty string by default).");
        info.TryGetValue(nameof(Urn), out URN urn, URN.Empty, $"{nameof(TilesetDef)}.{nameof(Urn)} not found or invalid (Set to URN.Empty by default).");
        info.TryGetValue("ImagePath", out string imagePath, string.Empty, $"{nameof(TilesetDef)}.{nameof(ImagePath)} not found or invalid (Set to empty string by default).");
        info.TryGetValue("TileWidth", out int tileWidth, 32, $"{nameof(TilesetDef)}.{nameof(TileWidth)} not found or invalid (Set to 32 by default).");
        info.TryGetValue("TileHeight", out int tileHeight, 32, $"{nameof(TilesetDef)}.{nameof(TileHeight)} not found or invalid (Set to 32 by default).");

        Unique = unique;
        Name = name;
        Urn = urn;
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
    }

    public SKBitmap GetSKBitmap()
    {
        return SKBitmap.Decode(ImagePath);
    }

    public Bitmap GetSimpleBitmap()
    {
        return new Bitmap(ImagePath);
    }

    public ITileDef GetTileAt(int col, int row)
    {
        if (col < 0 || row < 0)
            throw new ArgumentOutOfRangeException("Column and row must be non-negative.");

        var positionInTileset = new Point(col * TileWidth, row * TileHeight);
        return new TileDefinition(new Point(TileWidth, TileHeight), positionInTileset, this);
    }

    public override Bitmap GetBitmap(bool forceReload = false)
    {
        if(BitmapCache != null)
            return BitmapCache;
        
        return GetSimpleBitmap();
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
}
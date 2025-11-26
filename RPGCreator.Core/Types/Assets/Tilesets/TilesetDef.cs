using Avalonia.Media.Imaging;
using RPGCreator.Core.Types.Internal;
using Serilog;
using SkiaSharp;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class TilesetDef : ImageAsset, ITilesetDef,ISerializable, IDeserializable
{
    public event Action? ImageChanged;

    public string Name { get; set; } = "";
    public Ulid Unique { get; private set; }
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

    public void SetObjectData(Serializer.DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.Empty);
        info.TryGetValue(nameof(Name), out string name, string.Empty);
        info.TryGetValue(nameof(Urn), out URN urn, URN.Empty);
        info.TryGetValue("ImagePath", out string imagePath, string.Empty);
        info.TryGetValue("TileWidth", out int tileWidth, 32);
        info.TryGetValue("TileHeight", out int tileHeight, 32);

        Unique = unique;
        Name = name;
        Urn = urn;
        if (Urn.IsEmpty)
        {
            Urn = new URN("tileset" , $"{name}@{Unique}");
        }
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        
        Log.Debug("[TilesetDef] SetObjectData ({0}, {1})", Unique, Name);
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
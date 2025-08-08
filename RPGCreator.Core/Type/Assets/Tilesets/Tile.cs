using Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Map;
using SkiaSharp;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class Tile : BaseDrawable, ITileable, ISerializable, IDeserializable
{
    public Point SizeInTileset { get; set; }
    public Point PositionInTileset { get; set; }
    public Rectangle UV => new Rectangle(PositionInTileset, SizeInTileset);
    public Tileset Tileset { get; private set; }
    
    public Tile()
    {
    }

    public Tile(Point sizeInTileset, Point positionInTileset, Tileset tileset)
    {
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        Tileset = tileset;
    }

    public void UpdateTileset(ITileset newTileset)
    {
        if(newTileset is Tileset tileset)
            Tileset = tileset;
#if DEBUG
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Attempted to update NTileset with a non-NTileset type: {newTileset.GetType().Name}");
            Console.ResetColor();
        }
#endif
    }

    public ITileable? GetDrawableTile(TileLayer? layer = null, Point? position = null)
    {

        return GetCopy();

    }

    public ITileable GetCopy()
    {

        return new Tile(SizeInTileset, PositionInTileset, Tileset);

    }

    public bool IsEqualTo(ITileable other)
    {
        return other.Tileset.Unique == Tileset.Unique &&
               other.PositionInTileset.X == PositionInTileset.X && other.PositionInTileset.Y == PositionInTileset.Y &&
               other.SizeInTileset.X == SizeInTileset.X && other.SizeInTileset.Y == SizeInTileset.Y;
    }

    protected override void _Draw(SpriteBatchExtend? sb)
    {
        sb.Draw(Tileset.GetTexture(sb.GraphicsDevice), Position, UV, Color.White);
    }

    protected override void _Update(GameTime gameTime)
    {
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
            SizeInTileset.X,
            SizeInTileset.Y,
            SizeInTileset.X + Tileset.TileWidth,
            SizeInTileset.Y + Tileset.TileHeight
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
            SizeInTileset.X,
            SizeInTileset.Y,
            SizeInTileset.X + Tileset.TileWidth,
            SizeInTileset.Y + Tileset.TileHeight
        );

        var croppedBitmap = new CroppedBitmap(tilesetBitmap, cropRegion);
        return croppedBitmap;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(Tile));
        info.AddValue("SizeInTileset", SizeInTileset);
        info.AddValue("PositionInTileset", PositionInTileset);
        info.AddValue("Tileset", Tileset.Unique);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("SizeInTileset", out Point sizeInTileset, new Point(32, 32), "Size in tileset not found or invalid (Set to 32x32 by default).");
        info.TryGetValue("PositionInTileset", out Point positionInTileset, new Point(0, 0), "Position in tileset not found or invalid (Set to 0,0 by default).");
        info.TryGetValue("Tileset", out Ulid tilesetUnique, Ulid.Empty, "Tileset not found or invalid.");

        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;

        void OnEditedProjectLoaded()
        {
            var tileset = EngineCore.Instance.Data.EditedProject.GetAssetsType<Tileset>(BaseAsset.TYPE.TILESETS)
                .FirstOrDefault(t => t.Unique == tilesetUnique);
            
            if (tileset == null)
            {
                throw new Exception($"Tileset with unique ID {tilesetUnique} not found in the project.");
            }

            Tileset = tileset;
            
            EngineCore.Instance.Data.EditedProject.OnProjectLoaded -= OnEditedProjectLoaded; // Unsubscribe from the event to avoid memory leaks
        }

        EngineCore.Instance.Data.EditedProject.OnProjectLoaded += OnEditedProjectLoaded;
    }
}
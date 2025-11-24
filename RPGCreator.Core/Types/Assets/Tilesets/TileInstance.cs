using Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using SkiaSharp;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class TileInstance : BaseDrawable, ITileInstance
{
    public ITileDef Definition { get; private set; }
    public TileInstance(ITileDef tileDef)
    {
        Definition = tileDef;
        Position = tileDef.DefaultPosition;
    }


    public ITileInstance? GetDrawableTile(TileLayerDefinition? layer = null, Microsoft.Xna.Framework.Point? position = null)
    {
        return GetCopy();
    }

    public ITileInstance GetCopy()
    {

        return new TileInstance(Definition);
    }

    public bool IsEqualTo(ITileInstance other)
    {
        if (other == null) return false;

        return Definition.IsEqualTo(other.Definition);
    }


    protected override void _Draw(SpriteBatchExtend? sb)
    {
        sb.Draw(Definition.TilesetDef.GetTexture(sb.GraphicsDevice), Position, Definition.UV, Color.White);
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
        var imageToCrop = SKBitmap.Decode(Definition.TilesetDef.ImagePath);
        
        var cropRegion = new SKRectI(
            Definition.SizeInTileset.X,
            Definition.SizeInTileset.Y,
            Definition.SizeInTileset.X + Definition.TilesetDef.TileWidth,
            Definition.SizeInTileset.Y + Definition.TilesetDef.TileHeight
        );
        var drawRegion = new SKRectI(
            0,
            0,
            Definition.TilesetDef.TileWidth,
            Definition.TilesetDef.TileHeight
        );
        
        var tileBitmap = new SKBitmap(Definition.TilesetDef.TileWidth, Definition.TilesetDef.TileHeight);
        using var canvas = new SKCanvas(tileBitmap);
        
        canvas.DrawBitmap(imageToCrop, cropRegion, drawRegion);
        
        return tileBitmap;
    }

    protected virtual CroppedBitmap GetTileBmpAvalonia()
    {
        var tilesetBitmap = new Bitmap(Definition.TilesetDef.ImagePath);
        
        var cropRegion = new PixelRect(
            Definition.SizeInTileset.X,
            Definition.SizeInTileset.Y,
            Definition.SizeInTileset.X + Definition.TilesetDef.TileWidth,
            Definition.SizeInTileset.Y + Definition.TilesetDef.TileHeight
        );

        var croppedBitmap = new CroppedBitmap(tilesetBitmap, cropRegion);
        return croppedBitmap;
    }

    public void Clean()
    {
        // No resources to clean up for TileInstance
        // If there were any disposable resources, they would be disposed here.
    }

    public void ResetFrom(ITileDef def, params object[] parameters)
    {
        if (def == null)
        {
            throw new ArgumentNullException(nameof(def), "Tile definition cannot be null.");
        }

        // Reset the tile instance to the provided definition
        Definition = def;
        Position = def.DefaultPosition;
    }
}
using Avalonia;
using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.CSharp.Extensions;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using SkiaSharp;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class TileInstance : BaseDrawable, ITileInstance
{
    public System.Numerics.Vector2 Position { get; set; }
    public ITileDef Definition { get; private set; }

    public TileInstance()
    {
    }

    public TileInstance(ITileDef tileDef)
    {
        Definition = tileDef;
        Position = tileDef.Position;
    }


    public ITileInstance? GetDrawableTile(TileLayerDefinition? layer = null, System.Drawing.Point? position = null)
    {
        return GetCopy();
    }

    public ITileInstance GetCopy()
    {

        return new TileInstance(Definition);
    }

    public void Update(TimeSpan deltaTime)
    {
        throw new NotImplementedException();
    }

    public bool IsEqualTo(ITileInstance other)
    {
        if (other == null) return false;

        return Definition.IsEqualTo(other.Definition);
    }


    protected override void _Draw(SpriteBatchExtend? sb)
    {
        var texture = EngineServices.ResourcesService.Load<Texture2D>(Definition.TilesetDef.ImagePath);
        switch (Definition.Flip)
        {
            case TileFlip.None:
                sb.Draw(texture, Position, Definition.UV.ToRectangle(), Color.White);
                return;
            case TileFlip.Horizontal:
                sb.Draw(texture, Position, Definition.UV.ToRectangle(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
                return;
            case TileFlip.Vertical:
                sb.Draw(texture, Position, Definition.UV.ToRectangle(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
                return;
            case TileFlip.Both:
                sb.Draw(texture, Position, Definition.UV.ToRectangle(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
                return;
        }
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
        
        var cropRegion = new SKRect(
            Definition.SizeInTileset.Width,
            Definition.SizeInTileset.Height,
            Definition.SizeInTileset.Width + Definition.TilesetDef.TileWidth,
            Definition.SizeInTileset.Height + Definition.TilesetDef.TileHeight
        );
        var drawRegion = new SKRect(
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
            (int)Definition.SizeInTileset.Width,
            (int)Definition.SizeInTileset.Height,
            (int)Definition.SizeInTileset.Width + Definition.TilesetDef.TileWidth,
            (int)Definition.SizeInTileset.Height + Definition.TilesetDef.TileHeight
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
        Position = def.Position;
    }
}
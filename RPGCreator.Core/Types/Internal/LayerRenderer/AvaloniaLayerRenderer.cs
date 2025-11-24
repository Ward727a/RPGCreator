using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Types.Internal.LayerRenderer;

/// <summary>
/// Layer renderer for Avalonia (aka RPGCreator.UI).<br/>
/// This class is responsible for rendering tile layers in the Avalonia UI framework.<br/>
/// It implements the <see cref="ILayerRenderer{TElementDef,TElementInstance}"/> interface, which defines the contract for rendering layers.
/// </summary>
public class AvaloniaLayerRenderer : ILayerRenderer<ITileDef, ITileInstance>
{
    private readonly Canvas _drawingCanvas;
    
    /// <summary>
    /// Create a new instance of <see cref="AvaloniaLayerRenderer"/>.
    /// </summary>
    /// <param name="drawingCanvas">The canvas where the layer will be drawn</param>
    public AvaloniaLayerRenderer(Canvas drawingCanvas)
    {
        _drawingCanvas = drawingCanvas;
    }

    public void Draw(IMapLayerInstance<ITileDef, ITileInstance> tileLayer)
    {
        _drawingCanvas.Children.Clear(); // Clear the canvas before drawing

        foreach (var element in tileLayer.InstancedElements.ToList())
        {
            var tile = element.Value;
            var position = element.Key;
            
            var croppedBitmap = new CroppedBitmap(tile.Definition.TilesetDef.GetSimpleBitmap(), new PixelRect(
                tile.Definition.PositionInTileset.X * tile.Definition.SizeInTileset.Width,
                tile.Definition.PositionInTileset.Y * tile.Definition.SizeInTileset.Height,
                tile.Definition.TilesetDef.TileWidth,
                tile.Definition.TilesetDef.TileHeight
            ));
            var tileImage = new Image()
            {
                Source = croppedBitmap,
                Width = tile.Definition.TilesetDef.TileWidth,
                Height = tile.Definition.TilesetDef.TileHeight
            };
            
            Canvas.SetLeft(tileImage, position.X);
            Canvas.SetTop(tileImage, position.Y);
            _drawingCanvas.Children.Add(tileImage);
        }
    }
}
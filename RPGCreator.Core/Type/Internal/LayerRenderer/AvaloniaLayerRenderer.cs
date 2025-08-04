using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Map;

namespace RPGCreator.Core.Type.Internal.LayerRenderer;

/// <summary>
/// Layer renderer for Avalonia (aka RPGCreator.UI).<br/>
/// This class is responsible for rendering tile layers in the Avalonia UI framework.<br/>
/// It implements the <see cref="ILayerRenderer"/> interface, which defines the contract for rendering layers.
/// </summary>
public class AvaloniaLayerRenderer : ILayerRenderer
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
    
    public void Draw(TileLayer tileLayer)
    {
        _drawingCanvas.Children.Clear(); // Clear the canvas before drawing

        var copyOfElements = tileLayer.Elements.ToList(); // Create a copy of the elements to avoid modifying the collection while iterating
        foreach (var element in copyOfElements)
        {
            var tile = element.Value;
            var position = element.Key;
            
            var croppedBitmap = new CroppedBitmap(tile.Tileset.GetSimpleBitmap(), new PixelRect(
                tile.PositionInTileset.X * tile.SizeInTileset.Width,
                tile.PositionInTileset.Y * tile.SizeInTileset.Height,
                tile.Tileset.TileWidth,
                tile.Tileset.TileHeight
                ));
            var tileImage = new Image()
            {
                Source = croppedBitmap,
                Width = tile.Tileset.TileWidth,
                Height = tile.Tileset.TileHeight
            };
            
            Canvas.SetLeft(tileImage, position.X);
            Canvas.SetTop(tileImage, position.Y);
            _drawingCanvas.Children.Add(tileImage);
        }
        
    }
}
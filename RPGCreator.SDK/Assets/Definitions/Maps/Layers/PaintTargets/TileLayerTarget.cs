using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.PaintTargets;

public class TileLayerTarget : IPaintTarget
{
    public List<Vector2> PreviewPosition { get; set; }
    public object? PreviewObject { get; set; }
    
    private readonly TileLayerDefinition _layerDef;
    public IMapDef? MapDef { get; }
    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }

    public TileLayerTarget(TileLayerDefinition layerDef, IMapDef map, int gridWidth, int gridHeight)
    {
        _layerDef = layerDef;
        MapDef = map;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }
    
    public bool IsValidPosition(Vector2 position)
    {
        if (MapDef == null)
            return false;
        return position is { X: >= 0, Y: >= 0 } && (position.X < MapDef.Size.Width * GridWidth && position.Y < MapDef.Size.Height * GridHeight);
    }

    public bool CanAcceptObject(object objectToPaint)
    {
        return objectToPaint is ITileDef;
    }

    public void PaintAt(Vector2 position, object objectToPaint)
    {
        if(objectToPaint is ITileDef tileDef)
            _layerDef.AddElement(tileDef, position);
    }

    public void EraseAt(Vector2 position)
    {
        _layerDef.TryRemoveElement(position, out var _);
    }

    public void PreviewAt(Vector2 position, object objectToPreview)
    {
        if (RuntimeServices.RenderService.CurrentPreviewTarget != this)
        {
            RuntimeServices.RenderService.CurrentPreviewTarget = this;
        }
        
        if (objectToPreview is ITileDef tileDef && PreviewObject != tileDef)
        {
            PreviewObject = tileDef;
        }
        
        PreviewPosition = [position];
    }
    
    public void PreviewAt(List<Vector2> positions, object objectToPreview)
    {
        if (RuntimeServices.RenderService.CurrentPreviewTarget != this)
        {
            RuntimeServices.RenderService.CurrentPreviewTarget = this;
        }
        
        if (objectToPreview is ITileDef tileDef && PreviewObject != tileDef)
        {
            PreviewObject = tileDef;
        }
        
        PreviewPosition = positions;
    }

    public void ClearPreview()
    {
        if (RuntimeServices.RenderService.CurrentPreviewTarget == this)
        {
            RuntimeServices.RenderService.CurrentPreviewTarget = null;
        }
        
        PreviewObject = null;
    }
}
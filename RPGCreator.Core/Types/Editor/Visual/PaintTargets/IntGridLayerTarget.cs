using RPGCreator.Core.Types.Editor.Interfaces;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

namespace RPGCreator.Core.Types.Editor.Visual.PaintTargets;


public class IntGridData
{
    public IntGridValueRef IntGridRef { get; set; }
    public IntGridTilesetDef IntGridTilesetDef { get; set; }
}

public class IntGridLayerTarget : IPaintTarget
{
    public int GridWidth { get; } = 32;
    public int GridHeight { get; } = 32;
    
    public AutoLayerDefinition LayerDef { get; }
    public MapDefinition? MapDef { get; }
    
    public IntGridLayerTarget(AutoLayerDefinition layerDef, MapDefinition? mapDef)
    {
        LayerDef = layerDef;
        MapDef = mapDef;
    }
    
    public bool IsValidPosition(Point position)
    {
        if (MapDef == null) return false;
        return position.X >= 0 && position.Y >= 0 && position.X < MapDef.Size.Width * GridWidth && position.Y < MapDef.Size.Height * GridHeight;
    }

    public void PaintAt(Point position, object objectToPaint)
    {
        if (objectToPaint is IntGridData gridData)
        {
            LayerDef.IntGridSet ??= gridData.IntGridTilesetDef;
            LayerDef.SourceIntGrid.AddElement(gridData.IntGridRef.Value, position);
            LayerDef.BakeRegion(position);
        }
    }

    public void EraseAt(Point position)
    {
        throw new NotImplementedException();
    }

    public void PreviewAt(Point position, object objectToPreview)
    {
        // Preview functionality can be implemented here if needed
    }
}
using RPGCreator.Core.Types.Editor.Interfaces;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

namespace RPGCreator.Core.Types.Editor.Visual.PaintTargets;


public class IntGridLayerTarget : IPaintTarget
{
    public int GridWidth { get; } = 32;
    public int GridHeight { get; } = 32;
    
    public AutoLayerDefinition LayerDef { get; }
    public IMapDef? MapDef { get; }
    
    public IntGridLayerTarget(AutoLayerDefinition layerDef, IMapDef? mapDef)
    {
        LayerDef = layerDef;
        MapDef = mapDef;
    }

    public void PaintAt(Point position, object objectToPaint)
    {
        if (objectToPaint is not IntGridData gridData) return;
        
        LayerDef.IntGridSet ??= gridData.IntGridTilesetDef;
        LayerDef.SourceIntGrid.AddElement(gridData.IntGridRef.Value, position);
        LayerDef.BakeRegion(position);
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
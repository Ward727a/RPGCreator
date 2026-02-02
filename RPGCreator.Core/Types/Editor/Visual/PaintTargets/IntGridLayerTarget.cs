using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Editor;

namespace RPGCreator.Core.Types.Editor.Visual.PaintTargets;


public class IntGridLayerTarget : IPaintTarget
{
    public int GridWidth { get; } = 32;
    public int GridHeight { get; } = 32;

    private AutoLayerDefinition LayerDef { get; }
    public IMapDef? MapDef { get; }
    
    public IntGridLayerTarget(AutoLayerDefinition layerDef, IMapDef? mapDef)
    {
        LayerDef = layerDef;
        MapDef = mapDef;
    }

    public void PaintAt(Vector2 position, object objectToPaint)
    {
        if (objectToPaint is not IntGridData gridData) return;
        
        LayerDef.IntGridSet ??= gridData.IntGridTilesetDef;
        LayerDef.SourceIntGrid.AddElement(gridData.IntGridRef.Value, position);
        LayerDef.BakeRegion(position);
    }

    public void EraseAt(Vector2 position)
    {
        throw new NotImplementedException();
    }

    public void PreviewAt(Vector2 position, object objectToPreview)
    {
        // Preview functionality can be implemented here if needed
    }
}
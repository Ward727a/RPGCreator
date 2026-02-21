using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;

namespace RPGCreator.SDK.Editor;

public interface IPaintTarget
{
    int GridWidth { get; }
    int GridHeight { get; }
    
    IMapDef? MapDef { get; }
    
    bool CanAcceptObject(object objectToPaint);
    
    void PaintAt(Vector2 position, object objectToPaint);
    void EraseAt(Vector2 position);
    void PreviewAt(Vector2 position, object objectToPreview);
}
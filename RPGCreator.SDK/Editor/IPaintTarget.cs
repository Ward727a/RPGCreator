using System.Numerics;

namespace RPGCreator.SDK.Editor;

public interface IPaintTarget
{
    int GridWidth { get; }
    int GridHeight { get; }
    
    void PaintAt(Vector2 position, object objectToPaint);
    void EraseAt(Vector2 position);
    void PreviewAt(Vector2 position, object objectToPreview);
}
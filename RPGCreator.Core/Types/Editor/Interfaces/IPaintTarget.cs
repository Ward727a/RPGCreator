using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Editor.Interfaces;

public interface IPaintTarget
{
    int GridWidth { get; }
    int GridHeight { get; }
    
    bool IsValidPosition(Point position);
    
    void PaintAt(Point position, object objectToPaint);
    void EraseAt(Point position);
    void PreviewAt(Point position, object objectToPreview);
}
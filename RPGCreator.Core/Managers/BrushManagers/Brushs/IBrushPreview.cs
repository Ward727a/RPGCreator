using System.Numerics;
using RPGCreator.SDK.Editor.Brushes;

namespace RPGCreator.Core.Managers.BrushManagers.Brushs;

public interface IBrushPreview : IBrushPreviewFeature
{
    public void ShowPreview(Vector2 at);
}
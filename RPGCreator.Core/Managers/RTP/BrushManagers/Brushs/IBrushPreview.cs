using System.Numerics;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.SDK.Editor.Brushes;

namespace RPGCreator.Core.Managers.RTP.BrushManagers.Brushs;

public interface IBrushPreview : IBrushPreviewFeature
{
    public void ShowPreview(Vector2 at);
}
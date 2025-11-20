using Avalonia.Media.Imaging;
using RPGCreator.Core.Common;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Animations;

public class AnimationDef : IHasUniqueId
{
    public Ulid Unique { get; }
    public URN Urn { get; }
    public Ulid SpriteSheetId { get; set; }
    public List<int> FrameIndexes { get; set; } = new List<int>();
    public int TotalFrames { get; private set; }
    public double FrameDuration { get; set; } = 100; // in milliseconds
}
using Avalonia.Media.Imaging;
using RPGCreator.Core.Common;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Animations;

public class AnimationDef : IAssetDef
{
    
    public Action<Ulid>? SpriteSheetIdChanged;
    
    public string Name { get; set; } = "New Animation";
    public Ulid Unique { get; }
    public URN Urn { get; set; }
    private Ulid _spriteSheetId;

    public Ulid SpriteSheetId
    {
        get => _spriteSheetId;
        set
        {
            if(_spriteSheetId == value) return;
            _spriteSheetId = value;
            SpriteSheetIdChanged?.Invoke(_spriteSheetId);
            IsDirty = true;
        }
    }

    public List<int> FrameIndexes { get; set; } = new List<int>();
    public int TotalFrames => FrameIndexes.Count;
    public double FrameDuration { get; set; } = 100; // in milliseconds
    public int Fps
    {
        get => FrameDuration > 0 ? (int)(1000 / FrameDuration) : 0;
        set => FrameDuration = value > 0 ? 1000.0 / value : 0;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
    
    public AnimationDef()
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("rpgcreator", "animation", Unique.ToString());
    }
}
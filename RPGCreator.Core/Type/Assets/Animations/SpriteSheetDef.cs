using RPGCreator.Core.Common;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Type.Assets.Animations;

public class SpriteSheetDef : IHasUniqueId
{
    public Ulid Unique { get; }
    public URN Urn { get; }
    
    public event Action<string>? SpriteSheetPathChanged;
    
    private string _spriteSheetPath = string.Empty;
    public string SpriteSheetPath
    {
        get => _spriteSheetPath;
        set {
            if (File.Exists(value))
            {
                _spriteSheetPath = value;
                _cachedImage = new UnifiedImage(SpriteSheetPath);
                var bitmap = _cachedImage.UI;
                ImageSize = new Size(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
                Hash = ShaUtil.ComputeSha256(SpriteSheetPath);
                SpriteSheetPathChanged?.Invoke(_spriteSheetPath);
            }
            else
            {
                Log.Error("SpriteSheetDef: File does not exist at path {0}", value);
            }
        }
    }
    
    public string Hash { get; private set; } = string.Empty;
    
    private UnifiedImage? _cachedImage;
    public Size ImageSize { get; private set; }
    
    public SpriteSheetDef(string spriteSheetPath)
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("rpgcreator", "spritesheet", Unique.ToString());
        SpriteSheetPath = spriteSheetPath;
    }

    public UnifiedImage Image
    {
        get {
            if(_cachedImage == null)
            {
                _cachedImage = new UnifiedImage(SpriteSheetPath);
                var bitmap = _cachedImage.UI;
                ImageSize = new Size(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
                Hash = ShaUtil.ComputeSha256(SpriteSheetPath);
            }
            return _cachedImage;
        }
    }
    
    public List<Ulid> AssociatedAnimations { get; } = new List<Ulid>();
}
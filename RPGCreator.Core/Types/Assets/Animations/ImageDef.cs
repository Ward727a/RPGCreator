using Avalonia.Media.Imaging;
using RPGCreator.Core.Common;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.Types.Assets.Animations;
/*
 * LOAD IMAGE LOGIC
 *
   _cachedImage = new UnifiedImage(SpriteSheetPath);
   var bitmap = _cachedImage.UI;
   ImageSize = new Size(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
   Hash = ShaUtil.ComputeSha256(SpriteSheetPath);
 * 
 */

public class ImageDef : IHasUniqueId
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
    private Size? _imageSize;

    public Size ImageSize
    {
        get
        {
            if (_imageSize == null)
            {
                if (_cachedImage == null)
                {
                    _cachedImage = new UnifiedImage(SpriteSheetPath);
                }
                var bitmap = _cachedImage.UI as Bitmap ?? UnifiedImage.DefaultUI;
                _imageSize = new Size(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
            }
            return _imageSize.Value;
        }
        private set => _imageSize = value;
    }
    
    public ImageDef(string spriteSheetPath)
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
                var bitmap = _cachedImage.UI as Bitmap ?? UnifiedImage.DefaultUI;
                ImageSize = new Size(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
                Hash = ShaUtil.ComputeSha256(SpriteSheetPath);
            }
            return _cachedImage;
        }
    }
    
    public List<Ulid> AssociatedAnimations { get; } = new List<Ulid>();
}
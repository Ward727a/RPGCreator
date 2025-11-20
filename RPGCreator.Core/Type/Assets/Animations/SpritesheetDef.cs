using Avalonia;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Animations;

public class SpritesheetDef : IAssetDef
{
    public Ulid Unique { get; }
    public URN Urn { get; }
    public bool IsDirty { get; set; }
    
    private ImageDef _sourceImage;

    public ImageDef SourceImage
    {
        get => _sourceImage;
        set
        {
            if(_sourceImage == value) return;
            IsDirty = true;
            _sourceImage = value;
        }
    }

    private int _frameWidth;
    private int _frameHeight;

    public int FrameWidth
    {
        get => _frameWidth;
        set
        {
            if(_frameWidth == value) return;
            IsDirty = true;
            _frameWidth = value;
        }
    }

    public int FrameHeight
    {
        get => _frameHeight;
        set
        {
            if(_frameHeight == value) return;
            IsDirty = true;
            _frameHeight = value;
        }
    }

    public int Colums => SourceImage.ImageSize.Width / FrameWidth;
    
    public PixelRect GetFrameRect(int index)
    {
        var cols = Colums;
        var x = (index % cols) * FrameWidth;
        var y = (index / cols) * FrameHeight;
        return new PixelRect(x, y, FrameWidth, FrameHeight);
    }
}
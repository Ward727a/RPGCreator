using Avalonia;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Assets.Animations;

public class SpritesheetDef : IAssetDef
{
    
    
    public Ulid Unique { get; }
    public URN Urn { get; }
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;

    public SpritesheetDef()
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("rpgcreator", "spritesheet", Unique.ToString());
    }

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
    
    public List<int> GetAllRowIndexes(int row = 0)
    {
        var cols = Colums;
        var indexes = new List<int>();
        for (int i = 0; i < cols; i++)
        {
            indexes.Add(row * cols + i);
        }
        return indexes;
    }
    
    public PixelRect GetFrameRect(int index)
    {
        var cols = Colums;
        var x = (index % cols) * FrameWidth;
        var y = (index / cols) * FrameHeight;
        return new PixelRect(x, y, 42, 64);
    }
}
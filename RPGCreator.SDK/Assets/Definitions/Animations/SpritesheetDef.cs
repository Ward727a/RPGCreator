using System.Numerics;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Helpers;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;
using Rectangle = System.Drawing.Rectangle;

namespace RPGCreator.SDK.Assets.Definitions.Animations;

[SerializingType("SpritesheetDef")]
public class SpritesheetDef : BaseAssetDef, ISerializable, IDeserializable, IHasSavePath
{
    
    private Rectangle[] _frames = null!;
    public override UrnSingleModule UrnModule => "spritesheet".ToUrnSingleModule();

    public string ImagePath { get; set; } = null!;
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }

    public int Columns;
    public int Rows;

    public Vector2 FeetOrigin { get; private set; }
    
    public SpritesheetDef()
    {
        Unique = Ulid.NewUlid();
    }

    public Rectangle GetFrameRect(int index)
    {
        if (index < 0 || index >= _frames.Length) return _frames[0];
        return _frames[index];
    }
    
    public List<int> GetAllRowIndexes(int row)
    {
        int cols = Columns;
        List<int> indexes = new List<int>();
        for (int col = 0; col < cols; col++)
        {
            indexes.Add(row * cols + col);
        }
        return indexes;
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType());
        info.AddValue("Unique", Unique);
        info.AddValue("ImagePath", ImagePath);
        info.AddValue("ImageWidth", ImageWidth);
        info.AddValue("ImageHeight", ImageHeight);
        info.AddValue("FrameWidth", FrameWidth);
        info.AddValue("FrameHeight", FrameHeight);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("Unique", out Ulid unique, Ulid.Empty);
        Unique = unique;
        info.TryGetValue("ImagePath", out string imagePath, string.Empty);
        ImagePath = imagePath;
        info.TryGetValue("ImageWidth", out int imageWidth, 0);
        ImageWidth = imageWidth;
        info.TryGetValue("ImageHeight", out int imageHeight, 0);
        ImageHeight = imageHeight;
        info.TryGetValue("FrameWidth", out int frameWidth, 0);
        FrameWidth = frameWidth;
        info.TryGetValue("FrameHeight", out int frameHeight, 0);
        FrameHeight = frameHeight;

        CalculateValues();
    }

    /// <summary>
    /// This method calculates derived values such as FeetOrigin, Columns, Rows, and frame rectangles based on the current properties.
    /// </summary>
    public void CalculateValues()
    {
        if (!string.IsNullOrEmpty(ImagePath) && (ImageWidth <= 0 || ImageHeight <= 0))
        {
            TryResolveDimensions();
        }
        
        FeetOrigin = new Vector2(FrameWidth / 2f, FrameHeight);
        Columns = FrameWidth > 0 ? ImageWidth / FrameWidth : 1;
        Rows = FrameHeight > 0 ? ImageHeight / FrameHeight : 1;
        
        _frames = new Rectangle[Columns * Rows];
        for (int i = 0; i < _frames.Length; i++)
        {
            _frames[i] = new Rectangle((i % Columns) * FrameWidth, (i / Columns) * FrameHeight, FrameWidth, FrameHeight);
        }
    }

    public void TryResolveDimensions()
    {
        try
        {
            if (!File.Exists(ImagePath)) return;
            
            var info = ImageHelper.GetImageDimensions(ImagePath);
                
            ImageWidth = info.Width;
            ImageHeight = info.Height;
            
        } catch (Exception ex)
        {
            Logger.Error("Failed to resolve image dimensions for SpritesheetDef: {Urn} ({path}) | Exception: @{ex}", Urn, ImagePath, ex);
        }
    }

    public string SavePath { get; set; } = null!;
}
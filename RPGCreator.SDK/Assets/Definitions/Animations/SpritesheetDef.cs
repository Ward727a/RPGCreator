using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK.Assets.Definitions.Animations;

[SerializingType("SpritesheetDef")]
public class SpritesheetDef : IAssetDef, ISerializable, IDeserializable
{
    
    
    public Ulid Unique { get; private set; }
    public URN Urn { get; private set; }
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;

    public string ImagePath { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    
    public int Columns => FrameWidth > 0 ? ImageWidth / FrameWidth : 1;
    public int Rows => FrameHeight > 0 ? ImageHeight / FrameHeight : 1;
    
    public SpritesheetDef()
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("rpgcreator", "spritesheet", Unique.ToString());
    }

    public Rect GetFrameRect(int index)
    {
        int cols = Columns;
        int x = (index % cols) * FrameWidth;
        int y = (index / cols) * FrameHeight;
        return new Rect(x, y, FrameWidth, FrameHeight);
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

        Urn = new URN("rpgcreator", "spritesheet", Unique.ToString());
    }
}
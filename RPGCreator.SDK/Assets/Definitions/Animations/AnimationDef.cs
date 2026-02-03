using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Animations;

[SerializingType("AnimationDef")]
public class AnimationDef : IAssetDef, ISerializable, IDeserializable
{
    
    public string Name { get; set; } = "New Animation";
    public Ulid Unique { get; private set; }
    public URN Urn { get; set; }
    private Ulid _spritesheetId;
    public bool Loop { get; set; } = false;

    public Ulid SpritesheetId
    {
        get => _spritesheetId;
        set
        {
            if(_spritesheetId == value) return;
            _spritesheetId = value;
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

    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType());
        info.AddValue("Unique", Unique);
        info.AddValue("Name", Name);
        info.AddValue("SpriteSheetId", SpritesheetId);
        info.AddValue("FrameIndexes", FrameIndexes);
        info.AddValue("FrameDuration", FrameDuration);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("Unique", out Ulid unique, Ulid.Empty);
        Unique = unique;
        Urn = new URN("rpgcreator", "animation", Unique.ToString());
        info.TryGetValue("Name", out string name, "New Animation");
        Name = name;
        info.TryGetValue("SpriteSheetId", out Ulid spriteSheetId, Ulid.Empty);
        SpritesheetId = spriteSheetId;
        info.TryGetValue("FrameIndexes", out List<int> frameIndexes, new List<int>());
        FrameIndexes = frameIndexes;
        info.TryGetValue("FrameDuration", out double frameDuration, 100);
        FrameDuration = frameDuration;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Animations;

[SerializingType("AnimationDef")]
public partial class AnimationDef : BaseObservableAssetDef, ISerializable, IDeserializable, IHasSavePath
{
    [ObservableProperty]
    private Ulid _spritesheetId;

    [ObservableProperty] 
    private bool _loop = true;

    public List<int> FrameIndexes { get; set; } = new List<int>();
    public int TotalFrames => FrameIndexes.Count;
    public double FrameDuration { get; set; } = 100; // in milliseconds
    public int Fps
    {
        get => FrameDuration > 0 ? (int)(1000 / FrameDuration) : 0;
        set => FrameDuration = value > 0 ? 1000.0 / value : 0;
    }
    
    public AnimationDef()
    {
    }

    public override UrnSingleModule UrnModule => "animation".ToUrnSingleModule();

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

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        if (SpritesheetId != Ulid.Empty)
            referencedIds.Add(SpritesheetId);
        return referencedIds;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("Unique", out Ulid unique, Ulid.Empty);
        Unique = unique;
        info.TryGetValue("Name", out string name, "New Animation");
        Name = name;
        info.TryGetValue("SpriteSheetId", out Ulid spriteSheetId, Ulid.Empty);
        SpritesheetId = spriteSheetId;
        info.TryGetValue("FrameIndexes", out List<int> frameIndexes, new List<int>());
        FrameIndexes = frameIndexes;
        info.TryGetValue("FrameDuration", out double frameDuration, 100);
        FrameDuration = frameDuration;
    }

    public string SavePath { get; set; } = null!;
}
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Types.Assets.Characters.Stats;

public class StatModifier : IStatModifier
{
    public Ulid Unique { get; set; }
    public object? Source { get; set; }
    public int Priority { get; set; }
    public EStatModifierMode Mode { get; set; }
    public float Value { get; set; }
    public Ulid StatsUnique { get; set; }
    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(StatModifier))
            .AddValue("Unique", Unique)
            // .AddValue("Source", Source) // Run-time object, not serializable (Just added here to explain why it's not included)
            .AddValue("Priority", Priority)
            .AddValue("Mode", Mode)
            .AddValue("Value", Value)
            .AddValue("StatsUnique", StatsUnique);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        if (StatsUnique != Ulid.Empty) referencedIds.Add(StatsUnique);
        return referencedIds;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        
        info.TryGetValue("Unique", out var unique, Ulid.Empty);
        Unique = unique;
        // The Source property is not serializable as it's a run-time object, so we skip it. (Just added here to explain why it's not included)
        // If you need to serialize the source, you should implement a custom serialization logic for it (Good luck with that!).
        // info.TryGetValue("Source", out var source, null, "StatModifier.Source not found, using default value null.");
        // Source = source;
        info.TryGetValue("Priority", out var priority, 0);
        Priority = priority;
        info.TryGetValue("Mode", out var mode, EStatModifierMode.Flat);
        Mode = mode;
        info.TryGetValue("Value", out var value, 0f);
        Value = value;
        info.TryGetValue("StatsUnique", out var statsUnique, Ulid.Empty);
        StatsUnique = statsUnique;
    }
}
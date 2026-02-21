using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Stats;

public abstract class BaseStatDefinition : BaseAssetDef, IStatDef
{
    // Note: We use JsonExtensionData to store any "old" or "extra" data that might be present in the JSON but is not defined in the current version of the class, to avoid losing data when deserializing and re-serializing with a newer version of the class.
    [System.Text.Json.Serialization.JsonExtensionData]
    public IDictionary<string, JToken> ExtraData { get; set; }
    public string SavePath { get; set; } = null!;
    public Ulid? PackId { get; set; }

    public string DisplayName => string.IsNullOrEmpty(Name) ? $"Stat_{Unique}" : Name;
    
    public abstract string Description { get; set; }
    public abstract double DefaultValue { get; set; }
    
    /// <summary>
    /// Whether the stat can have negative values or not, used to determine if the stat can go below zero or not.
    /// </summary>
    public bool CanBeNegative { get; set; }
    public EStatTypeKind TypeKind { get; set; }
    public abstract double MinValue { get; set; }
    public StatCapSettings CapSettings { get; set; }
    public bool IsVisible { get; set; }

    [JsonConstructor]
    protected BaseStatDefinition(Ulid unique) : this()
    {
        Unique = unique;
    }
    
    public BaseStatDefinition()
    {
        SuspendTracking();
        Unique = Ulid.NewUlid();
        Name = "New Stat";
        Description = string.Empty;
        DefaultValue = 0d;
        TypeKind = EStatTypeKind.Resource;
        MinValue = 0d;
        CapSettings = new StatCapSettings();
        IsVisible = true;
    }
    
    public IPrattFormula? StatCompiledFormula { get; set; }
    public string StatNonCompiledFormula { get; set; } = string.Empty;
    private readonly Dictionary<string, IGraphScript> _statGraphEvents = new();

    public virtual void AddEvent(string eventName, IGraphScript eventDocumentCompiled)
    {
        _statGraphEvents[eventName] = eventDocumentCompiled;
    }

    public virtual bool TryGetEvent(string eventName, out IGraphScript? eventCompiled)
    {
        return _statGraphEvents.TryGetValue(eventName, out eventCompiled);
    }
    public virtual Dictionary<string, IGraphScript> GetAllEvents()
    {
        return new Dictionary<string, IGraphScript>(_statGraphEvents);
    }

    public virtual SerializationInfo GetObjectData()
    {
        return new SerializationInfo(this.GetType())
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Description), Description)
            .AddValue(nameof(DefaultValue), DefaultValue)
            .AddValue(nameof(TypeKind), TypeKind)
            .AddValue(nameof(MinValue), MinValue)
            .AddValue(nameof(CapSettings), CapSettings)
            .AddValue(nameof(StatNonCompiledFormula), StatNonCompiledFormula)
            .AddValue(nameof(_statGraphEvents), _statGraphEvents.ToDictionary(kv => kv.Key, kv => kv.Value.DocumentPath));
    }

    public virtual List<Ulid> GetReferencedAssetIds()
    {
        return [CapSettings.CapStatUnique];
    }

    public virtual void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.TryGetValue(nameof(Unique), out var unique, Ulid.Empty);
        Unique = unique;
        info.TryGetValue(nameof(Name), out var name, string.Empty);
        Name = name;
        info.TryGetValue(nameof(Description), out var description, string.Empty);
        Description = description;
        info.TryGetValue(nameof(DefaultValue), out var defaultValue, 0f);
        DefaultValue = defaultValue;
        info.TryGetValue(nameof(TypeKind), out var statTypeKind, EStatTypeKind.Resource);
        TypeKind = statTypeKind;
        info.TryGetValue(nameof(MinValue), out var statMinValue, 0f);
        MinValue = statMinValue;
        info.TryGetValue(nameof(CapSettings), out var statCapSettings, new StatCapSettings());
        CapSettings = statCapSettings;
        info.TryGetValue(nameof(StatNonCompiledFormula), out var statNonCompiledFormula, string.Empty);
        StatNonCompiledFormula = statNonCompiledFormula;
        info.TryGetValue(nameof(_statGraphEvents), out Dictionary<string, string> statGraphEventsPaths, new());
        _statGraphEvents.Clear();
        foreach (var kv in statGraphEventsPaths)
        {
            if(EngineServices.GraphService.TryLoadScript(kv.Value, out var script))
            {
                _statGraphEvents[kv.Key] = script;
            }
            else
            {
                Logger.Error("[StatDefinition] Failed to load graph script for event '{EventName}' at path '{DocumentPath}'.", kv.Key, kv.Value);
            }
        }
    }
}
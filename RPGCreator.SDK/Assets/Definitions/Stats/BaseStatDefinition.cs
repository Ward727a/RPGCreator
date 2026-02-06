using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Stats;

public abstract class BaseStatDefinition : IStatDef
{
    public string SavePath { get; set; }
    public Ulid? PackId { get; set; }
    public Ulid Unique { get; private set; }
    public URN Urn { get; private set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double DefaultValue { get; set; }
    public EStatTypeKind StatTypeKind { get; set; }
    public double StatMinValue { get; set; }
    public CapSettings StatCapSettings { get; set; }
    public bool IsVisible { get; set; }

    public BaseStatDefinition()
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("entity_stat", $"{Unique}");
        Name = string.Empty;
        Description = string.Empty;
        DefaultValue = 0d;
        StatTypeKind = EStatTypeKind.Resource;
        StatMinValue = 0d;
        StatCapSettings = new CapSettings();
        IsVisible = true;
    }

    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
        Urn = new URN("entity_stat", $"{Unique}");
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
            .AddValue(nameof(StatTypeKind), StatTypeKind)
            .AddValue(nameof(StatMinValue), StatMinValue)
            .AddValue(nameof(StatCapSettings), StatCapSettings)
            .AddValue(nameof(StatNonCompiledFormula), StatNonCompiledFormula)
            .AddValue(nameof(_statGraphEvents), _statGraphEvents.ToDictionary(kv => kv.Key, kv => kv.Value.DocumentPath));
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
        info.TryGetValue(nameof(StatTypeKind), out var statTypeKind, EStatTypeKind.Resource);
        StatTypeKind = statTypeKind;
        info.TryGetValue(nameof(StatMinValue), out var statMinValue, 0f);
        StatMinValue = statMinValue;
        info.TryGetValue(nameof(StatCapSettings), out var statCapSettings, new CapSettings());
        StatCapSettings = statCapSettings;
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

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
}
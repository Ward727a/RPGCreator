using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Parser.PRATT;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Assets.Characters.Stats;

public class StatDefinition : IStatDef
{
    public string SavePath { get; set; }
    public Ulid? PackId { get; set; }
    public Ulid Unique { get; private set; }
    public URN Urn { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float DefaultValue { get; set; }
    public EStatTypeKind StatTypeKind { get; set; }
    public float StatMinValue { get; set; }
    public EStatTypeCap StatCapType { get; set; }
    public float StatCapValue { get; set; }
    public Ulid? StatCapStatUnique { get; set; }
    public bool IsVisible { get; set; }
    
    public StatDefinition()
    {
        Unique = Ulid.NewUlid();
        Urn = new URN("stat", $"{Unique}");
        Name = string.Empty;
        Description = string.Empty;
        DefaultValue = 0f;
        StatTypeKind = EStatTypeKind.Resource;
        StatMinValue = 0f;
        StatCapType = EStatTypeCap.ByValue;
        StatCapValue = 0f;
        StatCapStatUnique = null;
        IsVisible = true;
    }
    
    public PrattCompiledFormula? StatCompiledFormula { get; set; }
    public string StatNonCompiledFormula { get; set; } = string.Empty;
    private readonly Dictionary<string, GraphDocumentCompiled> _statGraphEvents = new();
    public void AddEvent(string eventName, GraphDocumentCompiled eventDocumentCompiled)
    {
        _statGraphEvents[eventName] = eventDocumentCompiled;
    }
    public bool TryGetEvent(string eventName, out GraphDocumentCompiled? eventCompiled)
    {
        return _statGraphEvents.TryGetValue(eventName, out eventCompiled);
    }
    public Dictionary<string, GraphDocumentCompiled> GetAllEvents()
    {
        return new Dictionary<string, GraphDocumentCompiled>(_statGraphEvents);
    }

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(StatDefinition))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Description), Description)
            .AddValue(nameof(DefaultValue), DefaultValue)
            .AddValue(nameof(StatTypeKind), StatTypeKind)
            .AddValue(nameof(StatMinValue), StatMinValue)
            .AddValue(nameof(StatCapType), StatCapType)
            .AddValue(nameof(StatCapValue), StatCapValue)
            .AddValue(nameof(StatCapStatUnique), StatCapStatUnique)
            .AddValue(nameof(StatNonCompiledFormula), StatNonCompiledFormula)
            .AddValue(nameof(_statGraphEvents), _statGraphEvents.ToDictionary(kv => kv.Key, kv => kv.Value.DocumentPath));
    }

    public void SetObjectData(Serializer.DeserializationInfo info)
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
        info.TryGetValue(nameof(StatCapType), out var statCapType, EStatTypeCap.ByValue);
        StatCapType = statCapType;
        info.TryGetValue(nameof(StatCapValue), out var statCapValue, 0f);
        StatCapValue = statCapValue;
        info.TryGetValue(nameof(StatCapStatUnique), out var statCapStatUnique, null as Ulid?);
        StatCapStatUnique = statCapStatUnique;
        info.TryGetValue(nameof(StatNonCompiledFormula), out var statNonCompiledFormula, string.Empty);
        StatNonCompiledFormula = statNonCompiledFormula;
        info.TryGetValue(nameof(_statGraphEvents), out Dictionary<string, string> statGraphEventsPaths, new());
        _statGraphEvents.Clear();
        foreach (var kv in statGraphEventsPaths)
        {
            var graphDocumentData = File.ReadAllText(kv.Value);
            EngineSerializer.Instance.Deserialize(graphDocumentData, out GraphDocument o, out var type);

            if (o is GraphDocument graphDocument)
            {
                graphDocument.SavePath = kv.Value;
                var graphDocumentCompiled = GraphDocumentCompiler.Compile(graphDocument);
                _statGraphEvents[kv.Key] = graphDocumentCompiled;
            }
        }
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
}
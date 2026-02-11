using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Stats;

public interface IStatDef : IBaseAssetDef, ISerializable, IDeserializable, IHasSavePath
{
    public const string OnValueChangedEvent = "OnValueChanged";
    
    /// <summary>
    /// The pack identifier that this stat belongs to.
    /// </summary>
    public Ulid? PackId { get; set; }
    
    /// <summary>
    /// The description of the stat, used for display purposes.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// The default value of the stat, used when the stat is not modified by any modifiers.
    /// </summary>
    public double DefaultValue { get; set; }
    
    /// <summary>
    /// Whether the stat can have negative values or not, used to determine if the stat can go below zero or not.
    /// </summary>
    public bool CanBeNegative { get; set; }
    
    /// <summary>
    /// Define the kind of this stat:<br/>
    /// <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Resource"/> for health, mana, etc.<br/>
    /// <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Attribute"/> for strength, agility, etc.<br/>
    /// <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Derived"/> for stats that are calculated based via other stats).
    /// </summary>
    public EStatTypeKind TypeKind { get; set; }
    
    /// <summary>
    /// Define the minimum value of the stat, used to prevent the stat from going below a certain threshold.
    /// </summary>
    public double MinValue { get; set; }
    
    /// <summary>
    /// Stat capping settings, used to define how the stat is capped, either by a fixed value or by another stat.<br/>
    /// This is used to prevent the stat from exceeding a certain threshold, either by a fixed value or by another stat.
    /// </summary>
    public StatCapSettings CapSettings { get; set; }

    /// <summary>
    /// If the stat is visible in the game, such as in the UI or in a character sheet.<br/>
    /// This is used to determine if the stat should be displayed to the player or not.
    /// </summary>
    public bool IsVisible { get; set; }
    
    /// <summary>
    /// The formula used to calculate the stat value, if applicable.<br/>
    /// It can be used to define how the stat value is calculated based on other stats or conditions.<br/>
    /// For example, a derived stat like "Attack Power" could be calculated as a formula based on the character's strength and agility stats.<br/>
    /// like "AttackPower = (Strength * 1.5) + (Agility * 0.5)" or similar expressions.
    /// </summary>
    public IPrattFormula? StatCompiledFormula { get; set; }

    /// <summary>
    /// The non-compiled formula used to calculate the stat value, if applicable (<see cref="TypeKind"/> == <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Derived"/>).<br/>
    /// This is a string representation of the formula that can be parsed and compiled later.<br/>
    /// It can be used to define how the stat value is calculated based on other stats or conditions.<br/>
    /// For example, a derived stat like "Attack Power" could be defined as a non-compiled formula like "AttackPower = (Strength * 1.5) + (Agility * 0.5)" or similar expressions.
    /// </summary>
    public string StatNonCompiledFormula { get; set; }

    public abstract void AddEvent(string eventName, IGraphScript eventDocumentCompiled);
    public abstract bool TryGetEvent(string eventName, out IGraphScript? eventCompiled);

    public abstract Dictionary<string, IGraphScript> GetAllEvents();
}

public record struct StatCapSettings()
{
    /// <summary>
    /// Define how the stat is capped, either by a fixed value or by another stat.
    /// </summary>
    public EStatTypeCap CapType { get; set; } = EStatTypeCap.ByValue;

    /// <summary>
    /// If <see cref="CapType"/> is <see cref="EStatTypeCap.ByValue"/> then this value is used to cap the stat.
    /// Else, check <see cref="CapStatUnique"/> if <see cref="CapType"/> is <see cref="EStatTypeCap.ByStat"/>.
    /// </summary>
    public double CapValue { get; set; } = 0d;

    /// <summary>
    /// If <see cref="CapType"/> is <see cref="EStatTypeCap.ByStat"/>, then this value is used to reference the stat that will cap this stat.<br/>
    /// Else, check <see cref="CapValue"/> if <see cref="CapType"/> is <see cref="EStatTypeCap.ByValue"/>.
    /// </summary>
    public Ulid CapStatUnique { get; set; } = Ulid.Empty;
}
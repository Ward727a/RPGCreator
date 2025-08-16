using RPGCreator.Core.Parser.PRATT;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Characters.Stats;

public interface IStatDef : ISerializable, IDeserializable, IHasUniqueId
{
    /// <summary>
    /// The unique identifier for the stat type used for identification.
    /// </summary>
    public Ulid Unique { get; }
    /// <summary>
    /// The name of the stat, used for display purposes.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The description of the stat, used for display purposes.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The default value of the stat, used when the stat is not modified by any modifiers.
    /// </summary>
    public float DefaultValue { get; set; }
    /// <summary>
    /// Define the kind of this stat:<br/>
    /// <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Resource"/> for health, mana, etc.<br/>
    /// <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Attribute"/> for strength, agility, etc.<br/>
    /// <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Derived"/> for stats that are calculated based via other stats).
    /// </summary>
    public EStatTypeKind StatTypeKind { get; set; }
    /// <summary>
    /// Define the minimum value of the stat, used to prevent the stat from going below a certain threshold.
    /// </summary>
    public float StatMinValue { get; set; }
    /// <summary>
    /// Define how the stat is capped, either by a fixed value or by another stat.
    /// </summary>
    public EStatTypeCap StatCapType { get; set; }
    /// <summary>
    /// If <see cref="StatCapType"/> is <see cref="EStatTypeCap.ByValue"/> then this value is used to cap the stat.
    /// Else, check <see cref="StatCapStatUnique"/> if <see cref="StatCapType"/> is <see cref="EStatTypeCap.ByStat"/>.
    /// </summary>
    public float StatCapValue { get; set; }
    /// <summary>
    /// If <see cref="StatCapType"/> is <see cref="EStatTypeCap.ByStat"/>, then this value is used to reference the stat that will cap this stat.<br/>
    /// Else, check <see cref="StatCapValue"/> if <see cref="StatCapType"/> is <see cref="EStatTypeCap.ByValue"/>.
    /// </summary>
    public Ulid? StatCapStatUnique { get; set; }
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
    public PrattCompiledFormula? StatCompiledFormula { get; set; }

    /// <summary>
    /// The non-compiled formula used to calculate the stat value, if applicable (<see cref="StatTypeKind"/> == <see cref="EStatTypeKind"/>.<see cref="EStatTypeKind.Derived"/>).<br/>
    /// This is a string representation of the formula that can be parsed and compiled later.<br/>
    /// It can be used to define how the stat value is calculated based on other stats or conditions.<br/>
    /// For example, a derived stat like "Attack Power" could be defined as a non-compiled formula like "AttackPower = (Strength * 1.5) + (Agility * 0.5)" or similar expressions.
    /// </summary>
    public string statNonCompiledFormula { get; set; }
}
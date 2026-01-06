using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Assets.Definitions.Stats;

public interface IStatModifier : ISerializable, IDeserializable
{
    /// <summary>
    /// The unique identifier of the modifier.
    /// </summary>
    public Ulid Unique { get; set; }
    /// <summary>
    /// The source of the modifier, can be an item, effect, or any other object.
    /// </summary>
    public object? Source { get; set; }
    /// <summary>
    /// The priority of the modifier, used to determine the order of application.<br/>
    /// Higher values are applied first, lower values are applied later.
    /// </summary>
    public int Priority { get; set; }
    /// <summary>
    /// The mode of the modifier, determines how the value is applied to the stat.<br/>
    /// If the mode is <see cref="EStatModifierMode.Override"/>, the value will override the base stat.<br/>
    /// If the mode is <see cref="EStatModifierMode.Percentage"/>, the value will be multiplied by the base stat.<br/>
    /// If the mode is <see cref="EStatModifierMode.Flat"/>, the value will be added to the base stat.
    /// </summary>
    public EStatModifierMode Mode { get; set; }
    /// <summary>
    /// The value of the modifier, can be positive or negative.<br/>
    /// Positive values will increase the stat, negative values will decrease it.
    /// </summary>
    public float Value { get; set; }
    /// <summary>
    /// The unique identifier of the stat that will be modified by this modifier.<br/>
    /// This is used to identify which stat the modifier applies to, allowing for multiple modifiers to be applied to different stats on the same character.
    /// </summary>
    public Ulid StatsUnique { get; set; }
}
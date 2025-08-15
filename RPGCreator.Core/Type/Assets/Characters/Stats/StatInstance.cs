namespace RPGCreator.Core.Type.Assets.Characters.Stats;

public sealed class StatInstance
{
    /// <summary>
    /// Define the unique identifier of the stat type.
    /// </summary>
    public readonly Ulid RuntimeId;
    /// <summary>
    /// Define the unique identifier of the stat definition in the game resources.<br/>
    /// This is the one that will be used by this stat type.
    /// </summary>
    public readonly Ulid StatDefinitionId;
    /// <summary>
    /// The base value of the stat.<br/>
    /// This value is the one that will be used to calculate the current value of the stat.<br/>
    /// It is the value that will be modified by modifiers, such as items, effects, or any other object that can modify the stat.<br/>
    /// BUT it should not be modified by any modifier, as it is the base value of the stat. (Except for resources based stats, such as health or mana, which can be modified by effects or items that restore or consume resources.)
    /// </summary>
    public float BaseValue;
    /// <summary>
    /// The current value of the stat.<br/>
    /// This value is the one that will be used by the character to perform actions, such as attacking, defending, or casting spells.<br/>
    /// It can be modified by modifiers, such as items, effects, or any other object that can modify the stat.
    /// </summary>
    public float CurrentValue { get; private set; }
}
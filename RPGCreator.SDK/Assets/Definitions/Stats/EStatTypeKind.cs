namespace RPGCreator.SDK.Assets.Definitions.Stats;

public enum EStatTypeKind
{
    /// <summary>
    /// Represents a stat that is defined in the game resources, such as health, or mana.
    /// </summary>
    Resource,
    /// <summary>
    /// Represents a stat that is defined by the game, such as strength, or agility.
    /// </summary>
    Attribute,
    /// <summary>
    /// Represents a stat that is defined by another stat, such as a "speed" stat that is : (Agility*1.5 + Strength*0.5) * Health.
    /// </summary>
    Derived
}
namespace RPGCreator.SDK.Assets.Definitions.Stats;

public enum EStatModifierMode
{
    /// <summary>
    /// Just add the value to the base stat.
    /// </summary>
    Flat,
    /// <summary>
    /// Multiply the base stat by the value.
    /// </summary>
    Percentage,
    /// <summary>
    /// Set the stat to the value, overriding any previous modifications.
    /// </summary>
    Override
}
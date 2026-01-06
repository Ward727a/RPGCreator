namespace RPGCreator.SDK.Assets.Definitions.Stats;

public enum EStatTypeCap
{
    /// <summary>
    /// If the stat is capped by a fixed value, such as a "health" stat that is capped by 1000.<br/>
    /// This is the most common type of cap, as it is used for most stats in game.
    /// </summary>
    ByValue,
    /// <summary>
    /// If the stat is capped following another stat, such as a "health" stat that is capped by "maxHealth" stat.
    /// </summary>
    ByStat,
}
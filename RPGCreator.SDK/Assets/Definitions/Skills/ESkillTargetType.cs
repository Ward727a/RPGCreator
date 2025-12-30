namespace RPGCreator.SDK.Assets.Definitions.Skills;

public enum ESkillTargetType
{
    /// <summary>
    /// A single ENEMY target
    /// </summary>
    Single,
    /// <summary>
    /// A group of ENEMY targets
    /// </summary>
    AoE,
    /// <summary>
    /// Self target (the caster)
    /// </summary>
    Self,
    /// <summary>
    /// A single ALLY target
    /// </summary>
    Party,
    /// <summary>
    /// A group of ALLY targets
    /// </summary>
    AoEParty
}
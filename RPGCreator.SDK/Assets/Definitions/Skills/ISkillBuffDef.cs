using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.SDK.Assets.Definitions.Skills;

public interface ISkillBuffDef : ISkillDef
{
    string Group { get; set; } // Group or category of the buff (e.g., "Poison", "Stun", "Heal Over Time")
    
    float Duration { get; set; } // Duration of the buff in seconds
    int MaxStacks { get; set; } // Maximum number of stacks for the buff
    bool IsDebuff { get; set; } // Indicates if the buff is a debuff (negative effect)
    
    /// <summary>
    /// Ulid => Stat unique identifier
    /// </summary>
    Dictionary<Ulid, IPrattFormula> StatModifiers { get; set; } // Stat modifiers applied by the buff
    Dictionary<Ulid, string> StatNonCompiledFormulas { get; set; } // Non-compiled formulas for stat modifiers
    
    float TickInterval { get; set; } // Interval in seconds/turn for periodic effects (0 for instant effects, 1 for every second/turn, 2 for every 2 seconds/turns, etc.)
    EBuffStackBehavior StackBehavior { get; set; } // Behavior when applying a buff that is already active (e.g., refresh duration, increase stacks, etc.)
    
    bool IsHidden { get; set; } // If true, the buff will not be shown in the UI
    
    List<string> ExclusiveGroups { get; set; } // List of buff groups that cannot coexist with this buff
    List<Ulid> ExclusiveBuffs { get; set; } // List of specific buffs that cannot coexist with this buff
}
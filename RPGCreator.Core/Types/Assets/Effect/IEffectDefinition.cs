using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Types.Assets.Effect;

public interface IEffectDefinition : ISerializable, IDeserializable
{
    /// <summary>
    /// Unique identifier for the effect.
    /// </summary>
    public Ulid Unique { get; }
    /// <summary>
    /// Define the name of the effect.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Define the description of the effect.<br/>
    /// This is used to provide additional information about the effect, such as its purpose or how it works.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// Define the tags associated with the effect.<br/>
    /// Tags are used to categorize effects and can be used for filtering or searching purposes.<br/>
    /// For example, an effect might have tags like "damage", "healing", "buff", "debuff", etc.
    /// </summary>
    public List<string> Tags { get; }
    /// <summary>
    /// Define what the effect does.<br/>
    /// This is a list of <see cref="StatModifier"/> that will be applied to the target when the effect is applied.<br/>
    /// Each modifier can affect a specific stat, and can be configured to modify the stat in different ways (e.g., flat addition, percentage increase, or override the stat).
    /// </summary>
    List<IStatModifier> Modifiers { get; }
    /// <summary>
    /// Defines the type of effect this is.<br/>
    /// If the effect is <see cref="EEffectTimeType.Instant"/>, it will be applied immediately and then removed.<br/>
    /// If the effect is <see cref="EEffectTimeType.Definitive"/>, it will be applied and remain until removed.<br/>
    /// If the effect is <see cref="EEffectTimeType.Until"/>, it will be applied and removed after a certain duration.
    /// </summary>
    EEffectTimeType TimeType { get; }
    /// <summary>
    /// The duration of the effect in seconds. <br/>
    /// If the effect is either instant or permanent, this value is ignored.
    /// </summary>
    float Duration { get; }
    /// <summary>
    /// The period of the effect in seconds.<br/>
    /// This is only relevant if the effect is of type <see cref="EEffectTimeType.Until"/>.<br/>
    /// It defines how often the effect will be applied during its duration.<br/>
    /// For example, if the period is set to 2 seconds, the effect will be applied every 2 seconds until the duration expires.
    /// </summary>
    float Period { get; }
    /// <summary>
    /// Defines how the effect manages its own stacking.<br/>
    /// If the stacking policy is set to <see cref="EEffectStackingPolicy.Stack"/>, the same effect can stack multiple times on the same target.<br/>
    /// If the stacking policy is set to <see cref="EEffectStackingPolicy.Refresh"/>, the effect will refresh its duration each time it is applied to the same target.<br/>
    /// If the stacking policy is set to <see cref="EEffectStackingPolicy.Ignore"/>, the effect will not stack nor refresh its duration, it will simply be ignored if it is already applied to the target.
    /// </summary>
    EEffectStackingPolicy StackingPolicy { get; }
    /// <summary>
    /// Defines the maximum number of stacks this effect can have.<br/>
    /// This is only relevant if the stacking policy is set to <see cref="EEffectStackingPolicy.Stack"/>.<br/>
    /// If the stacking policy is set to any other value, this value is ignored.
    /// </summary>
    int MaxStacks { get; }
    /// <summary>
    /// Defines the priority of the effect.<br/>
    /// This is used to determine the order in which effects are applied when multiple effects are applied to the same target.<br/>
    /// Higher priority effects will be applied first, and lower priority effects will be applied later.
    /// </summary>
    int Priority { get; }
    /// <summary>
    /// Defines effects tags that are incompatible with this effect.<br/>
    /// If an effect is applied to a target that already has any effect with one of these tags, the new effect will not be applied.
    /// </summary>
    List<string> NonCompatibleTags { get; }
    /// <summary>
    /// Defines effects tags that will replace the existing effects on the target.<br/>
    /// If an effect is applied to a target that already has any effect with one of these tags, all the existing effects with those tags will be removed and replaced with the new effect.<br/>
    /// This is useful for effects that should replace existing effects, such as a stronger version of an effect or a different type of effect that serves a similar purpose.
    /// </summary>
    List<string> ReplaceTags { get; }
}
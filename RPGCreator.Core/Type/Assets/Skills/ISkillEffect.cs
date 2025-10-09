using MonoGame.Extended.ECS;
using RPGCreator.Core.Runtimes;
using RPGCreator.Core.Type.Assets.Actors;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Skills;

/// <summary>
/// Native C# implementation of a skill effect.
/// </summary>
public interface ISkillEffect : IHasUniqueId, ICloneable
{
    public string DisplayName { get; }
    /// <summary>
    /// Custom properties for the skill effect.<br/>
    /// It can be used to allow users to configure the effect in the editor.<br/>
    /// The properties can be of any type, but it's recommended to use simple types like int, float, string, bool, etc.<br/>
    /// The properties can be accessed in the ApplyEffect method to modify the behavior of the effect.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; }
    
    static IReadOnlyList<SkillEffectPropertyDescriptor> PropertyDescriptors { get; } = new List<SkillEffectPropertyDescriptor>();
    
    public void ApplyEffect(IEntity caster, List<IEntity> target);
}
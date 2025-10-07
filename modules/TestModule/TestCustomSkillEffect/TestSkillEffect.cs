using MonoGame.Extended.ECS;
using RPGCreator.Core.Runtimes;
using RPGCreator.Core.Runtimes.Contents;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Internal;

namespace TestModule.TestCustomSkillEffect;

[SkillEffect]
public class TestSkillEffect : ISkillEffect
{
    public Ulid Unique { get; } = Ulid.NewUlid();
    public URN Urn { get; } = new URN("test_module", "skill_effect","test_skill_effect");
    public Dictionary<string, object> Properties { get; set; }
    public void ApplyEffect(IEntity caster, List<IEntity> target)
    {
        // Example effect: Log the application of the effect
        Console.WriteLine($"TestSkillEffect applied by caster {caster.Id} to {target.Count} targets.");
    }

}
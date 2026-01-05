using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Modules.Attributes;
using RPGCreator.SDK.Types;
using Entity = RPGCreator.SDK.ECS.Entities.Entity;

namespace TestModule.TestCustomSkillEffect;

[SkillEffect]
public class TestSkillEffect : ISkillEffect
{
    public Ulid Unique { get; } = Ulid.NewUlid();
    public URN Urn { get; } = new URN("test_module", "skill_effect","test_skill_effect");
    public string DisplayName { get; } = "Test Skill Effect";

    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>()
    {
        {
            "TestNumber", 0
        },
        {
            "TestString", "default"
        },
        {
            "TestBoolean", false
        },
        {
            "TestVector2/Point", Vector2.Zero
        }
    };

    public static IReadOnlyList<SkillEffectPropertyDescriptor> PropertyDescriptors { get; } =
        new List<SkillEffectPropertyDescriptor>
        {
            new SkillEffectPropertyDescriptor()
            {
                Name = "TestNumber",
                Type = EffectPropertyType.Number,
                DefaultValue = 0
            },
            new SkillEffectPropertyDescriptor()
            {
                Name = "TestString",
                Type = EffectPropertyType.Text,
                DefaultValue = "default"
            },
            new SkillEffectPropertyDescriptor()
            {
                Name = "TestBoolean",
                Type = EffectPropertyType.Boolean,
                DefaultValue = false
            },
            new SkillEffectPropertyDescriptor()
            {
                Name = "TestVector2/Point",
                Type = EffectPropertyType.Vector2,
                DefaultValue = Vector2.Zero
            }
        };

    public void ApplyEffect(Entity caster, List<Entity> target)
    {
        // Example effect: Log the application of the effect
        Console.WriteLine($"TestSkillEffect applied by caster {caster.Id} to {target.Count} targets.");
    }

    public object Clone()
    {
        return new TestSkillEffect
        {
            Properties = new Dictionary<string, object>(Properties),
        };
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
}
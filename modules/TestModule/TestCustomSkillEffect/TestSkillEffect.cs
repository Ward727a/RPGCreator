using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.Types;

namespace TestModule.TestCustomSkillEffect;


/*
 *
 * This is a test implementation.
 * For now, this should not be used, as the API is still in development, and this file is meant to be used for testing and development purposes.
 * Note: This implementation is NOT FUNCTIONAL, and a lots of thing are not implemented.
 * If you want / need to use a custom skill effect, please wait until the API is finalized, as custom skill effect API is being reworked.
 * 
 */


[SkillEffect]
public class TestSkillEffect : ISkillEffect
{
    public Ulid Unique { get; private set; }
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

    public string Name { get; set; }
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
    public UrnSingleModule UrnModule { get; }
    public void SuspendTracking()
    {
        throw new NotImplementedException();
    }

    public void ResumeTracking()
    {
        throw new NotImplementedException();
    }

    public void UpdateUrn()
    {
        throw new NotImplementedException();
    }

    public void Init(Ulid id)
    {
        Unique = id;
    }
}
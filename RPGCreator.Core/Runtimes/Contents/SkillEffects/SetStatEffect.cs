using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.Core.Runtimes.ECS.Components.Actor;
using RPGCreator.Core.Types.Assets.Skills;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.Types;
using Serilog;

namespace RPGCreator.Core.Runtimes.Contents.SkillEffects;

[SkillEffect]
public class SetStatEffect : ISkillEffect
{
    public Ulid Unique { get; } = Ulid.NewUlid();
    public URN Urn { get; } = new URN("skill_effect","set_stat");
    public string DisplayName { get; } = "Set Stat";
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>()
    {
        { "StatDefUnique", Ulid.Empty }, // The unique ID of the stat definition to modify
        { "Value", 0f } // The value to set the stat to
    };

    public static IReadOnlyList<SkillEffectPropertyDescriptor> PropertyDescriptors { get; } = new List<SkillEffectPropertyDescriptor>
    {
        new SkillEffectPropertyDescriptor
        {
            Name = "StatDefUnique",
            Type = EffectPropertyType.StatReference,
            DefaultValue = Ulid.Empty
        },
        new SkillEffectPropertyDescriptor
        {
            Name = "Value",
            Type = EffectPropertyType.Number,
            DefaultValue = 0f
        }
    };


    public void ApplyEffect(Entity caster, List<Entity> target)
    {
        // First check if we have all the required properties
        if (!Properties.ContainsKey("StatDefUnique") || !Properties.ContainsKey("Value"))
        {
            // Missing required properties
            Log.Error("SetStatEffect: Missing required properties 'StatDefUnique' or 'Value'.");
            return;
        }
        
        var statDefUnique = (Ulid)Properties["StatDefUnique"];
        var value = Convert.ToSingle(Properties["Value"]);

        foreach (var targetStat in target)
        {
            ApplyStatChange(targetStat, statDefUnique, value);
        }
    }

    private void ApplyStatChange(Entity target, Ulid statDefUnique, float value)
    {
        if (target.HasComponent<StatsComponent>())
        {
            var statsComponent = target.GetComponent<StatsComponent>();
    
            var statInstance = statsComponent.GetStatByUnique(statDefUnique);
            if (statInstance == null) return;
            
            statInstance.SetCurrentValue(value);
            return;
        }
        Log.Error("SetStatEffect: Target entity does not have a StatsComponent.");
    }

    public object Clone()
    {
        return new SetStatEffect
        {
            Properties = new Dictionary<string, object>(Properties),
        };
    }
}
using MonoGame.Extended.ECS;
using RPGCreator.Core.Runtimes.ECS.Components.Actor;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Runtimes.Contents.SkillEffects;

public class SetStatEffect : ISkillEffect
{
    public Ulid Unique { get; } = Ulid.NewUlid();
    public URN Urn { get; } = new URN("skill_effect","set_stat");
    public Dictionary<string, object> Properties { get; set; }
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
            // ApplyStatChange(targetStat, statDefUnique, value);
        }
    }

    // private void ApplyStatChange(Entity target, Ulid statDefUnique, float value)
    // {
    //     if (target.Has<StatsComponent>())
    //     {
    //         var statsComponent = target.Get<StatsComponent>();
    //         if (statsComponent == null) return;
    //
    //         var statInstance = statsComponent.GetStatByUnique(statDefUnique);
    //         if (statInstance == null) return;
    //         
    //         statInstance.SetCurrentValue(value);
    //         return;
    //     }
    //     Log.Error("SetStatEffect: Target entity does not have a StatsComponent.");
    // }
}
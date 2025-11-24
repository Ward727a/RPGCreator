using System.Reflection;
using RPGCreator.Core.Parser.Graph.NodesMaker;
using RPGCreator.Core.Runtimes.Contents;
using RPGCreator.Core.Types.Assets.Skills;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.Core.Types.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillEffectsRegistry: RegistryBase<ISkillEffect>
{
    
    public override string ModuleName => "skill_effects";
    
    public void ReloadData()
    {
        // Clear existing data
        Clear();
        Log.Information("Skill Effects registry cleared for data reload.");
        
        // We just want assemblies that are not system or common libraries
        // If the creator of the assembly use one of those names, well, too bad for them.
        // They should not do that anyway
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a =>
            !a.FullName.StartsWith("System") &&
            !a.FullName.StartsWith("Avalonia") &&
            !a.FullName.StartsWith("Microsoft") &&
            !a.FullName.StartsWith("Ulid") &&
            !a.FullName.StartsWith("CommunityToolkit") &&
            !a.FullName.StartsWith("Serilog") &&
            !a.FullName.StartsWith("MonoGame"));
        var skillEffectTypes = new List<System.Type>();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ISkillEffect).IsAssignableFrom(t))
                    .Where(t => t.GetCustomAttribute<SkillEffectAttribute>() != null);
                
                skillEffectTypes.AddRange(types);
                Log.Information("SkillEffectsRegistry: Found {Count} skill effect candidates in assembly {AssemblyName}.",
                    types.Count(), assembly.GetName().Name);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log.Error(ex, "Error loading types from assembly {AssemblyName}", assembly.FullName);
            }
        }
        
        Log.Information("SkillEffectsRegistry: Total {Count} skill effect candidates found across all assemblies.",
            skillEffectTypes.Count);

        foreach (var type in skillEffectTypes)
        {
            try
            {
                var skillEffectObject = Activator.CreateInstance(type);
                if (skillEffectObject is not ISkillEffect skillEffect)
                {
                    Log.Error("SkillEffectsRegistry: Type {TypeName} is not a valid ISkillEffect.", type.FullName);
                    continue;
                }

                Register(skillEffect);
                Log.Information(
                    "SkillEffectsRegistry: Registered skill effect {SkillEffectName} with unique ID {SkillEffectUnique} and URN {SkillEffectUrn}.",
                    type.FullName, skillEffect.Unique, skillEffect.Urn);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error instantiating skill effect of type {TypeName}", type.FullName);
            }
        }


    }

}
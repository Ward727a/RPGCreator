using System.Reflection;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillEffectsRegistry: RegistryBase<ISkillEffect>
{
    private readonly ScopedLogger _logger = Logger.ForContext<SkillEffectsRegistry>();
    public override string ModuleName => "skill_effects";
    
    public void ReloadData()
    {
        // Clear existing data
        Clear();
        _logger.Info("Skill Effects registry cleared for data reload.");
        
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
                _logger.Info("SkillEffectsRegistry: Found {Count} skill effect candidates in assembly {AssemblyName}.",
                    args: [types.Count(), assembly.GetName().Name]);
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.Error(ex, "Error loading types from assembly {AssemblyName}", args: assembly.FullName);
            }
        }
        
        _logger.Info("SkillEffectsRegistry: Total {Count} skill effect candidates found across all assemblies.",
            args: skillEffectTypes.Count);

        foreach (var type in skillEffectTypes)
        {
            try
            {
                var skillEffectObject = Activator.CreateInstance(type);
                if (skillEffectObject is not ISkillEffect skillEffect)
                {
                    _logger.Error("SkillEffectsRegistry: Type {TypeName} is not a valid ISkillEffect.", args: type.FullName);
                    continue;
                }

                Register(skillEffect);
                _logger.Info(
                    "SkillEffectsRegistry: Registered skill effect {SkillEffectName} with unique ID {SkillEffectUnique} and URN {SkillEffectUrn}.",
                    args: [type.FullName, skillEffect.Unique, skillEffect.Urn]);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error instantiating skill effect of type {TypeName}", args: type.FullName);
            }
        }


    }

}
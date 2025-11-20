using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillsRegistry : RegistryBase<ISkillDef>
{
    public override string ModuleName => "skills";
}
using RPGCreator.Core.Types.Assets.Skills;
using RPGCreator.Core.Types.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillsRegistry : RegistryBase<ISkillDef>
{
    public override string ModuleName => "skills";
}
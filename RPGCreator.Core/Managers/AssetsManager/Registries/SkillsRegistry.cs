using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets.Definitions.Skills;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillsRegistry : RegistryBase<ISkillDef>
{
    public override string ModuleName => "skills";
}
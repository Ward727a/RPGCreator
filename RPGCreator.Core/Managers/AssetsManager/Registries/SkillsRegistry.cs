using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Skills;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class SkillsRegistry : RegistryBase<ISkillDef>
{
    public override string ModuleName => "skills";
}
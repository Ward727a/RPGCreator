using RPGCreator.SDK.Assets.Definitions.Stats;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public sealed class StatsRegistry : RegistryBase<IStatDef>
{
    public override string ModuleName => "stats";
}
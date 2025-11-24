using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public sealed class StatsRegistry : RegistryBase<IStatDef>
{
    public override string ModuleName => "stats";
}
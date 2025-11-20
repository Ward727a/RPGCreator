using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public sealed class StatsRegistry : RegistryBase<IStatDef>
{
    public override string ModuleName => "stats";
}
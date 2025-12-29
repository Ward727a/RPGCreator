using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets.Definitions.Characters;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class CharacterRegistry : RegistryBase<CharacterData>
{
    public override string ModuleName => "characters";
}
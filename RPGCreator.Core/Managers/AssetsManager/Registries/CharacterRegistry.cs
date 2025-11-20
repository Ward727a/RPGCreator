using RPGCreator.Core.Type.Assets.Characters;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class CharacterRegistry : RegistryBase<CharacterData>
{
    public override string ModuleName => "characters";
}
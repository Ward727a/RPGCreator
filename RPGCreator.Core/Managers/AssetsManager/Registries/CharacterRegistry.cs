using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class CharacterRegistry : RegistryBase<CharacterData>
{
    public override string ModuleName => "characters";
    public override IEnumerable<Type> SupportedTypes => new[] { typeof(CharacterData), typeof(IEntityDefinition) };
}
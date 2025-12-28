using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.Runtimes.ECS.Components.Actor;

public struct CharDataComponent : IComponent
{
    public CharacterData CharacterData;
}
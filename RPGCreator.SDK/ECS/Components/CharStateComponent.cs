using RPGCreator.SDK.Assets.Definitions.Characters;

namespace RPGCreator.SDK.ECS.Components;

public struct CharStateComponent : IComponent
{
    public Dictionary<int, DirectionalAnimationSet> AnimationsMapping { get; set; }
}
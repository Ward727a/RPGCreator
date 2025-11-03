using RPGCreator.Core.Type.Assets.Animations;

namespace RPGCreator.Core.Runtimes.ECS.Components.Display.Animation;

public struct AnimationSetComponent : IComponent
{
        public Dictionary<string, AnimationInstance?> Animations;
}
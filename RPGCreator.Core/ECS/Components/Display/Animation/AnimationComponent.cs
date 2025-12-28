using RPGCreator.Core.Types.Assets.Animations;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.ECS.Components.Display.Animation;

public struct AnimationComponent() : IComponent
{
    public AnimationInstance? Instance;
    
    public double ElapsedTime = 0.0;
    public int CurrentFrame = 0;
    
    public bool IsPlaying = true;
    public float SpeedMultiplier = 1.0f;
}
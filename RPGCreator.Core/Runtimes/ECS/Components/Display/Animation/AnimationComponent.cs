namespace RPGCreator.Core.Runtimes.ECS.Components.Display.Animation;

public struct AnimationComponent : IComponent
{
    public string CurrentAnimation;
    public double ElapsedTime;
    public int CurrentFrame;
}
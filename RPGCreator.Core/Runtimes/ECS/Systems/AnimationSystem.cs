using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.Core.Runtimes.ECS.Components.Display.Animation;
using RPGCreator.Core.Types.Assets.Animations;
using RPGCreator.Core.Types.Internal;
using Serilog;

namespace RPGCreator.Core.Runtimes.ECS.Systems;

public class AnimationSystem : ISystem
{
    private readonly ILogger _logger = Log.ForContext<AnimationSystem>();
    
    private readonly ComponentManager _componentManager;
    
    private readonly GraphicsDevice _graphicsDevice;

    public override int Priority { get; } = 200; // Need to run before SpriteRenderSystem
    public override bool IsDrawingSystem { get; } = true;
    
    public AnimationSystem(ComponentManager componentManager, GraphicsDevice graphicsDevice)
    {
        _componentManager = componentManager;
        _graphicsDevice = graphicsDevice;
    }
    
    public override void Initialize(IECSWorld iecsWorld)
    {
        _logger.Information("AnimationSystem initialized.");
        
        // Ensure AnimationStateSystem is also added - If it's already added, this will have no effect
        iecsWorld._systemManager.AddSystem(new CharacterAnimationSystem(_componentManager));
    }

    public override void Update(GameTime deltaTime)
    {
        foreach (int entityId in _componentManager.Query(typeof(AnimationComponent), typeof(SpriteComponent)))
        {
            ref var sprite = ref _componentManager.GetComponent<SpriteComponent>(entityId);
            ref var animation = ref _componentManager.GetComponent<AnimationComponent>(entityId);
            
            if(animation.Instance == null || !animation.IsPlaying)
                continue;
            
            animation.ElapsedTime += deltaTime.ElapsedGameTime.TotalMilliseconds * animation.SpeedMultiplier;

            if (animation.ElapsedTime >= animation.Instance.Definition.FrameDuration)
            {
                animation.ElapsedTime = 0;
                animation.CurrentFrame++;
                
                if (animation.CurrentFrame >= animation.Instance.Definition.TotalFrames)
                {
                    animation.CurrentFrame = 0;
                }
            }

            var currentFrame = animation.Instance.GetFrame(animation.CurrentFrame);

            if (!currentFrame.Source.HasGraphicsDevice())
            {
                currentFrame.Source.SetGraphicsDevice(_graphicsDevice);
            }

            sprite.Texture = currentFrame.Texture;
            sprite.SourceRectangle = currentFrame.SourceRectangle;
        }
    }
}
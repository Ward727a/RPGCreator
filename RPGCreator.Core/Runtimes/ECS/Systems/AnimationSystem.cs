using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.Core.Runtimes.ECS.Components.Display.Animation;
using RPGCreator.Core.Type.Assets.Animations;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Runtimes.ECS.Systems;

public class AnimationSystem : ISystem
{
    private readonly ILogger _logger = Log.ForContext<AnimationSystem>();
    
    private readonly ComponentManager _componentManager;

    public override int Priority { get; } = -1; // Need to run before SpriteRenderSystem
    public override bool IsDrawingSystem { get; } = true;
    
    private string _lastKnownWorkingAnimation = "idle";
    
    public AnimationSystem(ComponentManager componentManager)
    {
        _componentManager = componentManager;
    }
    
    public override void Initialize(IECSWorld iecsWorld)
    {
        _logger.Information("AnimationSystem initialized.");
        
        // Ensure AnimationStateSystem is also added - If it's already added, this will have no effect
        iecsWorld._systemManager.AddSystem(new AnimationStateSystem(_componentManager));
    }

    public override void Update(GameTime deltaTime)
    {
        foreach (int entityId in _componentManager.Query(typeof(AnimationComponent), typeof(SpriteComponent)))
        {
            ref var sprite = ref _componentManager.GetComponent<SpriteComponent>(entityId);
            ref var animation = ref _componentManager.GetComponent<AnimationComponent>(entityId);
            ref var animationSet = ref _componentManager.GetComponent<AnimationSetComponent>(entityId);
            
            if(!animationSet.Animations.ContainsKey(animation.CurrentAnimation) || animationSet.Animations[animation.CurrentAnimation] == null)
                animation.CurrentAnimation = _lastKnownWorkingAnimation;
            
            var instance = animationSet.Animations[animation.CurrentAnimation];
            animation.ElapsedTime += deltaTime.ElapsedGameTime.TotalMilliseconds;
            
            if(sprite.TextureAtlas != instance.TextureAtlas)
            {
                sprite.TextureAtlas = instance.TextureAtlas;
                animation.CurrentFrame = 0;
                animation.ElapsedTime = 0;
                _logger.Debug("Entity {entityId} switched TextureAtlas for animation {animationName}.", entityId, animation.CurrentAnimation);
            }
            
            if (sprite.TextureAtlas.RegionCount == 0)
            {
                var frameSize = new Size(48, 64);
                var totalFrames = sprite.TextureAtlas.Texture.Width / frameSize.Width *
                    sprite.TextureAtlas.Texture.Height / frameSize.Height;

                for (int i = 0; i < totalFrames; i++)
                {
                    int x = (i * frameSize.Width) % sprite.TextureAtlas.Texture.Width;
                    int y = ((i * frameSize.Width) / sprite.TextureAtlas.Texture.Width) * frameSize.Height;
                    var frameRect = new Rectangle(x, y, frameSize.Width, frameSize.Height);
                    // Here you would normally add each frame to the atlas, but since we already have the atlas,
                    // we assume it's pre-built.
                    sprite.TextureAtlas.CreateRegion(frameRect);
                    _logger.Debug("Added frame {frameIndex} at {frameRect} to TextureAtlas.", i, frameRect);
                }
            }
            
            _lastKnownWorkingAnimation = animation.CurrentAnimation;
            
            if(sprite.TextureAtlasRegion == null)
            {
                sprite.TextureAtlasRegion = sprite.TextureAtlas[0];
                continue;
            }
            
            if (animation.ElapsedTime >= instance.Definition.FrameDuration)
            {
                animation.ElapsedTime = 0;
                animation.CurrentFrame++;

                if (animation.CurrentFrame >= instance.Definition.TotalFrames)
                {
                    animation.CurrentFrame = 0;
                }

                sprite.TextureAtlasRegion = sprite.TextureAtlas[animation.CurrentFrame];
            }
        }
    }
}
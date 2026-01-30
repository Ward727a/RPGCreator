using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.ECS.Components.Display;
using RPGCreator.Core.ECS.Components.Display.Animation;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.ECS.Systems;

public class MonogameRenderContext : IRenderContext
{
    public SpriteBatch? SpriteBatch { get; }
    public GraphicsDevice? GraphicsDevice { get; }
    
    public bool HasSpriteBatch => SpriteBatch != null;
    public bool HasGraphicsDevice => GraphicsDevice != null;
    
    public MonogameRenderContext(SpriteBatch? spriteBatch = null, GraphicsDevice? graphicsDevice = null, object? target = null)
    {
        SpriteBatch = spriteBatch;
        GraphicsDevice = graphicsDevice;
    }
}

public class CoreAnimationDrawer : IDrawer<AnimationInstance>
{
    private SpritesheetDef? _cachedSpritesheet;
    private IAssetScope _assetScope;
    private SpriteComponent _spriteComponent;
    public CoreAnimationDrawer(SpriteComponent targetSprite, IAssetScope? assetScope = null)
    {
        _spriteComponent = targetSprite;
        _assetScope = assetScope ?? EngineServices.AssetsManager.CreateAssetScope();
    }
    public void Draw(IRenderContext context, AnimationInstance animation)
    {
        if (context is not MonogameRenderContext { HasGraphicsDevice: true } GameContext)
            return;

        if (_cachedSpritesheet == null || _cachedSpritesheet.Unique != animation.Definition.SpriteSheetId)
        {
            if (_cachedSpritesheet != null)
            {
                _assetScope.Unload(_cachedSpritesheet);
                EngineServices.ResourcesService.Unload(_cachedSpritesheet.ImagePath);
            }

            _cachedSpritesheet = _assetScope.Load<SpritesheetDef>(animation.Definition.SpriteSheetId);
            
            if (_cachedSpritesheet == null)
            {
                Logger.Error("[CoreAnimationDrawer] Failed to load spritesheet with ID: " + animation.Definition.SpriteSheetId);
                return;
            }
        }
        
        var currentFrame = animation.GetCurrentSpritesheetIndex();
        var spritesheetTexture = EngineServices.ResourcesService.Load<Texture2D>(_cachedSpritesheet.ImagePath);
        
        _spriteComponent.Texture = spritesheetTexture;
        var rect = _cachedSpritesheet.GetFrameRect(currentFrame);
        _spriteComponent.SourceRectangle = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }
}

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
    
    public override void Initialize(IEcsWorld iecsWorld)
    {
        _logger.Information("AnimationSystem initialized.");
        
        // Ensure AnimationStateSystem is also added - If it's already added, this will have no effect
        iecsWorld.SystemManager.AddSystem(new CharacterAnimationSystem(_componentManager));
    }

    public override void Update(TimeSpan deltaTime)
    {
        foreach (int entityId in _componentManager.Query(typeof(AnimationComponent), typeof(SpriteComponent)))
        {
            ref var sprite = ref _componentManager.GetComponent<SpriteComponent>(entityId);
            ref var animation = ref _componentManager.GetComponent<AnimationComponent>(entityId);
            
            if(animation.Instance == null || !animation.IsPlaying)
                continue;
            
            animation.ElapsedTime += deltaTime.TotalMilliseconds * animation.SpeedMultiplier;

            if (animation.ElapsedTime >= animation.Instance.Definition.FrameDuration)
            {
                animation.ElapsedTime = 0;
                animation.CurrentFrame++;
                
                if (animation.CurrentFrame >= animation.Instance.Definition.TotalFrames)
                {
                    animation.CurrentFrame = 0;
                }
            }

            animation.Instance.Draw(new MonogameRenderContext(graphicsDevice: _graphicsDevice), new CoreAnimationDrawer(sprite));
        }
    }
}
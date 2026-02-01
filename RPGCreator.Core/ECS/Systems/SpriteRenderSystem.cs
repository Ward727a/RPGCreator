using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.ECS.Components.Display;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using Serilog;

namespace RPGCreator.Core.ECS.Systems;

public class SpriteRenderSystem : ISystem
{
    
    private readonly ComponentManager _componentManager;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatchExtend _spriteBatch;
    
    private readonly Dictionary<string, Texture2D> _textureCache = new();
    
    public SpriteRenderSystem(ComponentManager componentManager, GraphicsDevice graphicsDevice)
    {
        _componentManager = componentManager;
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatchExtend(graphicsDevice);
    }

    public override int Priority { get; } = 400;
    public override bool IsDrawingSystem { get; } = true;

    public override void Initialize(IEcsWorld ecsWorld)
    {
        Log.Information("SpriteRenderSystem initialized.");
    }

    public override void Update(TimeSpan deltaTime)
    {
        _spriteBatch.Begin();

        // #if DEBUG
        // var sw = System.Diagnostics.Stopwatch.StartNew();
        // #endif
        foreach (var (entityId, sprite) in _componentManager.GetAll<SpriteComponent>())
        {
            if (sprite.Texture == null)
                continue;
            
            var transform = _componentManager.GetComponent<TransformComponent>(entityId);
            
            var position = transform.Position;
            
            _spriteBatch.Draw(
                sprite.Texture,
                position,
                sprite.SourceRectangle,
                sprite.Color);
        }
        // #if DEBUG
        // sw.Stop();
        // Log.Debug($"SpriteRenderSystem rendered in {sw.ElapsedMilliseconds} ms");
        // #endif

        _spriteBatch.End();
    }
}
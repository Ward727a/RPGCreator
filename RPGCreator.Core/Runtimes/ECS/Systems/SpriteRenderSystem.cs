using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.Core.Type.Internal;
using Serilog;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Core.Runtimes.ECS.Systems;

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

    public override int Priority { get; } = 0;
    public override bool IsDrawingSystem { get; } = true;

    public override void Initialize(IECSWorld iecsWorld)
    {
        Log.Information("SpriteRenderSystem initialized.");
    }

    public override void Update(GameTime deltaTime)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred);

        // #if DEBUG
        // var sw = System.Diagnostics.Stopwatch.StartNew();
        // #endif
        foreach (var (entityId, sprite) in _componentManager.GetAll<SpriteComponent>())
        {
            Vector2 position = Vector2.Zero;

            var transform = _componentManager.GetComponent<TransformComponent>(entityId);
            position = transform.Position;

            if (!_textureCache.TryGetValue(sprite.SpritePath, out var texture))
            {
                texture = LoadTexture(sprite.SpritePath);
                _textureCache[sprite.SpritePath] = texture;
            }
            
            Size spriteSize = sprite.Size;

            _spriteBatch.Draw(texture, new Rectangle((int)position.X, (int)position.Y, spriteSize.Width, spriteSize.Height), Color.White);
        }
        // #if DEBUG
        // sw.Stop();
        // Log.Debug($"SpriteRenderSystem rendered in {sw.ElapsedMilliseconds} ms");
        // #endif

        _spriteBatch.End();
    }
    
    private Texture2D LoadTexture(string path)
    {
        using var stream = System.IO.File.OpenRead(path);
        return Texture2D.FromStream(_graphicsDevice, stream);
    }
}
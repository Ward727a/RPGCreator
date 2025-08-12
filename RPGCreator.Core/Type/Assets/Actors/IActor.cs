using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;

namespace RPGCreator.Core.Type.Assets.Actors;

/// <summary>
/// Represents a simple actor.
/// </summary>
public interface IActor
{
    public Ulid Unique { get; }

    public void Draw(SpriteBatchExtend? sb);
    public void Update(GameTime gameTime);
}
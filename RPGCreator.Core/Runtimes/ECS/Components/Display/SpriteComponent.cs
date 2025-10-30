using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Runtimes.ECS.Components.Display;

public class SpriteComponent : IComponent
{
    public string SpritePath;
    public Size Size;
    public bool IsAnimated;
    public Texture2DAtlas? TextureAtlas;
    public Texture2DRegion? TextureAtlasRegion;
    public double FrameDuration = 100.0; // milliseconds per frame
}
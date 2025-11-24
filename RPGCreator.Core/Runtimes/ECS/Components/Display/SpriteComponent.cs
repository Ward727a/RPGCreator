using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Runtimes.ECS.Components.Display;

public class SpriteComponent() : IComponent
{
    public Texture2D? Texture;
    public Rectangle? SourceRectangle = null;
    public Size RenderSize;
    public Color Color = Color.White;
    
    public SpriteEffects SpriteEffect = SpriteEffects.None;
    
    public float LayerDepth = 0f;
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.ECS.Components.Display;

public class SpriteComponent() : IComponent
{
    public Texture2D? Texture;
    public Rectangle? SourceRectangle = null;
    public Size RenderSize;
    public Color Color = Color.White;
    
    public SpriteEffects SpriteEffect = SpriteEffects.None;
    
    public float LayerDepth = 0f;
}
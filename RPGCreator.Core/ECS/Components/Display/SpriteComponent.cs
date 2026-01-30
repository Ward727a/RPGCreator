using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.ECS.Components.Display;

public struct SpriteComponent : IComponent
{
    public Texture2D? Texture;
    public Rectangle? SourceRectangle;
    public Size RenderSize;
    public Color Color;
    
    public SpriteEffects SpriteEffect;
    
    public float LayerDepth;
}
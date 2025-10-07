using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Runtimes.ECS.Components.Display;

public struct SpriteComponent : IComponent
{
    public string SpritePath;
    public Size Size;
}
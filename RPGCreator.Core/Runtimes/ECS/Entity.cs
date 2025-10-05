using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Runtimes.ECS;

public class Entity : IEntity, ICleanable
{
    public int Id { get; }
    public void Clean()
    {
        throw new NotImplementedException();
    }
}
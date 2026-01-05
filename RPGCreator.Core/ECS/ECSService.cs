using RPGCreator.SDK;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.ECS;

public class ECSService : IECSService
{
    public IECSWorld CreateWorld()
    {
        return new ECSWorld();
    }
}
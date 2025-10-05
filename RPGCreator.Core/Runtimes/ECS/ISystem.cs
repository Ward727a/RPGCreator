
namespace RPGCreator.Core.Runtimes.ECS;

public interface ISystem
{
    void Update(IWorld world, float deltaTime);
}
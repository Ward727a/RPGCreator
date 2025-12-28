using System.Numerics;
using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.SDK.ECS.Entities;

namespace RPGCreator.SDK.ECS.Services;

public interface IEntityFactory
{
    public Entity SpawnEntity(BaseEntity entityData, Vector2 position);
    public void InitializeEntity(Entity entity, BaseEntity entityData);
}
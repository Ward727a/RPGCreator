using System.Numerics;
using RPGCreator.SDK.ECS.Entities;

namespace RPGCreator.SDK.ECS.Factories;

public interface IEntityFactory
{
    public Entity SpawnEntity(BaseEntity entityData, Vector2 position);
    public void InitializeEntity(Entity entity, BaseEntity entityData);
}
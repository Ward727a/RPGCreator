using System.Numerics;
using RPGCreator.SDK.ECS.Entities;

namespace RPGCreator.SDK.ECS.Factories;

public interface IEntityFactory
{
    public Entity SpawnEntity(IEntityDefinition entityDefinitionData, Vector2 position);
    public void InitializeEntity(Entity entity, IEntityDefinition entityDefinitionData);
}
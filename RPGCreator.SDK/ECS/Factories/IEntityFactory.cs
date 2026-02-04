using System.Numerics;

namespace RPGCreator.SDK.ECS.Factories;

public interface IEntityFactory
{
    public BufferedEntity SpawnEntity(IEntityDefinition entityDefinitionData, Vector2 position);
    public void InitializeEntity(BufferedEntity entity, IEntityDefinition entityDefinitionData, Vector2 position);
    public void InitializeEntity(BufferedEntity entity, IEntityDefinition entityDefinitionData);
}
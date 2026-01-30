using System.Numerics;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Factories;

namespace RPGCreator.Core.Runtimes.Factories;

public class EntityFactory : IEntityFactory
{
    
    private readonly IEcsWorld _world;

    public EntityFactory(IEcsWorld world)
    {
        _world = world;
    }

    public BufferedEntity SpawnEntity(IEntityDefinition entityDefinitionData, Vector2 position)
    {
        var entity = _world.CreateEntity();
        
        entity.AddComponent(new TransformComponent(){Position = position});

        InitializeEntity(entity, entityDefinitionData);
        
        return entity;
    }

    public void InitializeEntity(BufferedEntity entity, IEntityDefinition entityDefinitionData)
    {
        var autoFeatures = EngineCore.Instance.Managers.FeaturesRules.GetAllAutoFeatures(entityDefinitionData.Tags);

        var addedTypes = new HashSet<Type>();
        
        // First the manual added features
        foreach (var feature in entityDefinitionData.Features)
        {
            var featureType = feature.GetType();
            addedTypes.Add(featureType);
            
            feature.OnInject(entity);
        }
        
        // Then the auto added features
        foreach (var feature in autoFeatures)
        {
            var featureType = feature.GetType();
            if (addedTypes.Contains(featureType))
                continue;
            feature.OnInject(entity);
        }
    }
}
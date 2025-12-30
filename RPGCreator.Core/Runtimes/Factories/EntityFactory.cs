using System.Numerics;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Factories;

namespace RPGCreator.Core.Runtimes.Factories;

public class EntityFactory : IEntityFactory
{
    
    private readonly EntityManager _entityManager;

    public EntityFactory(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }

    public Entity SpawnEntity(BaseEntity entityData, Vector2 position)
    {
        var entity = _entityManager.CreateEntity();

        var transform = entity.AddComponent<TransformComponent>();
        transform.Position = position;

        InitializeEntity(entity, entityData);
        
        return entity;
    }

    public void InitializeEntity(Entity entity, BaseEntity entityData)
    {
        var autoFeatures = EngineCore.Instance.Managers.FeaturesRules.GetAllAutoFeatures(entityData.Tags);

        var addedTypes = new HashSet<Type>();
        
        // First the manual added features
        foreach (var feature in entityData.Features)
        {
            var featureType = feature.GetType();
            addedTypes.Add(featureType);
            
            feature.OnInitialize(entity);
        }
        
        // Then the auto added features
        foreach (var feature in autoFeatures)
        {
            var featureType = feature.GetType();
            if (addedTypes.Contains(featureType))
                continue;
            feature.OnInitialize(entity);
        }
    }
}
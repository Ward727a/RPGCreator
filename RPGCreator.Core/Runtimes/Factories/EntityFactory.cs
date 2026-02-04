using System.Numerics;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Factories;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Types;

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

        entity.AddComponent(new TransformComponent { Position = position, ScaleX = 1f, ScaleY = 1f });

        InitializeEntity(entity, entityDefinitionData);
        
        return entity;
    }
    
    public void InitializeEntity(BufferedEntity entity, IEntityDefinition entityDefinitionData, Vector2 position)
    {
        entity.AddComponent(new TransformComponent()
        {
            Position = position, ScaleX = 1f, ScaleY = 1f
        });
        
        InitializeEntity(entity, entityDefinitionData);
    }

    private List<URN> _worldInjectedFeatures = new List<URN>();
    
    public void InitializeEntity(BufferedEntity entity, IEntityDefinition entityDefinitionData)
    {
        // First the manual added features
        foreach (var featureData in entityDefinitionData.Features)
        {
            var feature = EngineServices.FeaturesManager.CreateEntityFeatureInstance(featureData.FeatureUrn);
            if (feature == null)
                continue;
            
            if (!_worldInjectedFeatures.Contains(feature.FeatureUrn))
            {
                feature.OnWorldSetup(_world);
                _worldInjectedFeatures.Add(feature.FeatureUrn);
            }
            feature.SetConfiguration(featureData.Configuration, new EngineSecurityToken());
            feature.OnInject(entity, entityDefinitionData);
        }
    }
}
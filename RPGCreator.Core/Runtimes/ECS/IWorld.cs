namespace RPGCreator.Core.Runtimes.ECS;

public interface IWorld
{
    EntityManager _entityManager { get; }
    
    IEntity CreateEntity();
    void DestroyEntity(IEntity entity);
    T AddComponent<T>(IEntity entity) where T : IComponent, new();
    ref T GetComponent<T>(IEntity entity) where T : IComponent;
    void Update(float deltaTime);
    
    public WorldQuery<T> Query<T>() where T : struct, IComponent;
}
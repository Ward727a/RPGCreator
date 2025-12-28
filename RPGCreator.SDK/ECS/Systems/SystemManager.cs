namespace RPGCreator.SDK.ECS.Systems;

public class SystemManager(IECSWorld world)
{
    private IECSWorld _world = world;
    private readonly List<ISystem> _systems = new();
    private readonly Queue<ISystem> _toAdd = new();
    private readonly Queue<ISystem> _toRemove = new();

    public void AddSystem(ISystem system)
    {
        _toAdd.Enqueue(system);
    }

    public void RemoveSystem(ISystem system)
    {
        _toRemove.Enqueue(system);
    }

    public void Update(TimeSpan deltaTime)
    {
        while (_toAdd.Count > 0)
        {
            var sys = _toAdd.Dequeue();
            
            if (_systems.Contains(sys)) continue;
            
            sys.Initialize(_world);
            _systems.Add(sys);
            sys.OnEnable?.Invoke();
        }

        while (_toRemove.Count > 0)
        {
            var sys = _toRemove.Dequeue();
            if (_systems.Remove(sys))
                sys.OnDisable?.Invoke();
        }

        foreach (var system in _systems.Where(s => !s.IsDrawingSystem).OrderBy(s => s.Priority))
        {
            system.Update(deltaTime);
        }
    }
    
    public void Draw(TimeSpan deltaTime)
    {
        foreach (var system in _systems.Where(s => s.IsDrawingSystem).OrderBy(s => s.Priority))
        {
            system.Update(deltaTime);
        }
    }
}
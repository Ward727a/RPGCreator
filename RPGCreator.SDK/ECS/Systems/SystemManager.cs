namespace RPGCreator.SDK.ECS.Systems;

public class SystemManager(IEcsWorld world)
{
    private IEcsWorld _world = world;
    private readonly List<ISystem> _updateSystems = new();
    private readonly List<ISystem> _drawingSystems = new();
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
            
            if (_updateSystems.Contains(sys)) continue;
            
            sys.Initialize(_world);
            if (sys.IsDrawingSystem)
            {
                _drawingSystems.Add(sys);
                _drawingSystems.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
            else
            {
                _updateSystems.Add(sys);
                _updateSystems.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
            sys.OnEnable?.Invoke();
        }

        while (_toRemove.Count > 0)
        {
            var sys = _toRemove.Dequeue();
            if (_drawingSystems.Remove(sys) || _updateSystems.Remove(sys))
                sys.OnDisable?.Invoke();
        }

        foreach (var system in _updateSystems)
        {
            system.Update(deltaTime);
        }
    }
    
    public List<ISystem> GetDrawingSystems()
    {
        return _drawingSystems.OrderBy(x => x.Priority).ToList();
    }
    
    public void Draw(TimeSpan deltaTime)
    {
        RuntimeServices.RenderService.PrepareDrawing();
        
        foreach (var system in GetDrawingSystems())
        {
            system.Update(deltaTime);
        }
        
        RuntimeServices.RenderService.FinishDrawing();
    }
}
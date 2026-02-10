namespace RPGCreator.SDK.ECS.Systems;

public class SystemManager(IEcsWorld world)
{
    private IEcsWorld _world = world;
    
    private int _nextSystemId = 0;
    
    private readonly Dictionary<int, ISystem> _systemsById = new();
    private readonly Dictionary<ISystem, int> _systemIdsBySystem = new();
    private readonly Dictionary<Type, ISystem> _systemsByType = new();
    private readonly List<ISystem> _updateSystems = new();
    private readonly List<ISystem> _drawingSystems = new();
    private readonly Queue<ISystem> _toAdd = new();
    private readonly Queue<ISystem> _toRemove = new();

    public int AddSystem(ISystem system)
    {
        _toAdd.Enqueue(system);
        return _nextSystemId + _toAdd.Count;
    }

    public void RemoveSystem(ISystem system)
    {
        _toRemove.Enqueue(system);
    }
    
    public void GetSystem<T>(out T? system) where T : ISystem
    {
        if (_systemsByType.TryGetValue(typeof(T), out var sys))
        {
            system = sys as T;
        }
        else
        {
            system = null;
        }
    }
    
    public void GetSystem<T>(int id, out T? system) where T : ISystem
    {
        if (_systemsById.TryGetValue(id, out var sys))
        {
            system = sys as T;
        }
        else
        {
            system = null;
        }
    }

    public void Update(TimeSpan deltaTime)
    {
        while (_toAdd.Count > 0)
        {
            var sys = _toAdd.Dequeue();
            int id = _nextSystemId++;

            sys.Initialize(_world);
            _systemsById[id] = sys;
            _systemIdsBySystem[sys] = id;
            _systemsByType[sys.GetType()] = sys;

            var targetList = sys.IsDrawingSystem ? _drawingSystems : _updateSystems;
            targetList.Add(sys);
            targetList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            sys.OnEnable?.Invoke();
        }

        while (_toRemove.Count > 0)
        {
            var sys = _toRemove.Dequeue();
            if (_drawingSystems.Remove(sys) || _updateSystems.Remove(sys))
            {
                if (_systemsByType.ContainsKey(sys.GetType()))
                {
                    _systemsByType.Remove(sys.GetType());
                }
                if (_systemIdsBySystem.Remove(sys, out var id))
                {
                    _systemsById.Remove(id);
                }
                
                sys.OnDisable?.Invoke();
            }
        }

        foreach (var system in _updateSystems)
        {
            system.Update(deltaTime);
        }
    }
    
    public List<ISystem> GetDrawingSystems()
    {
        return _drawingSystems;
    }
    
    public void Draw(TimeSpan deltaTime)
    {
        RuntimeServices.RenderService.PrepareDrawing();
        
        foreach (var system in _drawingSystems)
        {
            system.Update(deltaTime);
        }
        
        RuntimeServices.RenderService.FinishDrawing();
    }
}
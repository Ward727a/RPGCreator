using Microsoft.Xna.Framework;

namespace RPGCreator.Core.Runtimes.ECS;

public class SystemManager
{
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

    public void Update(GameTime gameTime)
    {
        while (_toAdd.Count > 0)
        {
            var sys = _toAdd.Dequeue();
            if (!_systems.Contains(sys))
                _systems.Add(sys);
            sys.OnEnable?.Invoke();
        }

        while (_toRemove.Count > 0)
        {
            var sys = _toRemove.Dequeue();
            if (_systems.Remove(sys))
                sys.OnDisable?.Invoke();
        }

        foreach (var system in _systems.Where(s => !s.IsDrawingSystem))
        {
            system.Update(gameTime);
        }
    }
    
    public void Draw(GameTime gameTime)
    {
        foreach (var system in _systems.Where(s => s.IsDrawingSystem))
        {
            system.Update(gameTime);
        }
    }
}
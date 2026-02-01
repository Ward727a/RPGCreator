namespace RPGCreator.SDK.ECS;

public class ECSEventBus
{
    private readonly Dictionary<System.Type, List<Delegate>> _subscribers = new();
    
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        if (!_subscribers.TryGetValue(typeof(TEvent), out var list))
        {
            list = new List<Delegate>();
            _subscribers[typeof(TEvent)] = list;
        }
        list.Add(handler);
    }

    public void Publish<TEvent>(TEvent evt)
    {
        if (_subscribers.TryGetValue(typeof(TEvent), out var list))
        {
            foreach (var handler in list.Cast<Action<TEvent>>())
                handler(evt);
        }
    }
}

public readonly struct ComponentChangedEvent<T> where T : IComponent
{
    public int EntityId { get; }
    public ChangeType Change { get; }
    public T? Previous { get; }
    public T? Current { get; }

    public ComponentChangedEvent(int entityId, ChangeType change, T? previous = default, T? current = default)
    {
        EntityId = entityId;
        Change = change;
        Previous = previous;
        Current = current;
    }

    public override string ToString() =>
        $"[ComponentChangedEvent<{typeof(T).Name}>] Entity={EntityId}, Change={Change}";
}

public enum ChangeType
{
    Added,
    Updated,
    Removed
}
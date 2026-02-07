using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RPGCreator.SDK.ECS;

public class EcsEventBus
{
    private static class EventStorage<TEvent>
    {
        public static readonly List<Action<TEvent>> Handlers = new();
    }
    
    private readonly HashSet<Type> _registeredTypes = new();
    
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        lock (EventStorage<TEvent>.Handlers)
        {
            EventStorage<TEvent>.Handlers.Add(handler);
            _registeredTypes.Add(typeof(TEvent));
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish<TEvent>(TEvent evt)
    {
        var handlers = EventStorage<TEvent>.Handlers;
        
        var span = CollectionsMarshal.AsSpan(handlers);
        for (int i = 0; i < span.Length; i++)
        {
            span[i](evt);
        }
    }
    
    public void ClearHandlersForType<TEvent>()
    {
        lock (EventStorage<TEvent>.Handlers)
        {
            EventStorage<TEvent>.Handlers.Clear();
            _registeredTypes.Remove(typeof(TEvent));
        }
    }
}
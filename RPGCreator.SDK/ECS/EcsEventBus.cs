using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.ECS;

public interface IEcsEvent
{
    public URN EventType { get; set; }
    public int? FromEntityId { get; set; }
    public CustomData Data { get; }
    public bool IsHandled { get; set; }

    public void Reset();
}

public class EcsEvent : IEcsEvent
{
    public URN EventType { get; set; }
    public int? FromEntityId { get; set; }
    public CustomData Data { get; } = new();
    public bool IsHandled { get; set; }

    public EcsEvent()
    {
    }

    public void Reset()
    {
        Data.Clear();
        IsHandled = false;
        FromEntityId = null;
        EventType = URN.Empty;
    }
}

public interface IEcsSubscriber
{
    public int Priority { get; }
    public URN EventType { get; }
    
    public void OnEvent(IEcsEvent evt);
    public void OnUnsubscribe();
}

public class BaseSubscriber : IEcsSubscriber, IDisposable
{
    public int Priority { get; set; }
    public URN EventType { get; }
    
    private readonly Action<IEcsEvent>? _onEvent;
    private readonly Action? _onUnsubscribe;
    
    private bool _isDisposed;
    
    public BaseSubscriber(URN eventType, int priority = 0, Action<IEcsEvent>? onEvent = null, Action? onUnsubscribe = null)
    {
        EventType = eventType;
        Priority = priority;
        _onEvent = onEvent;
        _onUnsubscribe = onUnsubscribe;
    }
    
    public virtual void Subscribe()
    {
        RuntimeServices.GameSession.ActiveEcsWorld?.EventBus.Subscribe(this);
    }
    
    public virtual void OnEvent(IEcsEvent evt)
    {
        _onEvent?.Invoke(evt);
    }

    public virtual void OnUnsubscribe()
    {
        _onUnsubscribe?.Invoke();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        
        RuntimeServices.GameSession.ActiveEcsWorld?.EventBus.Unsubscribe(this);
    }
}

public class EcsEventBus
{
    private readonly ConcurrentStack<IEcsEvent> _eventPool = new();
    private readonly ConcurrentDictionary<URN, List<IEcsSubscriber>> _subscribers = new();
    private readonly ConcurrentQueue<IEcsEvent> _frameEvents = new();
    
    private readonly ConcurrentQueue<IEcsSubscriber> _subscribersToAdd = new();
    private readonly ConcurrentQueue<IEcsSubscriber> _subscribersToRemove = new();
    
    public void Subscribe(IEcsSubscriber subscriber)
    {
        _subscribersToAdd.Enqueue(subscriber);
    }
    
    public void Unsubscribe(IEcsSubscriber subscriber)
    {
        _subscribersToRemove.Enqueue(subscriber);
    }
    
    public EcsEventBus Publish(URN eventType, int? fromEntityId = null, Action<CustomData>? dataInitializer = null)
    {
        var evt = _eventPool.TryPop(out var pooledEvent) ? pooledEvent : new EcsEvent();
        evt.EventType = eventType;
        evt.FromEntityId = fromEntityId;
        dataInitializer?.Invoke(evt.Data);
        Publish(evt);
        return this;
    }
    
    public void Publish(IEcsEvent ev)
    {
        Logger.Debug("Publishing event {0} from entity {1}", ev.EventType, ev.FromEntityId);
        if (_subscribers.TryGetValue(ev.EventType, out var list))
        {
            ReadOnlySpan<IEcsSubscriber> subscriberSpan = CollectionsMarshal.AsSpan(list);

            for (int i = 0; i < subscriberSpan.Length; i++)
            {
                subscriberSpan[i].OnEvent(ev);
            
                if (ev.IsHandled) break;
            }
        }
    
        _frameEvents.Enqueue(ev);
    }
    
    public void TickEndOfFrame()
    {
        while (_subscribersToAdd.TryDequeue(out var subscriber))
        {
            var eventType = subscriber.EventType;
            var list = _subscribers.GetOrAdd(eventType, _ => new List<IEcsSubscriber>());
            Logger.Debug("Subscribing to event {0} with priority {1}", eventType, subscriber.Priority);
            lock (list)
            {
                list.Add(subscriber);
                list.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
        }
        
        while (_subscribersToRemove.TryDequeue(out var subscriber))
        {
            var eventType = subscriber.EventType;
            if (_subscribers.TryGetValue(eventType, out var list))
            {
                lock (list)
                {
                    list.Remove(subscriber);
                }
            }
            subscriber.OnUnsubscribe();
        }
        
        if (_frameEvents.Count == 0) return;
        
        while (_frameEvents.TryDequeue(out var evt))
        {
            evt.Reset();
            _eventPool.Push(evt);
        }
        
    }
}
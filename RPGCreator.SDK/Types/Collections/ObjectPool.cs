using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.Core.Types.Internal;

public class ObjectPool<T> where T : class, ICleanable
{
    private readonly Stack<T> _stack = new();
    private readonly int _maxSize;
    private readonly Func<T>? _factory;
    public int Count => _stack.Count;
    public int RentedCount { get; private set; } = 0;
    
    public ObjectPool(Func<T>? factory = null, int maxSize = 1024)
    {
        _maxSize = Math.Max(maxSize, 0);
        _factory = factory;

        if (_maxSize <= 0)
        {
            Logger.Error("ObjectPool created with a max size of {maxSize} is not useful. " +
                      "Consider using a different pool size or factory method.", _maxSize);
        }
    }

    /// <summary>
    /// Get an object from the pool. If the pool is empty, a new instance will be created using the factory method if provided.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Rent()
    {
        RentedCount++;
        Logger.Debug("ObjectPool Rent called. RentedCount is now {RentedCount}.", RentedCount);
        return _stack.Count > 0 ? _stack.Pop() :
            _factory != null ? _factory() :
            throw new InvalidOperationException("No factory method provided to create new instances.");
    }
    
    internal void AddRented(int count)
    {
        RentedCount += count;
        Logger.Debug("ObjectPool AddRented called. RentedCount is now {RentedCount}.", RentedCount);
    }

    /// <summary>
    /// Return an object to the pool. If the pool is full, the object will be discarded.
    /// The object will be cleaned before being added back to the pool by calling <see cref="ICleanable.Clean"/>.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Return(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        
        RentedCount--;
        
        if(RentedCount < 0)
        {
            Logger.Error("ObjectPool Return called more times than Rent. RentedCount is now negative.");
            RentedCount = 0;
        }
        
        if (_stack.Count < _maxSize)
        {
            item.Clean();
            _stack.Push(item);
        }
        else
        {
            Logger.Warning("ObjectPool reached its max size of {maxSize}. Item will not be returned.", _maxSize);
            
            if( item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
    public void Clear()
    {
        while (_stack.Count > 0)
        {
            var item = _stack.Pop();
            item.Clean();
            
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
    
}
using System.Diagnostics;
using System.Linq.Expressions;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

public class TileFactory : IAssetFactory<TileInstance, TileDefinition>
{
    private readonly Func<TileDefinition, TileInstance> Constructor;
    private readonly Func<AutotileDef, AutotileInstance> AutoConstructor;
    
    private readonly ObjectPool<TileInstance> _tilePool;
    private readonly ObjectPool<AutotileInstance> _autoPool;

    public TileFactory(int maxTilePool = 1024, int maxAutoTilePool = 2048)
    {
        
        // Check if ITileInstance has a constructor that accepts ITileDef and save it.
        var tileConstructorInfo = typeof(TileInstance)
            .GetConstructor(new[] { typeof(TileDefinition) })
            ?? throw new InvalidOperationException(
                $"Type {typeof(TileInstance).Name} does not have a constructor that accepts {typeof(TileDefinition).Name}.");
        
        var param = Expression.Parameter(typeof(TileDefinition), "def");
        var newExpression = Expression.New(tileConstructorInfo, param);
        Constructor = Expression.Lambda<Func<TileDefinition, TileInstance>>(newExpression, param).Compile();
        
        // Check if AutotileInstance has a constructor that accepts AutotileDef and save it.
        var autoConstructorInfo = typeof(AutotileInstance)
            .GetConstructor(new[] { typeof(AutotileDef) })
            ?? throw new InvalidOperationException(
                $"Type {typeof(AutotileInstance).Name} does not have a constructor that accepts {typeof(AutotileDef).Name}.");
        var autoParam = Expression.Parameter(typeof(AutotileDef), "def");
        var autoNewExpression = Expression.New(autoConstructorInfo, autoParam);
        AutoConstructor = Expression.Lambda<Func<AutotileDef, AutotileInstance>>(autoNewExpression, autoParam).Compile();

        _tilePool = new ObjectPool<TileInstance>(null, maxTilePool);
        _autoPool = new ObjectPool<AutotileInstance>(null, maxAutoTilePool);
        
    }

    public TileInstance Create(TileDefinition def)
    {
        TileInstance instance;
        if (_tilePool.Count > 0)
        {
            // Rent an instance from the pool.
            instance = _tilePool.Rent();
            instance.ResetFrom(def);
        }
        else
        {
            // Create a new instance if the pool is empty.
            instance = Constructor(def);
        }
        return instance;

    }

    public ValueTask<TileInstance> CreateAsync(TileDefinition def, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public void Refresh(TileDefinition def)
    {
        throw new NotImplementedException();
    }

    public void Release(TileDefinition def)
    {
        throw new NotImplementedException();
    }

    public AutotileInstance Create(AutotileDef def)
    {
        AutotileInstance instance;
        if (_autoPool.Count > 0)
        {
            // Rent an instance from the pool.
            instance = _autoPool.Rent();
            instance.ResetFrom(def);
        }
        else
        {
            // Create a new instance if the pool is empty.
            instance = AutoConstructor(def);
        }

        return instance;
    }
    
    public ITileInstance Create(ITileDef def)
    {
        ITileInstance instance;

        switch (def)
        {
            case TileDefinition tileDef:
            {
                return Create(tileDef);
            }
            case AutotileDef autoDef:
            {
                return Create(autoDef);
            }
            default:
            {
                throw new ArgumentException($"Unsupported TileDef type: {def.GetType().Name}");
            }
        }

    }

    public void Release(ITileInstance instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        // Return the instance to the appropriate pool for reuse.
        switch (instance)
        {
            case TileInstance tileInstance:
                Release(tileInstance);
                break;
            case AutotileInstance autoTileInstance:
                Release(autoTileInstance);
                break;
            default:
                throw new ArgumentException($"Unsupported TileInstance type: {instance.GetType().Name}");
        }
    }

    public void Release(TileInstance instance)
    {
        if(instance == null) throw new ArgumentNullException(nameof(instance));
        
        _tilePool.Return(instance);
    }
    
    public void Release(AutotileInstance instance)
    {
        if(instance == null) throw new ArgumentNullException(nameof(instance));
        
        _autoPool.Return(instance);
    }

    public void Clear()
    {
        // Clear both pools.
        _tilePool.Clear();
        _autoPool.Clear();
    }
}
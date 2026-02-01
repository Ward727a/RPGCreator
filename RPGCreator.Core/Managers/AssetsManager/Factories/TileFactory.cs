using System.Linq.Expressions;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

public class TileFactory : IAssetFactory<TileInstance, TileDefinition>
{
    private readonly Func<TileDefinition, TileInstance> Constructor;
    
    private readonly ObjectPool<TileInstance> _tilePool;

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
        
    }

    public TileInstance Create(TileDefinition def)
    {
        TileInstance instance;
        // Rent an instance from the pool.
        instance = _tilePool.Rent();
        instance.ResetFrom(def);
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
    
    public ITileInstance Create(ITileDef def)
    {
        ITileInstance instance;

        switch (def)
        {
            case TileDefinition tileDef:
            {
                return Create(tileDef);
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
            default:
                throw new ArgumentException($"Unsupported TileInstance type: {instance.GetType().Name}");
        }
    }

    public void Release(TileInstance instance)
    {
        if(instance == null) throw new ArgumentNullException(nameof(instance));
        
        _tilePool.Return(instance);
    }

    public void Clear()
    {
        // Clear both pools.
        _tilePool.Clear();
    }
}
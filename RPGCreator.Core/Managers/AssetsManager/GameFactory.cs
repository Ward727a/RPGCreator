using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.Layers;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.EngineService;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager;

public class GameFactory : IGameFactory
{
    private class FactoryStrategy
    {
        public Func<IBaseAssetDef, object> Create { get; init; } = null!;
        public Func<IBaseAssetDef, CancellationToken, ValueTask<object>> CreateAsync { get; init; } = null!;
        public Action<object> ReleaseInstance { get; init; } = null!;
        public Action<IBaseAssetDef> Refresh { get; init; } = null!;
        public Action<IBaseAssetDef> Release { get; init; } = null!;
    }
    
    private readonly Dictionary<Type, FactoryStrategy> _defFactoryStrategies = new();
    private readonly Dictionary<Type, FactoryStrategy> _instStrategies = new();
    
    private readonly List<Action> _clearActions = new();
    
    public GenericPooledFactory<TileLayerInstance, TileLayerDefinition> TileLayerFactory = new();
    public GenericCachedFactory<MapInstance, MapDefinition> MapFactory = new();
    public TilesetFactory TilesetFactory { get; } = new();
    public TileFactory TileFactory { get; } = new();
    public StatFactory StatFactory { get; } = new();
    public AnimationFactory AnimationFactory { get; } = new();

    public GameFactory()
    {
        // Register built-in factories
        Register(TileLayerFactory);
        Register(MapFactory);
        Register(TilesetFactory);
        Register(TileFactory);
        Register(StatFactory);
        Register(AnimationFactory);
    }
    
    public void Register<TInst, TDef>(IAssetFactory<TInst, TDef> factory) 
    where TInst : class
    where TDef : IBaseAssetDef
    {
        var type = typeof(TDef);
        var instType = typeof(TInst);
        if (_defFactoryStrategies.ContainsKey(type))
        {
            throw new InvalidOperationException($"Factory for type {type.Name} is already registered.");
        }

        var strategy = new FactoryStrategy
        {
            Create = (def) => factory.Create((TDef)def),

            CreateAsync = async (def, ct) => await factory.CreateAsync((TDef)def, ct),
            ReleaseInstance = (instance) => factory.Release((TInst)instance),

            Refresh = (def) => factory.Refresh((TDef)def),
            Release = (def) => factory.Release((TDef)def)
        };
        
        _defFactoryStrategies[type] = strategy;
        _instStrategies[instType] = strategy;

        _clearActions.Add(factory.Clear);
    }
    public TInst CreateInstance<TInst>(IBaseAssetDef def) where TInst : class
    {
        if (_defFactoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            return (TInst)strategy.Create(def);
        }
        throw new InvalidOperationException($"No factory registered for definition type {def.GetType().Name}");
    }

    public async ValueTask<TInst> CreateInstanceAsync<TInst>(IBaseAssetDef def, CancellationToken ct = default) where TInst : class
    {
        if (_defFactoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            var result = await strategy.CreateAsync(def, ct);
            return (TInst)result;
        }
        throw new InvalidOperationException($"No factory registered for definition type {def.GetType().Name}");
    }
    
    public void ReleaseInstance<TInst>(TInst instance) where TInst : class
    {
        var instType = instance.GetType();

        if (_instStrategies.TryGetValue(instType, out var strategy))
        {
            strategy.ReleaseInstance(instance);
            return;
        }
        Log.Warning($"Tried to release instance of type {instance.GetType().Name} but no factory was found.");
    }

    public void Release(IBaseAssetDef def)
    {
        if (_defFactoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            strategy.Release(def);
            return;
        }
        Log.Warning($"Tried to release asset {def.Unique} but no factory was found.");
    }

    public void Refresh(IBaseAssetDef def)
    {
        if (_defFactoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            strategy.Refresh(def);
        }
    }

    public void ClearAll()
    {
        foreach (var clearAction in _clearActions)
        {
            clearAction();
        }
    }
}
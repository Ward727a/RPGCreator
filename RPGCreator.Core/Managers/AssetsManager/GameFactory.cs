using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Assets.Animations;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.Layers;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types.Interfaces;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager;

public class GameFactory : IGameFactory
{
    private class FactoryStrategy
    {
        public Func<IAssetDef, object> Create { get; init; } = null!;
        public Func<IAssetDef, CancellationToken, ValueTask<object>> CreateAsync { get; init; } = null!;
        public Action<object> ReleaseInstance { get; init; } = null!;
        public Action<IAssetDef> Refresh { get; init; } = null!;
        public Action<IAssetDef> Release { get; init; } = null!;
    }
    
    private readonly Dictionary<System.Type, FactoryStrategy> _defFactoryStrategies = new();
    private readonly Dictionary<System.Type, FactoryStrategy> _instStrategies = new();
    
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
        Register<TileLayerInstance, TileLayerDefinition>(TileLayerFactory);
        Register<MapInstance, MapDefinition>(MapFactory);
        Register<TilesetInstance, TilesetDef>(TilesetFactory);
        Register<TileInstance, TileDefinition>(TileFactory);
        Register<StatInstance, IStatDef>(StatFactory);
        Register<AnimationInstance, AnimationDef>(AnimationFactory);
    }
    
    public void Register<TInst, TDef>(IAssetFactory<TInst, TDef> factory) 
    where TInst : class
    where TDef : IAssetDef
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
    public TInst CreateInstance<TInst>(IAssetDef def) where TInst : class
    {
        if (_defFactoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            return (TInst)strategy.Create(def);
        }
        throw new InvalidOperationException($"No factory registered for definition type {def.GetType().Name}");
    }

    public async ValueTask<TInst> CreateInstanceAsync<TInst>(IAssetDef def, CancellationToken ct = default) where TInst : class
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

    public void Release(IAssetDef def)
    {
        if (_defFactoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            strategy.Release(def);
            return;
        }
        Log.Warning($"Tried to release asset {def.Unique} but no factory was found.");
    }

    public void Refresh(IAssetDef def)
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
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Map;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager;

public class GameFactory
{
    private class FactoryStrategy
    {
        public Func<IAssetDef, object> Create { get; init; } = null!;
        public Func<IAssetDef, CancellationToken, ValueTask<object>> CreateAsync { get; init; } = null!;
        public Action<IAssetDef> Refresh { get; init; } = null!;
        public Action<IAssetDef> Release { get; init; } = null!;
    }
    
    private readonly Dictionary<System.Type, FactoryStrategy> _factoryStrategies = new();
    
    private readonly List<Action> _clearActions = new();
    
    public GenericPooledFactory<TileLayerInstance, TileLayerDefinition> TileLayerFactory = new();
    public GenericCachedFactory<MapInstance, MapDefinition> MapFactory = new();
    public TilesetFactory TilesetFactory { get; } = new();
    public TileFactory TileFactory { get; } = new();
    public StatFactory StatFactory { get; } = new();

    public GameFactory()
    {
        // Register built-in factories
        Register<TileLayerInstance, TileLayerDefinition>(TileLayerFactory);
        Register<MapInstance, MapDefinition>(MapFactory);
        Register<TilesetInstance, TilesetDef>(TilesetFactory);
        Register<AutoTilesetInstance, AutoTilesetDef>(TilesetFactory);
        Register<TileInstance, TileDefinition>(TileFactory);
        Register<StatInstance, IStatDef>(StatFactory);
    }
    
    public void Register<TInst, TDef>(IAssetFactory<TInst, TDef> factory) 
    where TInst : class
    where TDef : IAssetDef
    {
        var type = typeof(TDef);
        if (_factoryStrategies.ContainsKey(type))
        {
            throw new InvalidOperationException($"Factory for type {type.Name} is already registered.");
        }

        // 1. On crée la stratégie (les lambdas font le cast)
        var strategy = new FactoryStrategy
        {
            // Create : On cast l'entrée (IAssetDef -> TDef) et on retourne object
            Create = (def) => factory.Create((TDef)def),

            // CreateAsync : Attention, il faut await pour caster le résultat ValueTask<T> en object
            CreateAsync = async (def, ct) => await factory.CreateAsync((TDef)def, ct),

            // Refresh & Release : On cast juste l'entrée
            Refresh = (def) => factory.Refresh((TDef)def),
            Release = (def) => factory.Release((TDef)def)
        };
        
        _factoryStrategies[type] = strategy;

        _clearActions.Add(factory.Clear);
    }
    public TInst CreateInstance<TInst>(IAssetDef def) where TInst : class
    {
        if (_factoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            return (TInst)strategy.Create(def);
        }
        throw new InvalidOperationException($"No factory registered for definition type {def.GetType().Name}");
    }

    public async ValueTask<TInst> CreateInstanceAsync<TInst>(IAssetDef def, CancellationToken ct = default) where TInst : class
    {
        if (_factoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            var result = await strategy.CreateAsync(def, ct);
            return (TInst)result;
        }
        throw new InvalidOperationException($"No factory registered for definition type {def.GetType().Name}");
    }

    public void Release(IAssetDef def)
    {
        if (_factoryStrategies.TryGetValue(def.GetType(), out var strategy))
        {
            strategy.Release(def);
            return;
        }
        Log.Warning($"Tried to release asset {def.Unique} but no factory was found.");
    }

    public void Refresh(IAssetDef def)
    {
        if (_factoryStrategies.TryGetValue(def.GetType(), out var strategy))
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
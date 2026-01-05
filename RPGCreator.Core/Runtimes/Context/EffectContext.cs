using Microsoft.Xna.Framework;
using RPGCreator.Core.Runtimes.Services;
using RPGCreator.Core.Types.Assets.Actors;
using RPGCreator.Core.Types.Assets.Entities;
using RPGCreator.Core.Types.Assets.Items;

namespace RPGCreator.Core.Runtimes.Context;

public sealed class EffectContext : IEffectContext
{
    public Actor Target { get; }
    public object? Source { get; }
    public Random Rng { get; } = new Random();
    public GameTime GameTime { get; }

    public EffectContext
    (
        Actor target,
        GameTime gameTime,
        object? source = null
    )
    {
        Target = target;
        GameTime = gameTime;
        Source = source;
    }
    
    public TService? Resolve<TService>() where TService : IRuntimeService
    {
        // return EngineCore.Instance.ServiceProvider.GetService<TService>();
        return default;
    }
}
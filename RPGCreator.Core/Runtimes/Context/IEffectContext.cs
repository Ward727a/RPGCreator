using Microsoft.Xna.Framework;
using RPGCreator.Core.Runtimes.Services;
using RPGCreator.Core.Type.Assets.Actors;
using RPGCreator.Core.Type.Assets.Items;

namespace RPGCreator.Core.Runtimes.Context;

public interface IEffectContext
{
    IActor Target { get; }
    object? Source { get; }
    Random Rng { get; }
    GameTime GameTime { get; }
    TService Resolve<TService>() where TService : IRuntimeService;
}
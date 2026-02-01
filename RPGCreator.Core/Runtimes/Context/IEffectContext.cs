using Microsoft.Xna.Framework;
using RPGCreator.Core.Runtimes.Services;
using RPGCreator.Core.Types.Assets.Entities;

namespace RPGCreator.Core.Runtimes.Context;

public interface IEffectContext
{
    Actor Target { get; }
    object? Source { get; }
    Random Rng { get; }
    GameTime GameTime { get; }
    TService? Resolve<TService>() where TService : IRuntimeService;
}
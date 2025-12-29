using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.SDK.Assets.Definitions.Characters.Stats;

namespace RPGCreator.Core.Runtimes.Services.Actors;

public sealed class StatService : IStatService
{
    public float GetStat(CharacterActor target, string statName)
    {
        throw new NotImplementedException();
    }

    public void ModifyBaseStat(CharacterActor target, string statName, float value)
    {
        throw new NotImplementedException();
    }

    public void AddModifier(CharacterActor target, IStatModifier modifier)
    {
        throw new NotImplementedException();
    }

    public void RemoveModifier(CharacterActor target, IStatModifier modifier)
    {
        throw new NotImplementedException();
    }

}
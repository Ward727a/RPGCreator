using RPGCreator.Core.Types.Assets.Entities.Characters;
using RPGCreator.SDK.Assets.Definitions.Stats;

namespace RPGCreator.Core.Runtimes.Services.Actors;


public interface IStatService : IActorService
{
    float GetStat(CharacterActor target, string statName);
    void ModifyBaseStat(CharacterActor target, string statName, float value);
    void AddModifier(CharacterActor target, IStatModifier modifier);
    void RemoveModifier(CharacterActor target, IStatModifier modifier);
}
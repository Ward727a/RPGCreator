using RPGCreator.Core.Types.Assets.Actors;
using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Assets.Items;
using RPGCreator.Core.Types.Assets.Effect;

namespace RPGCreator.Core.Runtimes.Services.Actors;


public interface IStatService : IActorService
{
    float GetStat(CharacterActor target, string statName);
    void ModifyBaseStat(CharacterActor target, string statName, float value);
    void AddModifier(CharacterActor target, IStatModifier modifier);
    void RemoveModifier(CharacterActor target, IStatModifier modifier);
}
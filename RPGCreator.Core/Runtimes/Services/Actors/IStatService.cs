using RPGCreator.Core.Type.Assets.Actors;
using RPGCreator.Core.Type.Assets.Characters;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Assets.Items;
using RPGCreator.Core.Type.Assets.Effect;

namespace RPGCreator.Core.Runtimes.Services.Actors;


public interface IStatService : IActorService
{
    float GetStat(CharacterActor target, string statName);
    void ModifyBaseStat(CharacterActor target, string statName, float value);
    void AddModifier(CharacterActor target, IStatModifier modifier);
    void RemoveModifier(CharacterActor target, IStatModifier modifier);
}
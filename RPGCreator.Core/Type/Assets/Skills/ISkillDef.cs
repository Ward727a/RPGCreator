using RPGCreator.Core.Parser.PRATT;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Skills;

public interface ISkillDef : IHasSavePath, IHasUniqueId, ISerializable, IDeserializable
{
    public Ulid? PackId { get; set; }
    public string Name { get; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public Dictionary<IStatDef, float> Cost { get; set; } // Stat and amount to consume (Stat need to be a ressource kind!)
    public float Cooldown { get; set; }
    public ESkillTargetType TargetType { get; set; }
    public float Range { get; set; } // Number of units the skill can reach from the original target
    
    public PrattCompiledFormula? SkillScalingFormula { get; set; }
    public string SkillNonCompiledFormula { get; set; }
}
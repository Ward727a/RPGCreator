using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Skills;

public interface ISkillDef : IBaseAssetDef, IHasSavePath, ISerializable, IDeserializable
{
    public Ulid? PackId { get; set; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public Dictionary<IStatDef, float> Cost { get; set; } // Stat and amount to consume (Stat need to be a ressource kind!)
    public float Cooldown { get; set; }
    public ESkillTargetType TargetType { get; set; }
    public float Range { get; set; } // Number of units the skill can reach from the original target
    public List<URN> EffectsUrn { get; set; } // List of URNs of the effects to apply when the skill is used
    
    public IPrattFormula? SkillScalingFormula { get; set; }
    public string SkillNonCompiledFormula { get; set; }
    
    public void SetName(string name);
}
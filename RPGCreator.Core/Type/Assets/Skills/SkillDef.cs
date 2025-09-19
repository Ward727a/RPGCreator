using RPGCreator.Core.Parser.PRATT;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Skills;

public class SkillDef : ISkillDef
{
    public string SavePath { get; set; }
    public Ulid Unique { get; }
    public URN Urn { get; private set; }
    public Ulid? PackId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public Dictionary<IStatDef, float> Cost { get; set; } = new();
    public float Cooldown { get; set; }
    public ESkillTargetType TargetType { get; set; }
    public float Range { get; set; }
    public PrattCompiledFormula? SkillScalingFormula { get; set; }
    public string SkillNonCompiledFormula { get; set; } = string.Empty;
    
    public SkillDef()
    {
    }

    public SkillDef(string name) : this()
    {
        Unique = Ulid.NewUlid();
        Name = name;
        Urn = new URN("skill", $"{Name}@{Unique}");
    }
    
    public void SetName(string name)
    {
        Name = name;
        Urn = new URN("skill", $"{Name}@{Unique}");
    }

    public SerializationInfo GetObjectData()
    {
        throw new NotImplementedException();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        throw new NotImplementedException();
    }
}
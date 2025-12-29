using RPGCreator.Core.Parser.PRATT;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Characters.Stats;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Types.Assets.Skills;

public class SkillDef : ISkillDef
{
    public string SavePath { get; set; }
    public Ulid Unique { get; private set; } = Ulid.NewUlid();
    public URN Urn { get; private set; }
    public Ulid? PackId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public Dictionary<IStatDef, float> Cost { get; set; } = new();
    public float Cooldown { get; set; }
    public ESkillTargetType TargetType { get; set; }
    public float Range { get; set; }
    public List<URN> EffectsURN { get; set; } = new();
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
        var info = new SerializationInfo(typeof(SkillDef));
        info.AddValue("SavePath", SavePath);
        info.AddValue("Unique", Unique.ToString());
        info.AddValue("PackId", PackId?.ToString() ?? string.Empty);
        info.AddValue("Name", Name);
        info.AddValue("Description", Description);
        info.AddValue("IconPath", IconPath);
        
        var costDict = new Dictionary<string, float>();
        foreach (var (stat, amount) in Cost)
        {
            costDict[stat.Urn.ToString()] = amount;
        }
        info.AddValue("Cost", costDict);
        info.AddValue("Cooldown", Cooldown);
        info.AddValue("TargetType", (int)TargetType);
        info.AddValue("Range", Range);
        info.AddValue("SkillNonCompiledFormula", SkillNonCompiledFormula);
        // Note: We do not serialize the compiled formula, as it can be recompiled from the non-compiled formula.
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("SavePath", out string? savePath);
        SavePath = savePath ?? string.Empty;
        info.TryGetValue("Unique", out string? uniqueStr);
        if (uniqueStr != null && Ulid.TryParse(uniqueStr, out Ulid unique))
        {
            Unique = unique;
        }
        info.TryGetValue("PackId", out string? packIdStr);
        if (packIdStr != null && Ulid.TryParse(packIdStr, out Ulid packId))
        {
            PackId = packId;
        }
        info.TryGetValue("Name", out string? name);
        Name = name ?? string.Empty;
        info.TryGetValue("Description", out string? description);
        Description = description ?? string.Empty;
        info.TryGetValue("IconPath", out string? iconPath);
        IconPath = iconPath ?? string.Empty;
        info.TryGetDictionary("Cost", out Dictionary<string, float> costDict);
        if (costDict != null)
        {
            Cost.Clear();
            foreach (var (statUrnStr, amount) in costDict)
            {
                var statUrn = URN.Parse(statUrnStr);
                var statDef = EngineCore.Instance.Managers.Assets.TryResolveAsset(statUrn, out IStatDef? resolvedStatDef) ? resolvedStatDef : null;
                if (statDef != null)
                {
                    Cost[statDef] = amount;
                }
            }
        }
        info.TryGetValue("Cooldown", out float cooldown);
        Cooldown = cooldown;
        info.TryGetValue("TargetType", out int targetTypeInt);
        TargetType = (ESkillTargetType)targetTypeInt;
        info.TryGetValue("Range", out float range);
        Range = range;
        info.TryGetValue("SkillNonCompiledFormula", out string? skillNonCompiledFormula);
        SkillNonCompiledFormula = skillNonCompiledFormula ?? string.Empty;
        // Note: We do not deserialize the compiled formula, as it can be recompiled from the non-compiled formula.
        Urn = new URN("skill", $"{Name}@{Unique}");
    }

}
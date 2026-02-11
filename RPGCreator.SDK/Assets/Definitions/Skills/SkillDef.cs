using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Parser.PrattFormula;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Skills;

[SerializingType("SkillDef")]
public class SkillDef : BaseAssetDef, ISkillDef
{
    public string SavePath { get; set; }
    public override UrnSingleModule UrnModule => "skill".ToUrnSingleModule();
    public Ulid? PackId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public Dictionary<IStatDef, float> Cost { get; set; } = new();
    public float Cooldown { get; set; }
    public ESkillTargetType TargetType { get; set; }
    public float Range { get; set; }
    public List<URN> EffectsUrn { get; set; } = new();
    public IPrattFormula? SkillScalingFormula { get; set; }
    public string SkillNonCompiledFormula { get; set; } = string.Empty;
    
    public SkillDef()
    {
        SuspendTracking();
    }

    public SkillDef(string name) : this()
    {
        Unique = Ulid.NewUlid();
        Name = name;
    }
    
    public void SetName(string name)
    {
        Name = name;
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
        
        info.AddValue("Cooldown", Cooldown);
        info.AddValue("TargetType", (int)TargetType);
        info.AddValue("Range", Range);
        info.AddValue("SkillNonCompiledFormula", SkillNonCompiledFormula);
        // Note: We do not serialize the compiled formula, as it can be recompiled from the non-compiled formula.
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        foreach (var statDef in Cost.Keys)
        {
            referencedIds.Add(statDef.Unique);
        }
        return referencedIds;
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
        info.TryGetDictionary("Cost", out Dictionary<string, float>? costDict);
        if (costDict != null)
        {
            Cost.Clear();
            foreach (var (statUrnStr, amount) in costDict)
            {
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

        if (!string.IsNullOrEmpty(SkillNonCompiledFormula))
        {
            if(EngineServices.PrattFormulaService.TryCompile(SkillNonCompiledFormula, out IPrattFormula? compiledFormula))
            {
                SkillScalingFormula = compiledFormula;
            }
        }
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
}
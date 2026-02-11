using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Characters;

#region Should be moved to a more appropriate location

/// <summary>
/// Basic character stats structure. <br/>
/// This structure is just a simple container for now, but it will be changed and extended in the future.
/// </summary>
public class CharacterStats : ISerializable, IDeserializable
{
    public Ulid Unique => StatDef.Unique;
    public IStatDef StatDef { get; private set; }
    public double CurrentValue { get; set; } 
    public double MaxValue { get; set; } 
    public double MinValue { get; set; } 

    public IStatDef GenerateDefinition()
    {
        return null;
        // if(Math.Abs(CurrentValue - def.DefaultValue) < 0.001 && Math.Abs(MaxValue - def.CapSettings.CapValue) < 0.001 && Math.Abs(MinValue - def.MinValue) < 0.001)
        // {
        //     return StatDef;
        // }
        
        // var statDef = new BaseStatDefinition
        // {
        //     Name = StatDef.Name,
        //     Description = StatDef.Description,
        //     DefaultValue = CurrentValue,
        //     StatTypeKind = StatDef.StatTypeKind,
        //     StatMinValue = MinValue,
        //     StatCapSettings = new CapSettings()
        //     {
        //         StatCapType = EStatTypeCap.ByValue,
        //         StatCapValue = MaxValue,
        //         StatCapStatUnique = StatDef.StatCapSettings.StatCapStatUnique,
        //     },
        //     IsVisible = StatDef.IsVisible,
        //     PackId = StatDef.PackId,
        //     StatCompiledFormula = StatDef.StatCompiledFormula,
        //     StatNonCompiledFormula = StatDef.StatNonCompiledFormula,
        //     SavePath = StatDef.SavePath
        // };
        //
        // foreach (var graphDocumentCompiled in StatDef.GetAllEvents())
        // {
        //     statDef.AddEvent(graphDocumentCompiled.Key, graphDocumentCompiled.Value);
        // }
        //
        // return statDef;
    }

    public CharacterStats()
    {
    }
    
    public bool IsStatDefDifferent(IStatDef otherDef)
    {
        if(otherDef.Unique != StatDef.Unique) return false;
        if(Math.Abs(otherDef.DefaultValue - StatDef.DefaultValue) > 0.001) return true;
        if(Math.Abs(otherDef.CapSettings.CapValue - StatDef.CapSettings.CapValue) > 0.001) return true;
        if(Math.Abs(otherDef.MinValue - StatDef.MinValue) > 0.001) return true;
        if(otherDef.IsVisible != StatDef.IsVisible) return true;
        if(otherDef.PackId != StatDef.PackId) return true;
        if(otherDef.Name != StatDef.Name) return true;
        if(otherDef.Description != StatDef.Description) return true;
        if(otherDef.TypeKind != StatDef.TypeKind) return true;
        if(otherDef.CapSettings.CapType != StatDef.CapSettings.CapType) return true;
        if(otherDef.CapSettings.CapStatUnique != StatDef.CapSettings.CapStatUnique) return true;
        if(otherDef.StatNonCompiledFormula != StatDef.StatNonCompiledFormula) return true;
        if(otherDef.GetAllEvents() != StatDef.GetAllEvents()) return true;
        return false;
    }

    public void SetDef(IStatDef newDef)
    {
        if (newDef.Unique != StatDef.Unique)
        {
            throw new InvalidOperationException("Cannot set CharacterStats with a different StatDefinition unique ID.");
        }
        var oldDef = StatDef;
        StatDef = newDef;
        
        if(Math.Abs(MaxValue - oldDef.CapSettings.CapValue) < 0.001)
        {
            MaxValue = newDef.CapSettings.CapValue;
        }

        if (Math.Abs(MinValue - oldDef.MinValue) < 0.001)
        {
            MinValue = newDef.MinValue;
        }
        if (Math.Abs(CurrentValue - oldDef.DefaultValue) < 0.001)
        {
            CurrentValue = newDef.DefaultValue;
        }
    }
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterStats))
            .AddValue("StatDefId", StatDef.Unique)
            .AddValue("CurrentValue", CurrentValue)
            .AddValue("MaxValue", MaxValue)
            .AddValue("MinValue", MinValue);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [StatDef.Unique];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("CurrentValue", out float current, 0);
        info.TryGetValue("MaxValue", out float max, 0);
        info.TryGetValue("MinValue", out float min, 0);
        
        CurrentValue = current;
        MaxValue = max;
        MinValue = min;

        info.TryGetValue("StatDefId", out Ulid statId, Ulid.Empty);
        if (statId != Ulid.Empty)
        {
            if (EngineServices.AssetsManager.TryResolveAsset<IStatDef>(statId, out var def))
            {
                StatDef = def;
            }
        }
    }
}

public class CharacterSkill(ISkillDef def) : ISerializable, IDeserializable
{
    public Ulid Unique => SkillDef.Unique;
    public ISkillDef SkillDef { get; private set; } = def;
    
    public int SkillLevel { get; set; } = 1;
    public bool HasMaxLevel { get; set; } = false;
    /// <summary>
    /// If <see cref="HasMaxLevel"/> is true, this is the max level of the skill (it will be removed if the character reaches this level)
    /// </summary>
    public int MaxSkillLevel { get; set; } = 1; 

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterSkill))
            .AddValue("SkillDefId", SkillDef.Unique)
            .AddValue("SkillLevel", SkillLevel)
            .AddValue("HasMaxLevel", HasMaxLevel)
            .AddValue("MaxSkillLevel", MaxSkillLevel);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [SkillDef.Unique];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("SkillLevel", out int level, 1);
        info.TryGetValue("HasMaxLevel", out bool hasMax, false);
        info.TryGetValue("MaxSkillLevel", out int maxLevel, 1);
        
        SkillLevel = level;
        HasMaxLevel = hasMax;
        MaxSkillLevel = maxLevel;

        info.TryGetValue("SkillDefId", out Ulid skillId, Ulid.Empty);
        if (skillId != Ulid.Empty)
        {
            if (EngineServices.AssetsManager.TryResolveAsset<ISkillDef>(skillId, out var def))
            {
                SkillDef = def;
            }
        }
    }
}

#endregion

[SerializingType("DirectionalAnimationSet")]
public class DirectionalAnimationSet : ISerializable, IDeserializable
{

    /// <summary>
    /// Indicates the maximum number of directions supported by this animation set. <br/>
    /// Eight Directions => 8, or 4, or 1. <br/>
    /// Four Directions => 4, or 1. <br/>
    /// Single Direction => 1. <br/>
    /// </summary>
    public AnimationDirection MaxDirections = AnimationDirection.EightDirections;
    
    /// <summary>
    /// Key: Direction <br/>
    /// Value: Animation Unique ID <br/>
    /// </summary>
    public Dictionary<int, Ulid> Animations { get; private set; } = new();
    
    public void SetAnimation(EntityDirection direction, Ulid animationUnique)
    {
        Animations[direction.ToInt()] = animationUnique;
    }
    
    public Ulid GetAnimation(EntityDirection direction)
    {
        if (Animations.TryGetValue(direction.ToInt(), out var animationUnique))
        {
            return animationUnique;
        }
        return Ulid.Empty;
    }
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(DirectionalAnimationSet))
            .AddValue("Animations", Animations);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return Animations.Values.ToList();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Animations", out Dictionary<int, Ulid>? animations);
        if (animations != null)
        {
            Animations = animations;
        }
    }
}

/// <summary>
/// This structure contains the role-play information of a character. <br/>
/// It is used to define the character's background, personality, and other role-play related information.<br/>
/// This doesn't affect the gameplay directly, but it is used to enhance the role-play experience.<br/>
/// This structure is just a simple container for the role-play information, but it will be changed and extended in the future.
/// </summary>
[SerializingType("CharacterRolePlayInfo")]
public struct CharacterRolePlayInfo() : ISerializable, IDeserializable
{
    public string Description { get; set; } = string.Empty;
    public string Backstory { get; set; } = string.Empty;
    public string PersonalityTraits { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public int Age { get; set; } = 18; // Default age set to 18
    /// <summary>
    /// Breed AKA Race, but well, code editor don't like the word "Race" so we use "Breed" to avoid the auto-completion issues. <br/>
    /// This is stupid, I know, but it is what it is. <br/>
    /// This property is used to define the character race such as "Human", "Elf", "Dwarf", etc.
    /// </summary>
    public string Breed { get; set; } = string.Empty; // Default Breed (race) (could be "Human", "Elf", etc.)
    public string Faction { get; set; } = string.Empty; // Default faction (could be "Hero", "Villain", etc.)
    public string Alignment { get; set; } = string.Empty; // Default alignment (could be "Good", "Evil", etc.)
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterRolePlayInfo))
            .AddValue("Description", Description)
            .AddValue("Backstory", Backstory)
            .AddValue("PersonalityTraits", PersonalityTraits)
            .AddValue("FirstName", FirstName)
            .AddValue("LastName", LastName)
            .AddValue("Nickname", Nickname)
            .AddValue("Age", Age)
            .AddValue("Breed", Breed)
            .AddValue("Faction", Faction)
            .AddValue("Alignment", Alignment);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return new List<Ulid>();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Description", out string description, string.Empty);
        info.TryGetValue("Backstory", out string backstory, string.Empty);
        info.TryGetValue("PersonalityTraits", out string personalityTraits, string.Empty);
        info.TryGetValue("FirstName", out string firstName, string.Empty);
        info.TryGetValue("LastName", out string lastName, string.Empty);
        info.TryGetValue("Nickname", out string nickname, string.Empty);
        info.TryGetValue("Age", out int age, 18);
        info.TryGetValue("Breed", out string breed, string.Empty);
        info.TryGetValue("Faction", out string faction, string.Empty);
        info.TryGetValue("Alignment", out string alignment, string.Empty);

        Description = description;
        Backstory = backstory;
        PersonalityTraits = personalityTraits;
        FirstName = firstName;
        LastName = lastName;
        Nickname = nickname;
        Age = age;
        Breed = breed;
        Faction = faction;
        Alignment = alignment;
    }
}

[SerializingType("CharacterEquipSlot")]
public struct CharacterEquipSlot(string slotName, int slotIndex, string itemType, string itemId = "") : ISerializable, IDeserializable
{
    public string SlotName { get; set; } = slotName; // Name of the slot (e.g., "Head", "Chest", "Legs", etc.)
    public int SlotIndex { get; set; } = slotIndex; // Index of the slot (e.g., 0 for Head, 1 for Chest, etc.)
    public string ItemId { get; set; } = itemId; // ID of the item equipped in this slot (if any)
    public string ItemType { get; set; } = itemType; // Type of the item (e.g., "Weapon", "Armor", etc.)
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterEquipSlot))
            .AddValue("SlotName", SlotName)
            .AddValue("SlotIndex", SlotIndex)
            .AddValue("ItemId", ItemId)
            .AddValue("ItemType", ItemType);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("SlotName", out string slotName, string.Empty);
        info.TryGetValue("SlotIndex", out int slotIndex, 0);
            info.TryGetValue("ItemId", out string itemId, string.Empty);
        info.TryGetValue("ItemType", out string itemType, string.Empty);

        SlotName = slotName;
        SlotIndex = slotIndex;
        ItemId = itemId;
        ItemType = itemType;
    }
}

[SerializingType("EntityFeatureData")]
public class EntityFeatureData() : ISerializable, IDeserializable
{
    public EntityFeatureData(URN featureUrn, CustomData configuration) : this()
    {
        FeatureUrn = featureUrn;
        Configuration = configuration;
    }
    private static readonly ScopedLogger Logger = Logging.Logger.ForContext<EntityFeatureData>();
    
    public Ulid InstanceId { get; set; } = Ulid.NewUlid();
    public URN FeatureUrn { get; set; }
    public CustomData Configuration { get; set; }
    
    /// <summary>
    /// Define if this feature has been added by another feature or macro-features.
    /// </summary>
    public bool IsSubFeature;
    
    /// <summary>
    /// If the <see cref="IsSubFeature"/> is true, this contain the URN of the parent feature that added this feature.<br/>
    /// Else it is <see cref="Ulid.Empty"/>.<br/>
    /// </summary>
    public Ulid ParentFeatureId = Ulid.Empty;
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(EntityFeatureData))
            .AddValue("InstanceId", InstanceId)
            .AddValue("FeatureUrn", FeatureUrn)
            .AddValue("Configuration", Configuration)
            .AddValue(nameof(IsSubFeature), IsSubFeature)
            .AddValue(nameof(ParentFeatureId), ParentFeatureId);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return new List<Ulid>();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("InstanceId", out Ulid instanceId, Ulid.Empty);
        info.TryGetValue("FeatureUrn", out URN featureUrn, URN.Empty);
        info.TryGetValue("Configuration", out CustomData configuration, new CustomData());
        info.TryGetValue(nameof(IsSubFeature), out bool isSubFeature, false);
        info.TryGetValue(nameof(ParentFeatureId), out Ulid parentFeatureId, Ulid.Empty);
        
        if(instanceId == Ulid.Empty)
            Logger.Error("InstanceId key is missing or invalid during deserialization of CharacterFeatureData.");
        
        if(featureUrn == URN.Empty)
            Logger.Error("FeatureUrn key is missing or invalid during deserialization of CharacterFeatureData.");
        
        InstanceId = instanceId;
        FeatureUrn = featureUrn;
        Configuration = configuration;
        IsSubFeature = isSubFeature;
        ParentFeatureId = parentFeatureId;
    }
    
    public IEntityFeature ToEntityFeature()
    {
        var feature = EngineServices.FeaturesManager.CreateEntityFeatureInstance(FeatureUrn);
        feature.SetConfiguration(Configuration, new EngineSecurityToken());
        return feature;
    }
}

/// <summary>
/// This class represents a character in the game.
/// </summary>
[SerializingType("Character")]
public class CharacterData : BaseAssetDef, IEntityDefinition, ICharacter, ISerializable, IDeserializable
{
    public event Action? OnFeaturesChanged;
    
    public string SavePath { get; set; } = null!;

    #region Events

    public event EventHandler<string>? PortraitChanged;
    public event EventHandler<int>? LevelChanged;
    public event EventHandler<int>? MaxLevelChanged;
    public event EventHandler<int>? InitialLevelChanged;
    public event EventHandler<Ulid>? ClassChanged;
    
    #endregion
    
    #region Properties

    public string SpritePath => _portraitPath;
    public override UrnSingleModule UrnModule => "character".ToUrnSingleModule();
    public CustomData Properties { get; } = new CustomData();

    public ObservableCollection<EntityFeatureData> Features => _features;

    public List<string> Tags { get; }

    private string _portraitPath = string.Empty;
    
    public Dictionary<int, DirectionalAnimationSet> AnimationsMapping { get; private set; }= new();
    
    public DirectionalAnimationSet GetOrCreateAnimationSet(URN animationUrn)
    {
        var animIndex = EngineServices.ECS.StateRegistry.GetActionId(animationUrn);
        
        if (!AnimationsMapping.ContainsKey(animIndex))
        {
            AnimationsMapping[animIndex] = new DirectionalAnimationSet();
        }
        return AnimationsMapping[animIndex];
    }
    
    private int _initialLevel = 1;
    private int _currentLevel = 1;
    private int _maxLevel = 99;
    
    private Ulid _classId = Ulid.Empty;
    private ObservableCollection<EntityFeatureData> _features;
    
    [NotNull]
    public string? PortraitPath
    {
        get => _portraitPath;
        set
        {
            if (value == null) return;
            if (_portraitPath == value) return; // Avoid unnecessary updates
            _portraitPath = value;
            PortraitChanged?.Invoke(this, value);
        }
    }

    public int InitialLevel
    {
        get => _initialLevel;
        set
        {
            if (_initialLevel == value) return;
            _initialLevel = value;
            InitialLevelChanged?.Invoke(this, value);
        }
    }
    
    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            if (_currentLevel == value) return;
            _currentLevel = value;
            LevelChanged?.Invoke(this, value);
        }
    }
    
    public int MaxLevel
    {
        get => _maxLevel;
        set
        {
            if (_maxLevel == value) return;
            _maxLevel = value;
            MaxLevelChanged?.Invoke(this, value);
        }
    }
    
    public Ulid ClassId
    {
        get => _classId;
        set
        {
            if (_classId == value) return;
            _classId = value;
            ClassChanged?.Invoke(this, value);
        }
    }

    public HashSet<Ulid> Stats { get; private set; } = new();
    
    public Dictionary<Ulid, CharacterSkill> Skills { get; private set; } = new();
    
    public CharacterRolePlayInfo RolePlayInfo { get; private set; } = new CharacterRolePlayInfo();
    
    #endregion

    #region Constructors
    
    // Needed for serialization
    public CharacterData()
    {
        SuspendTracking();
        Name = "UNKNOWN";
        _features = new ObservableCollection<EntityFeatureData>();
        Tags = new List<string>();
    }
    
    public CharacterData(string name) : this()
    {
        Name = name;
        RefreshStats();
    }
    
    #endregion
    
    #region Methods

    public void RefreshStats()
    {

        var allStats = EngineServices.AssetsManager.GetAssets<IStatDef>();

        foreach (var statDef in allStats)
        {
            Stats.Add(statDef.Unique);
        }
    }
    
    
    /// <summary>
    /// Retrieves the feature configuration for the given URN.<br/>
    /// This method searches through the character's features and returns the configuration if found.
    /// </summary>
    /// <param name="instanceId">The instance ID of the feature to retrieve.</param>
    /// <param name="config">The output parameter that will hold the feature configuration if found.</param>
    /// <returns>
    /// True if the feature configuration was found; otherwise, false.
    /// </returns>
    public bool TryGetFeatureConfig(Ulid instanceId, [NotNullWhen(true)] out CustomData? config)
    {
        var data = _features.FirstOrDefault(f => f.InstanceId == instanceId);
        config = data?.Configuration;
        return config != null;
    }
    
    /// <summary>
    /// Retrieves the first feature configuration for the given URN.<br/>
    /// This method searches through the character's features and returns the configuration of the first matching feature.
    /// </summary>
    /// <param name="featureUrn">The URN of the feature to search for.</param>
    /// <param name="config">The output parameter that will hold the feature configuration if found.</param>
    /// <returns>
    /// True if the feature configuration was found; otherwise, false.
    /// </returns>
    public bool TryGetFirstFeatureConfig(URN featureUrn, [NotNullWhen(true)] out CustomData? config)
    {
        var data = _features.FirstOrDefault(f => f.FeatureUrn == featureUrn);
        config = data?.Configuration;
        return config != null;
    }

    /// <summary>
    /// Adds a new feature configuration to the character.<br/>
    /// This method creates a new CharacterFeatureData instance and adds it to the character's features list.<br/>
    /// It will also generate a new unique instance ID for the feature.
    /// </summary>
    /// <param name="urn">The URN of the feature to add.</param>
    /// <param name="config">The configuration data for the feature.</param>
    /// <returns>
    /// The unique instance ID of the newly added feature.
    /// </returns>
    public Ulid AddFeatureConfig(URN urn, CustomData config)
    {
        var featureData = new EntityFeatureData(urn, config);
        _features.Add(featureData);
        OnFeaturesChanged?.Invoke();
        return featureData.InstanceId;
    }

    public Ulid AddFeatureConfig(IEntityFeature feature)
    {
        return AddFeatureConfig(feature.FeatureUrn, feature.Configuration);
    }
    
    /// <summary>
    /// Removes a feature configuration from the character by its instance ID.<br/>
    /// This method searches through the character's features and removes the one with the matching instance ID.
    /// </summary>
    /// <param name="instanceId">The instance ID of the feature to remove.</param>
    /// <returns>
    /// True if the feature configuration was found and removed; otherwise, false.
    /// </returns>
    public bool RemoveFeatureConfig(Ulid instanceId)
    {
        var featureData = _features.FirstOrDefault(f => f.InstanceId == instanceId);
        if (featureData != null && _features.Remove(featureData))
        {
            OnFeaturesChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Retrieves all instance IDs of features matching the given URN.<br/>
    /// This method searches through the character's features and returns all matching instance IDs.
    /// </summary>
    /// <param name="featureUrn">The URN of the feature to search for.</param>
    /// <returns>
    /// An enumerable collection of instance IDs for the matching features.
    /// </returns>
    public IEnumerable<Ulid> GetAllFeatureInstanceIds(URN featureUrn)
    {
        return _features
            .Where(f => f.FeatureUrn == featureUrn)
            .Select(f => f.InstanceId);
    }
    
    #endregion

    #region Serialization
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterData))
            .AddValue("Unique", Unique)
            .AddValue("Name", Name)
            .AddValue("PortraitPath", PortraitPath)
            .AddValue("InitialLevel", InitialLevel)
            .AddValue("CurrentLevel", CurrentLevel)
            .AddValue("MaxLevel", MaxLevel)
            .AddValue("ClassId", ClassId)
            .AddValue("Stats", Stats)
            .AddValue("Features", Features)
            .AddValue(nameof(AnimationsMapping), AnimationsMapping)
            .AddValue("RolePlayInfo", RolePlayInfo);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var assetIds = new List<Ulid>();
        
        if (ClassId != Ulid.Empty)
        {
            assetIds.Add(ClassId);
        }

        assetIds.AddRange(Stats);
        assetIds.AddRange(Skills.Values.Select(s => s.SkillDef.Unique));
        
        foreach (var animationSet in AnimationsMapping.Values)
        {
            assetIds.AddRange(animationSet.Animations.Values);
        }
        
        return assetIds;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Unique", out Ulid unique, Ulid.Empty);
        info.TryGetValue("Name", out string name, "UNKNOWN");
        info.TryGetValue("PortraitPath", out string portraitPath, string.Empty);
        info.TryGetValue("InitialLevel", out int initialLevel, 1);
        info.TryGetValue("CurrentLevel", out int currentLevel, 1);
        info.TryGetValue("MaxLevel", out int maxLevel, 99);
        info.TryGetValue("ClassId", out Ulid classId, Ulid.Empty);
        info.TryGetValue("Stats", out HashSet<Ulid>? stats);
        info.TryGetValue(nameof(AnimationsMapping), out Dictionary<int, DirectionalAnimationSet>? animationsMapping);
        info.TryGetValue("Features", out ObservableCollection<EntityFeatureData> features, new ObservableCollection<EntityFeatureData>());
        info.TryGetValue("RolePlayInfo", out CharacterRolePlayInfo rolePlayInfo, new CharacterRolePlayInfo());
        
        if (animationsMapping != null)
        {
            AnimationsMapping = animationsMapping;
        }
        
        Unique = unique;
        Name = name;
        PortraitPath = portraitPath;
        InitialLevel = initialLevel;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        ClassId = classId;
        Stats = stats ?? new HashSet<Ulid>();
        _features = features;
        RolePlayInfo = rolePlayInfo;
        RefreshStats();
    }
    
    #endregion

}
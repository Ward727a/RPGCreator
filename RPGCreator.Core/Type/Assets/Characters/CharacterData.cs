using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Assets.Characters;

#region Should be moved to a more appropriate location

/// <summary>
/// Basic character stats structure. <br/>
/// This structure is just a simple container for now, but it will be changed and extended in the future.
/// </summary>
public class CharacterStats(IStatDef def) : ISerializable, IDeserializable
{
    public Ulid Unique => StatDef.Unique;
    public IStatDef StatDef { get; private set; } = def;
    public float CurrentValue { get; set; } = def.DefaultValue;
    public float MaxValue { get; set; } = def.StatCapValue;
    public float MinValue { get; set; } = def.StatMinValue;

    public IStatDef GenerateDefinition()
    {
        if(Math.Abs(CurrentValue - def.DefaultValue) < 0.001 && Math.Abs(MaxValue - def.StatCapValue) < 0.001 && Math.Abs(MinValue - def.StatMinValue) < 0.001)
        {
            return StatDef;
        }
        
        var statDef = new StatDefinition
        {
            Name = StatDef.Name,
            Description = StatDef.Description,
            DefaultValue = CurrentValue,
            StatTypeKind = StatDef.StatTypeKind,
            StatMinValue = MinValue,
            StatCapType = EStatTypeCap.ByValue,
            StatCapValue = MaxValue,
            IsVisible = StatDef.IsVisible,
            StatCapStatUnique = StatDef.StatCapStatUnique,
            PackId = StatDef.PackId,
            StatCompiledFormula = StatDef.StatCompiledFormula,
            StatNonCompiledFormula = StatDef.StatNonCompiledFormula,
            SavePath = StatDef.SavePath
        };

        foreach (var graphDocumentCompiled in StatDef.GetAllEvents())
        {
            statDef.AddEvent(graphDocumentCompiled.Key, graphDocumentCompiled.Value);
        }
        
        return statDef;
    }

    public bool IsStatDefDifferent(IStatDef otherDef)
    {
        if(otherDef.Unique != StatDef.Unique) return false;
        if(Math.Abs(otherDef.DefaultValue - StatDef.DefaultValue) > 0.001) return true;
        if(Math.Abs(otherDef.StatCapValue - StatDef.StatCapValue) > 0.001) return true;
        if(Math.Abs(otherDef.StatMinValue - StatDef.StatMinValue) > 0.001) return true;
        if(otherDef.IsVisible != StatDef.IsVisible) return true;
        if(otherDef.PackId != StatDef.PackId) return true;
        if(otherDef.Name != StatDef.Name) return true;
        if(otherDef.Description != StatDef.Description) return true;
        if(otherDef.StatTypeKind != StatDef.StatTypeKind) return true;
        if(otherDef.StatCapType != StatDef.StatCapType) return true;
        if(otherDef.StatCapStatUnique != StatDef.StatCapStatUnique) return true;
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
        
        if(Math.Abs(MaxValue - oldDef.StatCapValue) < 0.001)
        {
            MaxValue = newDef.StatCapValue;
        }

        if (Math.Abs(MinValue - oldDef.StatMinValue) < 0.001)
        {
            MinValue = newDef.StatMinValue;
        }
        if (Math.Abs(CurrentValue - oldDef.DefaultValue) < 0.001)
        {
            CurrentValue = newDef.DefaultValue;
        }
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
        throw new NotImplementedException();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// This structure contains the basic features of a character. <br/>
/// It is used to define the character's capabilities and behaviors in the game. <br/>
/// Each feature can be enabled or disabled, allowing for flexible character design. <br/>
/// For now, this structure is just a simple container for the features, but it will be changed and extended in the future.
/// </summary>
public struct CharacterFeatures() : ISerializable, IDeserializable
{
    public bool CanFight { get; set; } = true;
    public bool CanBeRecruited { get; set; } = true;
    public bool CanMove { get; set; } = true;
    public bool CanDie { get; set; } = true;
    public bool CanBeTalkedTo { get; set; } = true;
    public bool IsUnique { get; set; } = false;
    public bool IsPlayable { get; set; } = false;
    public bool IsBoss { get; set; } = false;
    public bool CanTrade { get; set; } = false;
    public bool CanTriggerEvents { get; set; } = true;
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterFeatures))
            .AddValue("CanFight", CanFight)
            .AddValue("CanBeRecruited", CanBeRecruited)
            .AddValue("CanMove", CanMove)
            .AddValue("CanDie", CanDie)
            .AddValue("CanBeTalkedTo", CanBeTalkedTo)
            .AddValue("IsUnique", IsUnique)
            .AddValue("IsPlayable", IsPlayable)
            .AddValue("IsBoss", IsBoss)
            .AddValue("CanTrade", CanTrade)
            .AddValue("CanTriggerEvents", CanTriggerEvents);
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("CanFight", out bool canFight, true, "CanFight not found or invalid (Set to true by default).");
        info.TryGetValue("CanBeRecruited", out bool canBeRecruited, true, "CanBeRecruited not found or invalid (Set to true by default).");
        info.TryGetValue("CanMove", out bool canMove, true, "CanMove not found or invalid (Set to true by default).");
        info.TryGetValue("CanDie", out bool canDie, true, "CanDie not found or invalid (Set to true by default).");
        info.TryGetValue("CanBeTalkedTo", out bool canBeTalkedTo, true, "CanBeTalkedTo not found or invalid (Set to true by default).");
        info.TryGetValue("IsUnique", out bool isUnique, false, "IsUnique not found or invalid (Set to false by default).");
        info.TryGetValue("IsPlayable", out bool isPlayable, false, "IsPlayable not found or invalid (Set to false by default).");
        info.TryGetValue("IsBoss", out bool isBoss, false, "IsBoss not found or invalid (Set to false by default).");
        info.TryGetValue("CanTrade", out bool canTrade, false, "CanTrade not found or invalid (Set to false by default).");
        info.TryGetValue("CanTriggerEvents", out bool canTriggerEvents, true, "CanTriggerEvents not found or invalid (Set to true by default).");

        CanFight = canFight;
        CanBeRecruited = canBeRecruited;
        CanMove = canMove;
        CanDie = canDie;
        CanBeTalkedTo = canBeTalkedTo;
        IsUnique = isUnique;
        IsPlayable = isPlayable;
        IsBoss = isBoss;
        CanTrade = canTrade;
        CanTriggerEvents = canTriggerEvents;
    }
}

#endregion

/// <summary>
/// This structure contains the role-play information of a character. <br/>
/// It is used to define the character's background, personality, and other role-play related information.<br/>
/// This doesn't affect the gameplay directly, but it is used to enhance the role-play experience.<br/>
/// This structure is just a simple container for the role-play information, but it will be changed and extended in the future.
/// </summary>
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

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Description", out string description, string.Empty, "Description not found or invalid (Set to empty by default).");
        info.TryGetValue("Backstory", out string backstory, string.Empty, "Backstory not found or invalid (Set to empty by default).");
        info.TryGetValue("PersonalityTraits", out string personalityTraits, string.Empty, "PersonalityTraits not found or invalid (Set to empty by default).");
        info.TryGetValue("FirstName", out string firstName, string.Empty, "FirstName not found or invalid (Set to empty by default).");
        info.TryGetValue("LastName", out string lastName, string.Empty, "LastName not found or invalid (Set to empty by default).");
        info.TryGetValue("Nickname", out string nickname, string.Empty, "Nickname not found or invalid (Set to empty by default).");
        info.TryGetValue("Age", out int age, 18, "Age not found or invalid (Set to 18 by default).");
        info.TryGetValue("Breed", out string breed, string.Empty, "Breed not found or invalid (Set to empty by default).");
        info.TryGetValue("Faction", out string faction, string.Empty, "Faction not found or invalid (Set to empty by default).");
        info.TryGetValue("Alignment", out string alignment, string.Empty, "Alignment not found or invalid (Set to empty by default).");

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

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("SlotName", out string slotName, string.Empty, "SlotName not found or invalid (Set to empty by default).");
        info.TryGetValue("SlotIndex", out int slotIndex, 0, "SlotIndex not found or invalid (Set to 0 by default).");
        info.TryGetValue("ItemId", out string itemId, string.Empty, "ItemId not found or invalid (Set to empty by default).");
        info.TryGetValue("ItemType", out string itemType, string.Empty, "ItemType not found or invalid (Set to empty by default).");

        SlotName = slotName;
        SlotIndex = slotIndex;
        ItemId = itemId;
        ItemType = itemType;
    }
}

/// <summary>
/// This class represents a character in the game.
/// </summary>
public class CharacterData : BaseAsset, ICharacter, ISerializable, IDeserializable
{
    #region Events

    public event EventHandler<string>? PortraitChanged;
    public event EventHandler<string>? SpriteChanged;
    public event EventHandler<int>? LevelChanged;
    public event EventHandler<int>? MaxLevelChanged;
    public event EventHandler<int>? InitialLevelChanged;
    public event EventHandler<Ulid>? ClassChanged;
    
    #endregion
    
    #region Properties

    public URN Urn { get; }
    
    private string _portraitPath = string.Empty;
    private string _spritePath = string.Empty;
    
    private int _initialLevel = 1;
    private int _currentLevel = 1;
    private int _maxLevel = 99;
    
    private Ulid _classId = Ulid.Empty;

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

    [NotNull]
    public string? SpritePath 
    {
        get => _spritePath;
        set
        {
            if (value == null) return;
            if (_spritePath == value) return; // Avoid unnecessary updates
            _spritePath = value;
            SpriteChanged?.Invoke(this, value);
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

    public Dictionary<Ulid, CharacterStats> Stats { get; private set; } = new();
    
    public Dictionary<Ulid, CharacterSkill> Skills { get; private set; } = new();
    
    public CharacterFeatures Features { get; private set; } = new CharacterFeatures();
    
    public CharacterRolePlayInfo RolePlayInfo { get; private set; } = new CharacterRolePlayInfo();
    
    #endregion

    #region Constructors
    
    // Needed for serialization
    public CharacterData()
    {
        Type = TYPE.CHARACTER_DATA;
    }
    
    public CharacterData(string name) : this()
    {
        Name = name;
        RefreshStats();
        Urn = new URN("character", $"{name}@{Unique}");
    }
    
    #endregion
    
    #region Methods

    public void RefreshStats()
    {
        var stats = EngineCore.Instance.Managers.Assets.StatsRegistry.All();

        foreach (var statDef in stats)
        {
            if (!Stats.ContainsKey(statDef.Unique))
            {
                Stats[statDef.Unique] = new CharacterStats(statDef);
                continue;
            }
            
            var charStat = Stats[statDef.Unique];
            if (charStat.IsStatDefDifferent(statDef))
            {
                Stats[statDef.Unique].SetDef(statDef);
            }
        }
    }
    
    #endregion

    #region Serialization
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterData))
            .AddValue("Unique", Unique)
            .AddValue("Name", Name)
            .AddValue("PortraitPath", PortraitPath)
            .AddValue("SpritePath", SpritePath)
            .AddValue("InitialLevel", InitialLevel)
            .AddValue("CurrentLevel", CurrentLevel)
            .AddValue("MaxLevel", MaxLevel)
            .AddValue("ClassId", ClassId)
            .AddValue("Stats", Stats)
            .AddValue("Features", Features)
            .AddValue("RolePlayInfo", RolePlayInfo);
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Unique", out Ulid unique, Ulid.Empty, "Unique identifier not found or invalid.");
        info.TryGetValue("Name", out string name, "UNKNOWN", "Name not found or invalid (Set to 'UNKNOWN' by default).");
        info.TryGetValue("PortraitPath", out string portraitPath, string.Empty, "Portrait path not found or invalid (Set to empty by default).");
        info.TryGetValue("SpritePath", out string spritePath, string.Empty, "Sprite path not found or invalid (Set to empty by default).");
        info.TryGetValue("InitialLevel", out int initialLevel, 1, "Initial level not found or invalid (Set to 1 by default).");
        info.TryGetValue("CurrentLevel", out int currentLevel, 1, "Current level not found or invalid (Set to 1 by default).");
        info.TryGetValue("MaxLevel", out int maxLevel, 99, "Max level not found or invalid (Set to 99 by default).");
        info.TryGetValue("ClassId", out Ulid classId, Ulid.Empty, "Class ID not found or invalid (Set to empty by default).");
        info.TryGetDictionary("Stats", out Dictionary<Ulid, CharacterStats>? stats);
        info.TryGetValue("Features", out CharacterFeatures features, new CharacterFeatures(), "Character features not found or invalid (Set to default features).");
        info.TryGetValue("RolePlayInfo", out CharacterRolePlayInfo rolePlayInfo, new CharacterRolePlayInfo(), "Character role-play info not found or invalid (Set to default role-play info).");
        
        Unique = unique;
        Name = name;
        PortraitPath = portraitPath;
        SpritePath = spritePath;
        InitialLevel = initialLevel;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        ClassId = classId;
        Stats = stats ?? new Dictionary<Ulid, CharacterStats>();
        Features = features;
        RolePlayInfo = rolePlayInfo;
        
        RefreshStats();
    }
    
    #endregion
}
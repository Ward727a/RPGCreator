using System.Diagnostics.CodeAnalysis;

namespace RPGCreator.Core.Type.Assets.Characters;

#region Should be moved to a more appropriate location

/// <summary>
/// Basic character stats structure. <br/>
/// This structure is just a simple container for now, but it will be changed and extended in the future.
/// </summary>
public struct CharacterStats() : ISerializable, IDeserializable
{
    public struct CharacterStatsChangedEventArgs(string statName, int oldValue, int newValue)
    {
        public string StatName { get; } = statName;
        public int OldValue { get; } = oldValue;
        public int NewValue { get; } = newValue;
    }
    public event EventHandler<CharacterStatsChangedEventArgs>? StatsChanged;
    
    private int _health = 100;
    public int Health 
    {
        get => _health;
        set
        {
            if (_health == value) return; // Avoid unnecessary updates
            int oldValue = _health;
            _health = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(Health), oldValue, value));
        }
    }
    
    private int _maxHealth = 100;
    public int MaxHealth 
    {
        get => _maxHealth;
        set
        {
            if (_maxHealth == value) return; // Avoid unnecessary updates
            int oldValue = _maxHealth;
            _maxHealth = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(MaxHealth), oldValue, value));
        }
    }
    
    private int _mana = 50;
    public int Mana 
    {
        get => _mana;
        set
        {
            if (_mana == value) return; // Avoid unnecessary updates
            int oldValue = _mana;
            _mana = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(Mana), oldValue, value));
        }
    }
    private int _maxMana = 50;
    public int MaxMana 
    {
        get => _maxMana;
        set
        {
            if (_maxMana == value) return; // Avoid unnecessary updates
            int oldValue = _maxMana;
            _maxMana = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(MaxMana), oldValue, value));
        }
    }
    
    private int _strength = 10;
    public int Strength 
    {
        get => _strength;
        set
        {
            if (_strength == value) return; // Avoid unnecessary updates
            int oldValue = _strength;
            _strength = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(Strength), oldValue, value));
        }
    }
    private int _maxStrength = 100;
    public int MaxStrength 
    {
        get => _maxStrength;
        set
        {
            if (_maxStrength == value) return; // Avoid unnecessary updates
            int oldValue = _maxStrength;
            _maxStrength = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(MaxStrength), oldValue, value));
        }
    }
    
    private int _agility = 10;
    public int Agility 
    {
        get => _agility;
        set
        {
            if (_agility == value) return; // Avoid unnecessary updates
            int oldValue = _agility;
            _agility = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(Agility), oldValue, value));
        }
    }
    private int _maxAgility = 100;
    public int MaxAgility 
    {
        get => _maxAgility;
        set
        {
            if (_maxAgility == value) return; // Avoid unnecessary updates
            int oldValue = _maxAgility;
            _maxAgility = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(MaxAgility), oldValue, value));
        }
    }
    
    private int _intelligence = 10;
    public int Intelligence 
    {
        get => _intelligence;
        set
        {
            if (_intelligence == value) return; // Avoid unnecessary updates
            int oldValue = _intelligence;
            _intelligence = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(Intelligence), oldValue, value));
        }
    }
    private int _maxIntelligence = 100;
    public int MaxIntelligence 
    {
        get => _maxIntelligence;
        set
        {
            if (_maxIntelligence == value) return; // Avoid unnecessary updates
            int oldValue = _maxIntelligence;
            _maxIntelligence = value;
            StatsChanged?.Invoke(this, new CharacterStatsChangedEventArgs(nameof(MaxIntelligence), oldValue, value));
        }
    }

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CharacterStats))
            .AddValue("Health", Health)
            .AddValue("MaxHealth", MaxHealth)
            .AddValue("Mana", Mana)
            .AddValue("MaxMana", MaxMana)
            .AddValue("Strength", Strength)
            .AddValue("MaxStrength", MaxStrength)
            .AddValue("Agility", Agility)
            .AddValue("MaxAgility", MaxAgility)
            .AddValue("Intelligence", Intelligence)
            .AddValue("MaxIntelligence", MaxIntelligence);
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Health", out int health, 100, "Health not found or invalid (Set to 100 by default).");
        info.TryGetValue("MaxHealth", out int maxHealth, 100, "MaxHealth not found or invalid (Set to 100 by default).");
        info.TryGetValue("Mana", out int mana, 50, "Mana not found or invalid (Set to 50 by default).");
        info.TryGetValue("MaxMana", out int maxMana, 50, "MaxMana not found or invalid (Set to 50 by default).");
        info.TryGetValue("Strength", out int strength, 10, "Strength not found or invalid (Set to 10 by default).");
        info.TryGetValue("MaxStrength", out int maxStrength, 100, "MaxStrength not found or invalid (Set to 100 by default).");
        info.TryGetValue("Agility", out int agility, 10, "Agility not found or invalid (Set to 10 by default).");
        info.TryGetValue("MaxAgility", out int maxAgility, 100, "MaxAgility not found or invalid (Set to 100 by default).");
        info.TryGetValue("Intelligence", out int intelligence, 10, "Intelligence not found or invalid (Set to 10 by default).");
        info.TryGetValue("MaxIntelligence", out int maxIntelligence, 100, "MaxIntelligence not found or invalid (Set to 100 by default).");

        Health = health;
        MaxHealth = maxHealth;
        Mana = mana;
        MaxMana = maxMana;
        Strength = strength;
        MaxStrength = maxStrength;
        Agility = agility;
        MaxAgility = maxAgility;
        Intelligence = intelligence;
        MaxIntelligence = maxIntelligence;
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

    public CharacterStats Stats { get; private set; } = new CharacterStats();

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
        info.TryGetValue("Stats", out CharacterStats stats, new CharacterStats(), "Character stats not found or invalid (Set to default stats).");
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
        Stats = stats;
        Features = features;
        RolePlayInfo = rolePlayInfo;
    }
    
    #endregion
}
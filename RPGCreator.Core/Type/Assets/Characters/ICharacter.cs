namespace RPGCreator.Core.Type.Assets.Characters;

public interface ICharacter
{
    public event EventHandler<string>? PortraitChanged;
    public event EventHandler<string>? SpriteChanged;
    public event EventHandler<int>? LevelChanged;
    public event EventHandler<int>? MaxLevelChanged;
    public event EventHandler<int>? InitialLevelChanged;
    public event EventHandler<Ulid>? ClassChanged;
    
    public Ulid Unique { get; }
    public string Name { get; }
    public string PortraitPath { get; set; }
    public string SpritePath { get; set; }
    public int InitialLevel { get; set; }
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; }
    public Ulid ClassId { get; set; }
    public CharacterStats Stats { get; }
    public CharacterFeatures Features { get; }
    public CharacterRolePlayInfo RolePlayInfo { get; }
}
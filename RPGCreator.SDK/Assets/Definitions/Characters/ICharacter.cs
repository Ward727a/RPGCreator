using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Characters;

public interface ICharacter : IHasSavePath, IHasUniqueId
{
    public event EventHandler<string>? PortraitChanged;
    public event EventHandler<int>? LevelChanged;
    public event EventHandler<int>? MaxLevelChanged;
    public event EventHandler<int>? InitialLevelChanged;
    public event EventHandler<Ulid>? ClassChanged;
    
    public string Name { get; }
    public string PortraitPath { get; set; }
    public int InitialLevel { get; set; }
    public int CurrentLevel { get; set; }
    public int MaxLevel { get; set; }
    public Ulid ClassId { get; set; }
    public HashSet<Ulid> Stats { get; }
    public CharacterRolePlayInfo RolePlayInfo { get; }
}
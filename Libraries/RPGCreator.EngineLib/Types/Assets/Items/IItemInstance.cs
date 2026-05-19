namespace RPGCreator.EngineLib.Types.Assets.Items;

public interface IItemInstance
{
    public Ulid Unique { get; }
    public Ulid ItemDataId { get; set; }
    public IItemData ItemData { get; }
}
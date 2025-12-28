using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Types.Assets.Items;


public enum ItemRarity
{
    Junk,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic,
}

public interface IItemData : ISerializable, IDeserializable
{
    public Ulid Unique { get; }
    public string ItemName { get; }
    public string ItemDescription { get; set; }
    public string ItemIcon { get; set; }
    
    /// <summary>
    /// Describes the rarity of the item.
    /// </summary>
    public ItemRarity Rarity { get; set; }
    
    /// <summary>
    /// Can be stacked in the inventory.
    /// </summary>
    public bool IsStackable { get; set; }
    /// <summary>
    /// Maximum stack size of the item in the inventory.
    /// </summary>
    public int MaxStackSize { get; set; }
    
    /// <summary>
    /// If used, the item will be consumed and removed from the inventory.<br/>
    /// </summary>
    public bool IsConsumable { get; set; }
    /// <summary>
    /// Can be equipped in the inventory.
    /// </summary>
    public bool IsEquippable { get; set; }
    /// <summary>
    /// Can be used in the inventory.<br/>
    /// </summary>
    public bool IsUsable { get; set; }
    /// <summary>
    /// This item is unique and can only exist once in the inventory.
    /// </summary>
    public bool IsUnique { get; set; }
    /// <summary>
    /// This item is a quest item.<br/>
    /// </summary>
    public bool IsQuestItem { get; set; }
    /// <summary>
    /// This item can be sold in the shop.
    /// </summary>
    public bool IsSellable { get; set; }
    /// <summary>
    /// This item can be removed from the inventory.
    /// </summary>
    public bool IsDroppable { get; set; }
}
using RPGCreator.Core.Types.Assets.Actors;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.Core.Types.Assets.Items;

public class ConsumableData : IItemData, IHasUniqueId
{

    public string Name;
    
    public SerializationInfo GetObjectData()
    {
        throw new NotImplementedException();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        throw new NotImplementedException();
    }

    public ConsumableData(string ItemName, string itemDescription = "", string itemIcon = "")
    {
        Name = ItemName;
        ItemDescription = itemDescription;
        ItemIcon = itemIcon;
        Rarity = ItemRarity.Common;
        IsStackable = true;
        MaxStackSize = 99; // Default max stack size for consumables
        IsConsumable = true;
        IsEquippable = false;
        IsUsable = true;
        IsUnique = false;
        IsQuestItem = false;
        IsSellable = true;
        IsDroppable = true;
    }

    public void Use(IActor actor)
    {
        
    }

    public Ulid Unique { get; }
    public URN Urn { get; }
    public string ItemName => Name;
    public string ItemDescription { get; set; }
    public string ItemIcon { get; set; }
    public ItemRarity Rarity { get; set; }
    public bool IsStackable { get; set; }
    public int MaxStackSize { get; set; }
    public bool IsConsumable { get; set; }
    public bool IsEquippable { get; set; }
    public bool IsUsable { get; set; }
    public bool IsUnique { get; set; }
    public bool IsQuestItem { get; set; }
    public bool IsSellable { get; set; }
    public bool IsDroppable { get; set; }
}
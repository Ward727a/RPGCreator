extends _Object
class_name item
# This is the base class for all items in the game.

# Import the necessary enums
const ItemObtainability = EnumRegister.ItemObtainability
const ItemRarity = EnumRegister.ItemRarity
const ItemUsability = EnumRegister.ItemUsability
const ItemDurabilityType = EnumRegister.ItemDurabilityType
const ItemRepairability = EnumRegister.ItemRepairability
const ItemDropability = EnumRegister.ItemDropability
const ItemType = EnumRegister.ItemType

var price: int = 0

var effects: Array = []

var rarity: ItemRarity = ItemRarity.TRASH # Object's rarity
var obtainability: ItemObtainability = ItemObtainability.UNOBTAINABLE # If the item can be obtained by the player
var usability: ItemUsability = ItemUsability.UNCONSUMABLE # If the item can be consumed
var durability_type: ItemDurabilityType = ItemDurabilityType.NONE # If the item has durability
var repairability: ItemRepairability = ItemRepairability.UNREPAIRABLE # If the item can be repaired by the player
var dropability: ItemDropability = ItemDropability.NOT_DROPABLE # If the item can be dropped by the player

var can_be_stored: bool = false # If the item can be stored (e.g. in a chest)
var can_be_sold: bool = false # If the item can be sold to a vendor

var durability: int = 0 # Object's durability if applicable
var max_durability: int = 0 # Object's maximum durability if applicable

# Override the drop function
# This function is called when the object is dropped by the player
func drop():
    write("'drop' function not implemented")
    pass

# Override the repair function
# This function is called when the object is repaired by the player
func repair():
    write("'repair' function not implemented")
    pass

# Override the destroy function
# This function is called when the object is destroyed (e.g. when the durability reaches 0)
func destroy():
    write("'destroy' function not implemented")
    pass

# Override the damage function (e.g. when the object is hit by an attack)
# This function is called when the object is damaged
func damage():
    write("'damage' function not implemented")
    pass
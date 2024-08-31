extends Base
class_name Loottables
# This is the base class for all loot tables in the game

var id: int = 0 # Loot table's ID
var name: String = "" # Loot table's name
var description: String = "" # Loot table's description

var loot: Array = [] # Loot table's loot

func _init(_id: int, _name: String, _description: String, _loot: Array):
    self.id = _id
    self.name = _name
    self.description = _description
    self.loot = _loot

    allowed_events = ["pre_addloot", "post_addloot", "pre_removeloot", "post_removeloot", "getloottable", "pre_getitems", "post_getitems"]

    set_custom_name("loottables")

    super()

# Add an item to the loot table
func add_loot(_item: _Object, _min_amount: int, _max_amount: int, _chance: int):

    # Emit the pre_addloot event
    call_event("pre_addloot", [_item, _min_amount, _max_amount, _chance])

    # Clamp the _chance value between 0 and 100
    _chance = clampi(_chance, 0, 100)

    var _loot = {
        "item": _item,
        "min_amount": _min_amount,
        "max_amount": _max_amount,
        "chance": _chance # Chance in percentage (0-100)
    }
    loot.append(_loot)

    # Emit the post_addloot event
    call_event("post_addloot", [_item, _min_amount, _max_amount, _chance])

# Remove an item from the loot table
func remove_loot(_item: item):

    # Emit the pre_removeloot event
    call_event("pre_removeloot", [_item])

    var success: bool = false

    for i in range(loot.size()):
        if loot[i]["item"] == _item:
            loot.erase(i)
            success = true
            break
    
    # Emit the post_removeloot event
    call_event("post_removeloot", [_item, success])

# Get the loot table
func get_loot_table():

    # Emit the pre_getloottable event
    call_event("getloottable", [loot])

    return loot

# Create an array of items to be returned, based on the loot table, and the chance of each item, and the min/max amount of each item, it can return an empty array if no items are dropped
func get_items():

    # Emit the pre_getitems event
    call_event("pre_getitems", [])

    var _items = []
    for i in range(loot.size()):
        if randi() % 100 < loot[i]["chance"]:
            var _amount = randi() % (loot[i]["max_amount"] - loot[i]["min_amount"]) + loot[i]["min_amount"]
            for j in range(_amount):
                _items.append(loot[i]["item"])
    
    # Emit the post_getitems event
    call_event("post_getitems", [_items])

    return _items
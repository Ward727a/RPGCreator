extends _Object
class_name Item
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

var parameters: Dictionary
var param_by_id: Dictionary

signal EVENTS_get_parameter(param_name: String, param_category: String, param_object: LUADynamicParameter)
signal EVENTS_set_parameter(param_name: String, param_category: String, param_object: LUADynamicParameter)
signal EVENTS_drop
signal EVENTS_repair
signal EVENTS_destroy
signal EVENTS_damage

func _init():
	
	id = IdGenerator.new().generate_id()
	name = str('Item-', IdGenerator.new().generate_small_id())
	
	var categories: Array = ItemParametersRegister.get_categories()
	
	for category: String in categories:
		
		var parameters = ItemParametersRegister.get_parameters(category)
		
		for parameter_key: String in parameters:
			
			set_parameter(parameter_key, category)
	
	EventsBus.EVENTS_Item.new_item.emit(self)

# Override the drop function
# This function is called when the object is dropped by the player
func drop():
	write("'drop' function not implemented")
	EVENTS_drop.emit()
	pass

# Override the repair function
# This function is called when the object is repaired by the player
func repair():
	write("'repair' function not implemented")
	EVENTS_repair.emit()
	pass

# Override the destroy function
# This function is called when the object is destroyed (e.g. when the durability reaches 0)
func destroy():
	write("'destroy' function not implemented")
	EVENTS_destroy.emit()
	pass

# Override the damage function (e.g. when the object is hit by an attack)
# This function is called when the object is damaged
func damage():
	write("'damage' function not implemented")
	EVENTS_damage.emit()
	pass

# Set a parameter and return the LUADynamicParameter
func set_parameter(param_name: String, param_category: String) -> LUADynamicParameter:
	
	var param_data: Dictionary = ItemParametersRegister.get_parameter(param_name, param_category)
	
	var parameter: LUADynamicParameter = LUADynamicParameter.new(param_data)
	
	if !parameters.has(param_category):
		parameters[param_category] = {}
	
	parameters[param_category][param_name] = null
	
	parameters[param_category][param_name] = parameter
	param_by_id[parameter.get_id()] = {
		"name": param_name,
		"category": param_category
	}
	return null
	
	#
	#EVENTS_set_parameter.emit(param_name, param_category, parameter)
	#
	#return parameter

# Return the LUADynamicParameter object given the LUADynamicParameter ID
func get_parameter_by_id(param_id) -> LUADynamicParameter:
	
	if !param_by_id.has(param_id):
		return null
	
	var param_name: String = param_by_id[param_id]['name']
	var param_category: String = param_by_id[param_id]['category']
	
	return get_parameter(param_name, param_category)

# Return the LUADynamicParameter object given the parameter data
func get_parameter(param_name: String, param_category: String) -> LUADynamicParameter:
	
	if !has_parameter(param_name, param_category):
		return null
	
	EVENTS_get_parameter.emit(param_name, param_category, parameters[param_category][param_name])
	
	return parameters[param_category][param_name]

# Check if item has given parameter data
func has_parameter(param_name: String, param_category: String) -> bool:
	
	if !parameters.has(param_category):
		return false
	
	if !parameters.has(param_name):
		return false
	
	return true

# return the list of parameters by id of LUADynamicParameter created
# Structure - Dictionary:
# "id": { "name", "category"}
func get_params_list() -> Dictionary:
	return param_by_id

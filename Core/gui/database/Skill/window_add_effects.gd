extends ConfirmationDialog
# This window is used to add effects to a skill

@onready var list: CustomListV2Base = get_node("add_effects_list")

var effects: Dictionary = {}
var effects_hash: int = 0
var effects_checked: Dictionary = {}

var nodes: Dictionary = {}

# Called when the node enters the scene tree for the first time.
func _ready():

	about_to_popup.connect(_on_about_to_popup)

## We will initialize the effects here (Add the effects to the list, etc)
func _on_about_to_popup():
	
	# Get all the effects
	var effects_data: Dictionary = EffectRegister.get_all_effects()
	
	print(var_to_str(effects_data))
	
	var effects_data_hash: int = effects_data.hash() # Get the hash of the effects data

	# Check if the hash are the same - So we don't have to update the list every time
	if effects_data_hash == effects_hash:
		
		print("Effects: Same hash")
		
		return
	
	print("Effects: Different hash")

	# Set the new hash
	effects_hash = effects_data_hash

	# Set the new list of effects
	effects = effects_data

	# Test add item to list
	# var idx = list.add_item({"val": "Test", "idx": "test", "hint": "Test hint"}) # Uncomment this line to test the list

	# print("IDX test_item: ", idx) # Uncomment this line to test the list
	
	reload_content()

func set_checked(checked_effects: Array[BaseEffect]):
	
	effects_checked.clear()

	for i in checked_effects:
		effects_checked[i.id] = i
	
	for i in nodes.keys():
		var node_item: Node = nodes[i]

		if effects_checked.has(i):
			node_item.set_checked(true)
		else:
			node_item.set_checked(false)

## Used to reload the [b]VISUAL[/b] content (Not the data)
func reload_content():

	list.clear()

	for i in effects.keys():
		var idx_item: String = list.add_item({"val": effects[i].name, "idx": i, "hint": effects[i].description})

		var _item: RefCounted = list.get_item(idx_item)

		var node_item: Node = _item.template_node

		print("effects_checked: ", effects_checked)
		print("condition_i: ", i)

		nodes[i] = node_item

		node_item.toggled.connect(_on_effect_toggled.bind(_item.data))

	print("Added: ", effects.keys().size(), " effects")

func _on_effect_toggled(toggle: bool, _idx: String, data: Dictionary):
	
	var effect_idx: String = data["idx"]

	# Check if the effect exists in the register
	if !EffectRegister.has_effect(effect_idx):
		print("Effect not found in the register")
		return

	var effect = EffectRegister.get_pure_effect(effect_idx)

	if toggle:
		effects_checked[effect_idx] = effect
	else:
		effects_checked.erase(effect_idx)
	
	print("Checked: ", effects_checked.size())

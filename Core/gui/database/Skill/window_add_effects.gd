extends ConfirmationDialog
# This window is used to add effects to a skill

@onready var list: CustomListV2Base = get_node("add_effects_list")

var effects: Dictionary = {}
var effects_hash: int = 0

# Called when the node enters the scene tree for the first time.
func _ready():

	about_to_popup.connect(_on_about_to_popup)

## We will initialize the effects here (Add the effects to the list, etc)
func _on_about_to_popup():
	
	# Get all the effects
	var effects_data: Dictionary = EffectRegister.get_all_effects()
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


## Used to reload the [b]VISUAL[/b] content (Not the data)
func reload_content():

	pass
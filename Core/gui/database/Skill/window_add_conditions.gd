extends ConfirmationDialog
# This window is used to add conditions to a skill

@onready var list: CustomListV2Base = get_node("add_conditions_list")

var conditions: Dictionary = {}
var conditions_hash: int = 0
var conditions_checked: Dictionary = {}

var nodes: Dictionary = {}

# Called when the node enters the scene tree for the first time.
func _ready():

	about_to_popup.connect(_on_about_to_popup)

## We will initialize the conditions here (Add the conditions to the list, etc)
func _on_about_to_popup():
	
	# Get all the conditions
	var conditions_data: Dictionary = SkillConditionsRegister.get_all_conditions()
	var conditions_data_hash: int = conditions_data.hash() # Get the hash of the conditions data

	# Check if the hash are the same - So we don't have to update the list every time we open the window
	if conditions_data_hash == conditions_hash:
		
		print("Conditions: Same hash")
		
		return
	
	print("Conditions: Different hash")

	# Set the new hash
	conditions_hash = conditions_data_hash

	# Set the new list of conditions
	conditions = conditions_data

	# Test add item to list
	# var idx = list.add_item({"val": "Test", "idx": "test"}) # Uncomment this line to test the list

	# print("IDX test_item: ", idx) # Uncomment this line to test the list

	# Reload the content
	reload_content()

func set_checked(checked_conditions: Array[BaseSkillCondition]):
	
	conditions_checked.clear()

	for i in checked_conditions:
		conditions_checked[i.id] = i
	
	for i in nodes.keys():
		var node_item: Node = nodes[i]

		if conditions_checked.has(i):
			node_item.set_checked(true)
		else:
			node_item.set_checked(false)

## Used to reload the [b]VISUAL[/b] content (Not the data)
func reload_content():

	list.clear()

	for i in conditions.keys():
		var idx_item: String = list.add_item({"val": conditions[i].name, "idx": i, "hint": conditions[i].description})

		var _item: RefCounted = list.get_item(idx_item)

		var node_item: Node = _item.template_node

		print("conditions_checked: ", conditions_checked)
		print("condition_i: ", i)

		nodes[i] = node_item

		node_item.toggled.connect(_on_condition_toggled.bind(_item.data))

	print("Added: ", conditions.keys().size(), " conditions")

## Called when the user toggles a condition [br]
## toggle: The state of the condition [br]
## idx: The unique id of the list item[br]
func _on_condition_toggled(toggle: bool, _idx: String, data: Dictionary):

	var cond_idx: String = data["idx"]

	# Check if the condition exists in the register
	if !SkillConditionsRegister.has_condition(cond_idx):
		print("Condition not found in the register")
		return

	var condition = SkillConditionsRegister.get_pure_condition(cond_idx)

	if toggle:
		conditions_checked[cond_idx] = condition
	else:
		conditions_checked.erase(cond_idx)
	
	print("Checked: ", conditions_checked.size())

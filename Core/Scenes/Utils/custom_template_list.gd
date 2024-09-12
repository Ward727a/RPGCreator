extends VBoxContainer
## This class is used to create a list with a custom template provided by the user.[br]
## 
## [color=yellow]!!! Please read before doing anything !!![br]This class is very sensitive to errors, make sure to follow the instructions carefully. If you don't and catch an error, the list will simply be removed from the scene tree.[/color][br]
## [br]
## [b]How to use:[/b][br]
## - Create a new scene[br]
## - Add a new VBoxContainer node[br]
## - Add this script to the node[br]
## - Set the template to the template you want to use (the template needs to have a script that extends the custom_list_template_base.gd)[br]
## - Set the rows to the rows you want to use - You can let it empty if you don't want to use the row system[br]
## - Save the scene[br]
## - Add the scene to the scene tree[br]
## - Done![br]
## [br]
## [b]How to create a new template:[/b][br]
## - Create a new scene[br]
## - Add a new Control node[br]
## - Add a new script to the node[br]
## - In the script, extend the custom_list_template_base.gd[br]
## - Implement all the functions found in the custom_list_template_base.gd[br]
## - Save the scene[br]
## - Done![br]
## [br]
## [b]How to create a new row:[/b][br]
## - Add a new row to the rows array[br]
## - The row needs to have the following data:[br]
## .   - name: The name of the row - [String][br]
## .   - metadata: The metadata of the row - [Dictionary][br]
## .   - type: The type of the row - [enum List.ItemType][br]
## .   - (optional) centered: If the row is centered - [bool][br]
## .   - (optional) percentage_size: The percentage size of the row (from 0 to 1) - [float][br]
## - If you don't want to use the row system, you can simply let this empty.[br]
## - Done![br]
## [br]


## This template node.[br]
## The template need to have a script that extends the custom_list_template_base.gd[br]
## The script need to have ALL the functions found in the custom_list_template_base.gd implemented!
@export var template: Node = null

## The list rows [br]
## Each row needs the following data:[br]
## - name: The name of the row - [String][br]
## - metadata: The metadata of the row - [Dictionary][br]
## - type: The type of the row - [enum List.ItemType][br]
## - (optional) centered: If the row is centered - [bool][br]
## - (optional) percentage_size: The percentage size of the row (from 0 to 1) - [float][br]
## [br]
## If you don't want to use the row system, you can simply let this empty.
@export var rows: Array[Dictionary] = [{}]	

## [b][color=red]Internal variable, do not touch.[/color][/b][br][br]
## The list node.[br]
## This is the node that will contain all the rows.
var list: VBoxContainer
## [b][color=red]Internal variable, do not touch.[/color][/b][br][br]
## The scroll node.[br]
## This is the node that will contain the list.
var scroll: ScrollContainer
## [b][color=red]Internal variable, do not touch.[/color][/b][br][br]
## The data of the list.[br]
## This is the data that will be shown in the list.
var data: Dictionary = {}
## [b][color=red]Internal variable, do not touch.[/color][/b][br][br]
## This will determine if the list is working (all check are done and valid, if not, set to true).[br]
var not_working: bool = false

func reload():

	# Clear the content
	_clear_content()

	# Check if the template is valid.
	if !_check_template():
		push_error("The template is invalid.")
		not_working = true
	
	# Check if the rows are valid.
	if !_check_rows():
		push_error("The rows are invalid.")
		not_working = true
	
	# hide the template
	template.hide()

	# Create the list.
	_create_list()

	# Create the row. (If any)
	_create_row()

# Called when the node enters the scene tree for the first time.
func _ready():

	# Check if the template is valid.
	if !_check_template():
		push_error("The template is invalid.")
		not_working = true
	
	# Check if the rows are valid.
	if !_check_rows():
		push_error("The rows are invalid.")
		not_working = true
	
	# hide the template
	template.hide()

	# Create the list.
	_create_list()

	# Create the row. (If any)
	_create_row()

func _check_template():
	# Check if the template is a valid node.
	if template == null:
		push_error("The template is missing.")
		return false

	# Check if the template is a valid node.
	if !template.is_class("Control"):
		push_error("The template is not a valid Control.")
		return false
	
	if template.get_script() == null:
		push_error("The template doesn't have a script.")
		return false
	
	var script: Script = template.get_script()

	if script.get_base_script() == null:
		push_error("The template script doesn't have a base script.")
		return false
	
	var base_script: Script = script.get_base_script()

	if base_script.get_path() != "res://Core/gui/CustomListTemplate/custom_list_template_base.gd":
		push_error("The template script doesn't have the correct base script.")
		return false
	
	
	# Check if the template has a _init function without arguments. (If no _init function is found, we don't care)
	for i in script.get_method_list():
		if i.name != "_init": # We only want to check the _init function here.
			continue
		
		# We don't want any arguments in the _init function. This is not allowed due to the fact that we will duplicate the template.
		if i.args.size() > 0: 
			push_error("The template script has the _init function with arguments. This is not allowed.")
			return false

	return true

func _check_rows():
	for i in rows:
		if !i.has("name"):
			push_error("The row ",i," doesn't have a name.")
			return false
		
		# Check if the name is a string.
		if !typeof(i["name"]) == TYPE_STRING:
			push_error("The row ",i," name is not a string.")
			return false
		
		if !i.has("metadata"):
			push_error("The row ",i," doesn't have metadata.")
			return false
		
		# Check if the metadata is a dictionary.
		if !typeof(i["metadata"]) == TYPE_DICTIONARY:
			push_error("The row ",i," metadata is not a dictionary.")
			return false
		
		if !i.has("type"):
			push_error("The row ",i," doesn't have a type.")
			return false

		# Check if the type is an integer.
		if !typeof(i["type"]) == TYPE_INT:
			push_error("The row ",i," type is not an integer.")
			return false
		
		# Check if the type is valid.
		if i["type"] < 0 or i["type"] > List.ItemType.MAX:
			push_error("The row ",i," type is invalid.")
			return false
		
		if i.has('centered'):
			if !typeof(i["centered"]) == TYPE_BOOL:
				push_error("The row ",i," centered is not a boolean.")
				return false
		else:
			i["centered"] = false
		
		if i.has('percentage_size'):
			if !typeof(i["percentage_size"]) == TYPE_FLOAT:
				push_error("The row ",i," percentage_size is not a float.")
				return false
		else:
			i["percentage_size"] = 1.0

	return true
## This function is used to create the list.[br]
## This will create the list and scroll and add them to the node. [br]
## This will also set the anchors of the scroll. And set the size flags of the list.[br]
## [br]
## [b][color=red]Internal function, do not touch.[/color][/b]
func _create_list():

	if not_working:
		return

	# Create the list.
	list = VBoxContainer.new()
	scroll = ScrollContainer.new()

	# Add the list to the scroll.
	scroll.add_child(list)

	# Add the scroll to the node.
	add_child(scroll)

	# Set the anchors.
	scroll.set_anchors_preset(Control.PRESET_FULL_RECT)
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	scroll.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	list.size_flags_vertical = Control.SIZE_EXPAND_FILL
	list.size_flags_horizontal = Control.SIZE_EXPAND_FILL

func _create_row():

	if not_working:
		return


	# Check if their is any rows.
	if rows.size() == 0:
		# No rows found.
		# This means the user doesn't want to use the row system.
		# We will simply return.
		return

	# Create the row.
	var _row: HBoxContainer = HBoxContainer.new()

	# Add the row to the list.
	list.add_child(_row)

	# Set the anchors.
	_row.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	# Add labels to the row.
	for i in rows:
		var _label: Label = Label.new()
		_label.text = i["name"]
		if i["centered"]:
			_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		
		_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		_label.size_flags_stretch_ratio = i["percentage_size"]
		_row.add_child(_label)

func remove_item(idx: String):

	if not_working:
		return

	# Check if the item exists.
	if !data.has(idx):
		push_error("The item ",idx," doesn't exist.")
		return

	# Get the item.
	var _item: __TemplateItem = data[idx]

	# Get the index of the item.
	var id_list = _item.id_list

	# Remove the item from the list.
	list.remove_child(id_list)

	# Remove the item from the data.
	data.erase(idx)

	# Update the id_list of the items.
	for i in data:
		if data[i].id_list > id_list:
			data[i].id_list -= 1

## Add an item to the list.[br]
## You need to provide the data of the item you want to add.[br]
## The data need to be formated the same way the template is expecting it.[br]
## [br]
## For the base template (if you don't have a custom template), the data need to be a dictionary with the key "val".[br]
## This will return the id of the item. Or an empty string if the item couldn't be added.
func add_item(_data: Dictionary) -> String:

	if not_working:
		return ""

	
	var id = IdGenerator.new().generate_id()

	# Create the item.
	var _item: __TemplateItem = __TemplateItem.new()
	# Set the id of the item. This will not change.
	_item.id = id
	# Set the data of the item.
	_item.data = _data

	# Create a new row with the template
	var _template: Control = template.duplicate()

	# Load the template.
	_template.load()

	# Set the unique id of the item in the template.
	_template.set_idx(id)

	# Set the data of the item in the template.
	_template.set_data(_data)

	# Add the row to the list.
	list.add_child(_template)

	# Set the id of the list.
	_item.template_node = _template

	# Show the template
	_template.show()

	# Add the item to the list
	data[id] = _item

	# Return the id of the item.
	return id

func _get_item(idx: String) -> __TemplateItem:

	if not_working:
		return null

	# Check if the item exists.
	if !data.has(idx):
		push_error("The item ",idx," doesn't exist.")
		return null

	return data[idx]

func clear_items():

	if not_working:
		return

	# Clear the items.
	for i in data.keys():
		list.remove_child(data[i].template_node)
		print("Removing: ",i)
		data.erase(i)
	
	# Checking if the data is empty.
	if data.size() != 0:
		push_error("The data is not empty after clearing the items.")
		return


func _clear_content():

	if not_working:
		return

	# Clear the list.
	list.queue_free()
	scroll.queue_free()

# Class of the row
class Row:
	var name: String
	var metadata: Dictionary
	var type: List.ItemType

class __TemplateItem:
	var id: String ## The id of the item (This will not change once set)
	var template_node: Control ## The control of the list - This is used to identify where in the list the item is
	var data: Dictionary = {} ## The data of the item
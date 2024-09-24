extends VBoxContainer
class_name CustomListV2Base
## Please note that this class is very sensitive and you need to be careful when using it. [br]
## If you don't know what you are doing, please ask for help. [br]
## If you do something wrong, the list will just not appear as it will be cleaned up by the engine. [br]

@onready var create_button: Button = get_node("MarginContainer2/menu/create")
@onready var add_button: Button = get_node("MarginContainer2/menu/add")
@onready var title_label: Label = get_node("TitleContainer/Title")
@onready var title_container: HBoxContainer = get_node("TitleContainer")
@onready var list_container: VBoxContainer = get_node("MarginContainer/Panel/MarginContainer/ScrollContainer/VBoxContainer")

@export var show_create_button: bool = true
@export var show_add_button: bool = true
@export var show_title: bool = true

## This variable is used to store a custom item template.[br]
## If not set, the default template will be used.
@export var custom_item_template: Node = null

@export var title: String = "Title"

signal add_pressed()
signal create_pressed()

func _ready():

	# init the buttons
	create_button = get_node("MarginContainer2/menu/create")
	add_button = get_node("MarginContainer2/menu/add")
	title_label = get_node("TitleContainer/Title")
	title_container = get_node("TitleContainer")

	if !show_create_button:
		create_button.hide()
	
	if !show_add_button:
		add_button.hide()

	if !show_title:
		title_container.hide()
	
	title_label.text = tr(title)

	if custom_item_template != null:

		# print("Custom item template found") # Debug

		list_container.template = custom_item_template
		list_container.reload()

## Add an item to the list.[br]
## You need to provide the data of the item you want to add.[br]
## The data need to be formated the same way the template is expecting it.[br]
## [br]
## For the base template (if you don't have a custom template), the data need to be a dictionary with the key "val".[br]
## [br]
## This function will return the idx of the item added. Or an empty string if the item couldn't be added.
func add_item(_data: Dictionary) -> String:
	return list_container.add_item(_data)

## Remove an item from the list.[br]
## You need to provide the idx of the item you want to remove.[br]
## The idx is the unique id of the item.[br]
func remove_item(_idx: String):
	list_container.remove_item(_idx)

func get_item(_idx: String) -> RefCounted:
	return list_container._get_item(_idx)

## Clear the list of all the items
func clear():
	list_container.clear_items()

func _on_create_pressed():
	create_pressed.emit()

func _on_add_pressed():
	add_pressed.emit()

extends VBoxContainer
class_name CustomListV2Base

@onready var create_button: Button = get_node("MarginContainer2/menu/create")
@onready var add_button: Button = get_node("MarginContainer2/menu/add")
@onready var title_label: Label = get_node("TitleContainer/Title")
@onready var title_container: HBoxContainer = get_node("TitleContainer")


@export var show_create_button: bool = true
@export var show_add_button: bool = true
@export var show_title: bool = true

@export var title: String = "Title"

func _ready():

	if !show_create_button:
		create_button.hide()
	
	if !show_add_button:
		add_button.hide()

	if !show_title:
		title_container.hide()
	
	title_label.text = tr(title)
extends CustomListTemplateBase

var edit_button: Button

var idx: String # The unique id of the item.
var label: Label

var condition_idx: String # The index of the condition

signal edit(condition_idx: String)

func load(): # This function will be called first when the list is created.
	label = get_node("Panel/MarginContainer/HBoxContainer/Label")
	edit_button = get_node("edit_cond_button")

	edit_button.pressed.connect(_on_edit_button_pressed)

func set_data(_data: Dictionary): # This function will be called at third when the list is created.

	print(_data)

	label.text = tr(_data["val"])
	label.get_parent().set_tooltip_text(tr(_data["hint"]))
	condition_idx = _data["idx"]

func set_idx(_idx: String): # This function will be called at second when the list is created. The IDX is the unique id of the item.
	idx = _idx

func _on_edit_button_pressed():
	edit.emit(condition_idx)
extends CustomListTemplateBase

var idx: String
var label: Label

var condition_idx: String

signal toggled(toggled_on: bool, idx: String)

func load(): # This function will be called first when the list is created.
	label = get_node("Panel/MarginContainer/HBoxContainer/Label")

func set_data(_data: Dictionary): # This function will be called at third when the list is created.

	print(_data)

	label.text = tr(_data["val"])
	label.get_parent().set_tooltip_text(tr(_data["hint"]))
	condition_idx = _data["idx"]

func set_checked(_checked: bool):
	# print("Set checked: ", _checked)
	get_node("Panel/MarginContainer/HBoxContainer/CheckBox").set_pressed_no_signal(_checked)

func set_idx(_idx: String): # This function will be called at second when the list is created. The IDX is the unique id of the item.
	idx = _idx

func _on_check_box_toggled(toggled_on:bool):
	print("Checkbox toggled: ", toggled_on)
	toggled.emit(toggled_on, idx)

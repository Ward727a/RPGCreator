extends CustomListTemplateBase

var idx: String
var label: Label


func load(): # This function will be called first when the list is created.
	label = get_node("Panel/MarginContainer/HBoxContainer/Label")
	pass

func set_data(_data: Dictionary): # This function will be called at third when the list is created.

	print(_data)

	label.text = _data["val"]

func set_idx(_idx: String): # This function will be called at second when the list is created. The IDX is the unique id of the item.
	idx = _idx
extends CustomListTemplateBase

var idx: String

var item_id: String
var label: Label

signal item_clicked(item_id: String)
signal delete_item(item_id: String)

func load(): # This function will be called first when the list is created.
	label = get_node("Panel/MarginContainer/HBoxContainer/Label")
	pass

func set_data(_data: Dictionary): # This function will be called at third when the list is created.

	print(_data)

	label.text = tr(_data["item_name"])
	item_id = _data['item_id']

func set_idx(_idx: String): # This function will be called at second when the list is created. The IDX is the unique id of the item.
	idx = _idx

func _on_h_box_container_item_clicked():
	item_clicked.emit(item_id)


func _on_delete_item_pressed():
	delete_item.emit(item_id)

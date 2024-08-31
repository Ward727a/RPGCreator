extends Panel

signal clicked_on_item()

func _gui_input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		# Check if the mouse click on the panel
		clicked_on_item.emit()
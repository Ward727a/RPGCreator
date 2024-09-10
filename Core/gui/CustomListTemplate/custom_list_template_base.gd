extends Control
class_name CustomListTemplateBase
# This class is used to create a custom template for a list.
# All function NEED to be implemented in the child class.
# If any function is not implemented the list will not work at all!
# The _init function is not allowed to have any arguments.

signal pressed(node: Control)

# This function is called when the list is created.
func load():
	pass

# This function will be called at third when the list is created.
func set_data(_data: Dictionary):
	pass

# This function will be called at second when the list is created. The IDX is the unique id of the item.
func set_idx(_idx: String):
	pass

# This function will be called when the item is pressed.
#
# !!! You don't need to implement this function in the child class.
func _gui_input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
			pressed.emit(self)
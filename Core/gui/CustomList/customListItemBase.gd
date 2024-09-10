extends VBoxContainer
class_name CustomListItemBase

var metadata: Dictionary = {}
var content = []
var types = []
var item_name: String = ""
var base_theme: StyleBoxFlat
var node_item: HBoxContainer
var clickeable_panel: Panel

signal clicked_on_item(node: Node, name: String, metadata: Dictionary)

func _init():
	clickeable_panel = Panel.new()
	clickeable_panel.custom_minimum_size = Vector2(0, 30)
	clickeable_panel.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND

	add_child(clickeable_panel)
	clickeable_panel.gui_input.connect(_click_on_panel)

	var margin: MarginContainer = MarginContainer.new()
	margin.anchors_preset = MarginContainer.PRESET_FULL_RECT
	margin.add_theme_constant_override("margin_left", 5)
	margin.add_theme_constant_override("margin_right", 5)

	clickeable_panel.add_child(margin)

	node_item = HBoxContainer.new()

	margin.add_child(node_item)

# test signal clickeable panel
func _click_on_panel(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		print("Clicked on panel")

func add_object(_object_type: Node):
	pass
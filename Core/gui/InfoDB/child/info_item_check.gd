extends HBoxContainer
class_name InfoItemCheck

@export
var label_name: String = "Label":
	set(new):
		label_name = str(tr(new), ":")
		reload_label()

@export
var content: bool = false:
	set(new):
		content = new
		reload_content()

signal content_changed(content: bool)

var label_node: Label
var content_node: CheckBox

func _init():

	# Generate the label node
	label_node = Label.new()
	label_node.text = label_name
	label_node.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	# Generate the content node
	content_node = CheckBox.new()
	content_node.set_pressed(content)
	content_node.toggled.connect(_on_content_changed)

	# Add the nodes
	add_child(label_node)
	add_child(content_node)

func _ready():
	label_node.text = label_name
	content_node.set_pressed(content)

func reload_label():
	label_node.text = label_name

func reload_content():
	content_node.set_pressed(content)

func set_label(_label: String):
	label_name = _label

func set_content(_content: bool):
	content = _content

func set_max_length(_max_length: int):
	content_node.max_length = _max_length

func _on_content_changed(value: bool) -> void:
	emit_signal("content_changed", value)
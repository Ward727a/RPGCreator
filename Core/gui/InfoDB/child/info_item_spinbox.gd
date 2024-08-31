extends HBoxContainer
class_name InfoItemSpinBox

@export
var label_name: String = "Label":
	set(new):
		label_name = str(tr(new), ":")
		reload_label()


@export
var content: int = 0:
	set(new):
		content = new
		reload_content()

signal content_changed(content: int)

var label_node: Label
var content_node: SpinBox

func _init():

	# Generate the label node
	label_node = Label.new()
	label_node.text = label_name

	# Generate the content node
	content_node = SpinBox.new()
	content_node.value = content
	content_node.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	content_node.connect("value_changed", _on_content_changed)

	# Add the nodes
	add_child(label_node)
	add_child(content_node)

func _ready():
	label_node.text = label_name
	content_node.value = content

func reload_label():
	label_node.text = label_name

func reload_content():
	content_node.value = content

func set_label(_label: String):
	label_name = _label

func set_content(_content: int):
	content = _content

func _on_content_changed(value: float) -> void:
	content = int(value)
	emit_signal("content_changed", content)
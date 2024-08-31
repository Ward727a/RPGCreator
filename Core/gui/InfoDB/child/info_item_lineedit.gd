extends HBoxContainer
class_name InfoItemLineEdit

@export
var label_name: String = "Label":
	set(new):
		label_name = str(tr(new), ":")
		reload_label()


@export
var content: String = "Content":
	set(new):
		content = new
		reload_content()

signal content_changed(content: String)

var label_node: Label
var content_node: LineEdit

func _init():

	# Generate the label node
	label_node = Label.new()
	label_node.text = label_name

	# Generate the content node
	content_node = LineEdit.new()
	content_node.text = content
	content_node.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	content_node.connect("text_changed", _on_content_changed)

	# Add the nodes
	add_child(label_node)
	add_child(content_node)

func _ready():
	label_node.text = label_name
	content_node.text = content

func reload_label():
	label_node.text = label_name

func reload_content():
	content_node.text = content

func set_label(_label: String):
	label_name = _label

func set_content(_content: String):
	content = _content

func set_max_length(_max_length: int):
	content_node.max_length = _max_length

func _on_content_changed(value: String) -> void:
	emit_signal("content_changed", value)
extends HBoxContainer
class_name InfoItemOptions

@export
var label_name: String = "Label":
	set(new):
		label_name = str(tr(new), ":")
		reload_label()

@export
## Options for the OptionButton
## Example: {1: {"name": "Option1", "uid": 2301}}
var options: Dictionary = {}:
	set(new):
		options = new
		reload_options()

@export
var content: int = 0:
	set(new):
		content = new
		reload_content()


signal content_changed(content: String)

var label_node: Label
var content_node: OptionButton

func _init(_options: Dictionary = {}):

	# Generate the label node
	label_node = Label.new()
	label_node.text = label_name

	# Generate the content node
	content_node = OptionButton.new()
	content_node.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	options = _options

	content_node.connect("item_selected", _on_content_changed)

	# Add the nodes
	add_child(label_node)
	add_child(content_node)

func _ready():
	label_node.text = label_name
	content_node.selected = content

func reload_label():
	label_node.text = label_name

func reload_content():
	content_node.selected = content

func reload_options():
	for key in options:

		var option = options[key]

		if option.has("name") and option.has("uid"):
			content_node.add_item(option["name"], option["uid"])
		else:
			push_error("Option does not have a name or a uid")

func set_label(_label: String):
	label_name = _label

func set_content(_content: int):
	content = _content

func set_options(_options: Dictionary):

	if _options.size() == 0:
		return

	options = _options

func _on_content_changed(value: int) -> void:
	content = value
	emit_signal("content_changed", content)
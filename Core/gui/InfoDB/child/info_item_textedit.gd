extends VBoxContainer
class_name InfoItemTextedit

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

@export
var custom_min_size: Vector2 = Vector2(0, 0):
	set(new):
		custom_min_size = new

signal content_changed(content: String)

var label_node: Label
var content_node: RichTextLabel
var container_top: HBoxContainer
var button_advanced: Button
var window: ConfirmationDialog
var advanced_edit: AdvancedEdit

func _init():

	# Generate the HBoxContainer
	container_top = HBoxContainer.new()
	container_top.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	# Generate the label node
	label_node = Label.new()
	label_node.text = label_name
	label_node.vertical_alignment = VERTICAL_ALIGNMENT_TOP
	label_node.size_flags_horizontal = Control.SIZE_EXPAND_FILL # We use this to make the button appear on the right side

	# Generate the advanced edit button
	button_advanced = Button.new()
	button_advanced.text = "OPEN_ADVANCED_EDITOR"
	button_advanced.tooltip_text = "OPEN_ADVANCED_EDITOR_HINT"
	button_advanced.connect("pressed", _on_advanced_edit_open)

	## Add the label & button to the container
	container_top.add_child(label_node)
	container_top.add_child(button_advanced)

	# Generate the content node
	content_node = RichTextLabel.new()
	content_node.bbcode_enabled = true
	content_node.text = content
	content_node.size_flags_horizontal = Control.SIZE_EXPAND_FILL

	# We prepare the advanced edit window
	window = ConfirmationDialog.new()
	window.set_title("ADVANCED_EDITOR")
	window.unresizable = false
	window.min_size = (Vector2(400, 400))
	window.connect("confirmed", _on_advanced_edit_confirmed)

	advanced_edit = AdvancedEdit.new()
	
	window.add_child(advanced_edit)

	# Hide the window
	window.hide()

	# Add the nodes
	add_child(container_top)
	add_child(content_node)
	add_child(window)


func _ready():
	label_node.text = label_name
	content_node.text = content
	content_node.custom_minimum_size = custom_min_size

func reload_label():
	label_node.text = label_name

func reload_content():
	content_node.text = content

func set_label(_label: String):
	label_name = _label

func set_content(_content: String):
	content = _content

func _on_content_changed() -> void:

	var value: String = content_node.text

	emit_signal("content_changed", value)

func _on_advanced_edit_open() -> void:

	advanced_edit.set_content(content)

	# Show the window
	window.popup_centered()

func _on_advanced_edit_confirmed() -> void:
	
	content = advanced_edit.get_content()
	content_node.text = content

	emit_signal("content_changed", content)
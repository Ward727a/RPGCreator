extends HBoxContainer
class_name InfoItemText

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

func _ready():
	$label.text = label_name
	$content.text = content

func reload_label():
	$label.text = label_name

func reload_content():
	$content.text = content

func set_label(_label: String):
	label_name = _label

func set_content(_content: String):
	content = _content
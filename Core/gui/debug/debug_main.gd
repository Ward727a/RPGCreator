extends ConfirmationDialog

var opened: bool = false

signal debug_toggle(toggle: bool)

# Called when the node enters the scene tree for the first time.
func _ready():
	
	about_to_popup.connect(_on_popup)
	canceled.connect(_on_cancel)
	confirmed.connect(_on_confirm)
	
	pass # Replace with function body.

func get_opened():
	return opened

func set_opened(new_value: bool):
	opened = new_value
	debug_toggle.emit(new_value)

func _on_popup():
	set_opened(true)

func _on_cancel():
	set_opened(false)

func _on_confirm():
	set_opened(false)

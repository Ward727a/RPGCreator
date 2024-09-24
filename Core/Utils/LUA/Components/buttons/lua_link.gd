extends LUA_Button
class_name LUA_Link

var open_confirmation: ConfirmationDialog
var uri: String = ""

func lua_fields():
	return ['_ready', '_pressed', 'open_confirmation', '_on_accept', 'add_child', 'remove_child', 'get_children', 'get_child', 'get_child_count', 'find_child', 'find_children', 'move_child']

# Called when the node enters the scene tree for the first time.
func _ready():
	
	flat = true
	mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
	
	open_confirmation = ConfirmationDialog.new()
	
	open_confirmation.set_title('CONFIRM_OPEN_LINK')
	
	open_confirmation.confirmed.connect(_on_accept)
	
	add_child(open_confirmation)
	
	pass # Replace with function body.

func _pressed():
	
	open_confirmation.set_text('This button try to redirect you to the site: %s\nAre you sure that you want to open this website?' % uri)
	open_confirmation.popup_centered()
	
	super()

func _on_accept():
	
	OS.shell_open(uri)

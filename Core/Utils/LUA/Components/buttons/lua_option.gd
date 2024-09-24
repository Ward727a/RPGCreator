extends OptionButton
class_name LUA_OptionButton

var lua: LuaAPI

var on_pressed
var on_button_down
var on_button_up
var on_item_select
var on_item_focus

func _ready():
	item_selected.connect(_on_item_selected)
	item_focused.connect(_on_item_focused)
	on_button_down.connect(_on_button_down)
	on_button_up.connect(_on_button_up)

func _pressed():
	
	if on_pressed != null:
		lua.call_function(on_pressed, [])

func _on_button_down():
	
	if on_pressed != null:
		lua.call_function(on_button_down, [])

func _on_button_up():
	
	if on_pressed != null:
		lua.call_function(on_button_up, [])

func _on_item_selected(item_key: int):
	
	if on_item_select != null:
		lua.call_function(on_item_select, [item_key])

func _on_item_focused(item_key: int):
	
	if on_item_focus != null:
		lua.call_function(on_item_focus, [item_key])

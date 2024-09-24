extends Button
class_name LUA_Button

var lua: LuaAPI

var on_pressed

func lua_fields():
	return []

func _pressed():
	
	if on_pressed != null:
		lua.call_function(on_pressed, [])
	

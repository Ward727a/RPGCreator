extends Object
class_name LuaLoader

signal setted_api()
signal setted_path()
signal loaded_script()

static var lua_scripts: Dictionary = {}

static var registers: Dictionary = {
	"character": CharacterRegister,
	"class": Register,
	"data": DataRegister,
	"effect": EffectRegister,
	"enum": EnumRegister,
	"history": HistoryRegister,
	"skill_conditions": SkillConditionsRegister,
	"skill": SkillRegister
}

var lua: LuaAPI
var lua_path: String = ""

var lua_loaded: bool = false
var allowed_folders: Array[String] = []

func _init(_lua_path: String):
	
	if !FileAccess.file_exists(_lua_path):
		push_error("Error when trying to load a LUA Script:\n\t'",_lua_path,"' doesn't exist!")
		free()
	
	lua = LuaAPI.new()
	self.lua_path = _lua_path
	
	var lua_locked_api: LuaLockedAPI = LuaLockedAPI.new(lua)
	
	lua.bind_libraries(["base", "string", "math", "table"])
	
	lua.push_variant("print", _lua_print_message)
	lua.push_variant("warn", _lua_warn_message)
	lua.push_variant("error", _lua_error_message)
	
	lua.push_variant("registers", registers)
	lua.push_variant("lua_object", lua_locked_api)
	lua.push_variant('var_to_str', test_var_to_str)
	
	lua_scripts[_lua_path] = {
		"loaded": false,
		"api": lua
	}

func test_var_to_str(variable: Variant) -> String:
	return var_to_str(variable)

func get_api() -> LuaAPI:
	return lua

func set_api(new_api: LuaAPI):
	lua = new_api
	setted_api.emit()

func get_path() -> String:
	return lua_path

## Set a path to an other lua script.
func set_path(new_lua_path: String):
	
	if lua_scripts.has(new_lua_path):
		if lua_scripts[new_lua_path].loaded:
			push_warning("Can't load an already loaded LUA script!")
			return
		
	if FileAccess.file_exists(new_lua_path):
		push_error("Error when trying to set path to LUA Script:\n\t'",lua_path,"' doesn't exist!")
		return
	
	if lua_scripts.has(lua_path):
		lua_scripts[lua_path].loaded = false
		lua_scripts[lua_path].api = null
	
	lua_path = new_lua_path
	
	lua_scripts[lua_path] = {
		"loaded": false,
		"api": lua
	}
	
	lua_loaded = false
	setted_path.emit()

func load_lua(args: Array = [], strict: bool = false) -> Dictionary:
	var err = lua.do_file(lua_path, args)
	
	if strict:
		if !lua.function_exists('loaded') or !lua.function_exists('unloaded'):
			return {"success": false, "load_function_missing": true}
	
	if err is LuaError:
		print(err.message)
		return {"success": false}
	
	lua_scripts[lua_path].loaded = true
	lua_loaded = true
	
	check_permission() # Check the permission required by the lua
	
	loaded_script.emit()
	
	if lua.function_exists('loaded'):
	
		err = lua.call_function('loaded', [])
		
		if err is LuaError:
			print(err.message)
			return {"success": false}
	
	
	return {"success": true}

func unload():
	lua_scripts[lua_path].loaded = false
	lua_scripts[lua_path].api = null
	if lua.function_exists('unloaded'):
		lua.call_function('unloaded', [])
	free()

func check_permission():
	
	if !lua_loaded:
		return
	
	var permissions = lua.pull_variant('permission')
	
	if permissions == null: # If their is no permission required, it's kind of strange, but ok, we just print a warning message
		push_warning('No permission required for lua: %s' % lua_path)
		return
	
	if typeof(permissions) != TYPE_ARRAY: # We check the type of the permissions variable
		push_error('Permission variable should be an array in the lua script: %s but is %s ' % [lua_path, type_string(typeof(permissions))])
		return
	
	for permission in permissions:
		
		match(permission):
			'UI': # Give the permission to create / edit / remove / get UI
				UIRegister.load_lua(lua)
				LuaUI.bind_UI_var(lua)
			'UI:ITEM_PARAMETER_BIND': # Give the permission to bind parameter to UI
				push_warning('Permission "%s" is not implemented!' % permission)
				continue
			'SKILL_CONDITION': # Give the permission to create / edit / remove / get skill condition
				push_warning('Permission "%s" is not implemented!' % permission)
				continue
			'SKILL_EFFECT': # Give the permission to create / edit / remove / get skill effect
				push_warning('Permission "%s" is not implemented!' % permission)
				continue
			'ITEM_PARAMETERS': # Give the permission to create / edit / remove / get item parameters
				ItemParametersRegister.load_lua(lua)
			_: # If the permission isn't listed, it doesn't exist, print an error message
				push_error('Permission "%s" is not a valid permission!' % permission)
				continue


## global lua function


func _lua_print_message(message: String):
	print("[LUA] ",message)

func _lua_warn_message(message: String):
	print("[LUA - Warn] ", message)

func _lua_error_message(message: String):
	printerr("[LUA - ERROR] ", message)

## This is a class that will allow to get the LuaAPI[br]
## This can be shared to the LUA script as it's locked, and only the software can get the value inside
class LuaLockedAPI:
	
	var lua: LuaAPI
	
	func _init(lua_api: LuaAPI):
		lua = lua_api
	
	## We return 'lua' to forbide the access to the var from LUA[br]
	## This is due to security reasons (if LUA script has access to this[br]
	## it can be used to load [b]ANY[/b] libraries!!
	func lua_fields():
		return ['lua']

extends Base
class_name BaseEffect

const EffectTypes = EnumRegister.EffectTypes
const EffectOutcome = EnumRegister.EffectOutcome
const EffectTime = EnumRegister.EffectTime
const EffectTarget = EnumRegister.EffectTarget

var lua_script: String
var lua_loader: LuaLoader
var lua_api: LuaAPI

var id: String = "" # Effect's ID

var name: String = "" # Effect's name
var description: String = "" # Effect's description

var duration: int = 0 # Effect's duration if applicable

var effect_type: EffectTypes = EffectTypes.CORE_STATS # Effect's type
var effect_outcome: EffectOutcome = EffectOutcome.NEUTRAL # Effect's outcome
var effect_time: EffectTime = EffectTime.INSTANT # Effect's time (instant, over time, or permanent)
var effect_target: EffectTarget = EffectTarget.SELF # Effect's target (self, ally, enemy, all)

var exported_value: Dictionary = {}

## This function will get all the value from the lua script to set them in this class.
func _init(_lua_script: String):
	lua_script = _lua_script
	lua_loader = LuaLoader.new(lua_script)
	lua_api = lua_loader.get_api()
	
	lua_api.push_variant("getEffect", _lua_get_effect)
	lua_api.push_variant("getName", _lua_get_name)
	lua_api.push_variant("getDescription", _lua_get_description)
	lua_api.push_variant("getID", _lua_get_id)
	lua_api.push_variant("getDuration", _lua_get_duration)
	lua_api.push_variant("setDuration", _lua_set_duration)
	lua_api.push_variant("setID", _lua_set_id)
	lua_api.push_variant("setDescription", _lua_set_description)
	lua_api.push_variant("setName", _lua_set_name)
	
	# enums
	lua_api.push_variant("getType", _lua_get_type)
	lua_api.push_variant("getTime", _lua_get_time)
	lua_api.push_variant("getOutcome", _lua_get_outcome)
	lua_api.push_variant("getTarget", _lua_get_target)
	lua_api.push_variant("setType", _lua_set_type)
	lua_api.push_variant("setTime", _lua_set_time)
	lua_api.push_variant("setOutcome", _lua_set_outcome)
	lua_api.push_variant("setTarget", _lua_set_target)
	
	# Exported
	lua_api.push_variant("getExports", _lua_get_exporteds)
	lua_api.push_variant("getExport", _lua_get_export)
	lua_api.push_variant("addExport", _lua_add_export)
	
	if !lua_loader.load_lua():
		free()
		return 
	
	id = lua_api.pull_variant('effect_id')
	name = lua_api.pull_variant('effect_name')
	description = lua_api.pull_variant('effect_hint')
	
	if !lua_api.function_exists("init"):
		push_error("ERROR: '", lua_script, "' doesn't have the 'init' function!")
		free()
		return 
	
	if !lua_api.function_exists("apply_effect"):
		push_error("ERROR: '", lua_script, "' doesn't have the 'apply_effect' function!")
		free()
		return 
	
	lua_api.call_function("init", [])

# Function to apply the effect
# It should apply the effect to the target
func apply_effect(_target: Character) -> void:
	
	lua_api.call_function("apply_effect", [_target])
	
	pass

func duplicate() -> BaseEffect:
	var dupli: BaseEffect = BaseEffect.new(lua_script)
	
	dupli.id = id
	dupli.name = name
	dupli.description = description
	dupli.lua_loader = lua_loader
	dupli.lua_api = lua_api
	dupli.lua_script = lua_script
	dupli.duration = duration
	dupli.effect_target = effect_target
	dupli.effect_type = effect_type
	dupli.effect_time = effect_time
	dupli.effect_outcome = effect_outcome
	dupli.exported_value = exported_value
	
	return dupli

func to_dictionary() -> Dictionary:
	
	var data: Dictionary = {}
	
	data = {
		"lua_script": lua_script,
		"effect_target": effect_target,
		"effect_time": effect_time,
		"effect_outcome": effect_outcome,
		"effect_type": effect_type,
		"exported_value": exported_value
	}
	
	return data

static func from_dictionary(data: Dictionary) -> BaseEffect:
	
	var effect: BaseEffect = BaseEffect.new(data['lua_script'])
	effect.effect_outcome = data['effect_outcome']
	effect.effect_time = data['effect_time']
	effect.effect_target = data['effect_target']
	effect.effect_type = data['effect_type']
	
	for key in data['exported_value'].keys():
		if effect.exported_value.has(key):
			effect.exported_value[key] = data['exported_value'][key]
	
	return effect

## All theses functions are reserved for the LUA plugins system!!

func _lua_get_effect():
	return self

func _lua_set_name(new_name: String):
	name = new_name

func _lua_set_id(new_id: String):
	id = new_id

func _lua_set_description(new_description: String):
	description = new_description

func _lua_set_duration(new_duration: int):
	duration = new_duration

func _lua_get_name() -> String:
	return name

func _lua_get_id() -> String:
	return id

func _lua_get_description() -> String:
	return description

func _lua_get_duration() -> int:
	return duration

func _lua_set_type(value: int):
	effect_type = Utils.cast_to_enum(value, EnumRegister.EffectTypes)

func _lua_set_target(value: int):
	effect_target = Utils.cast_to_enum(value, EnumRegister.EffectTarget)

func _lua_set_time(value: int):
	effect_time = Utils.cast_to_enum(value, EnumRegister.EffectTime)

func _lua_set_outcome(value: int):
	effect_outcome = Utils.cast_to_enum(value, EnumRegister.EffectOutcome)

func _lua_get_type() -> int:
	var type: int = effect_type
	return type

func _lua_get_outcome() -> int:
	var outcome: int = effect_outcome
	return outcome

func _lua_get_time() -> int:
	var time: int = effect_time
	return time

func _lua_get_target() -> int:
	var target: int = effect_target
	return target

func _lua_add_export(_name: String, default: Variant, type: String, hint: String) -> bool:
	
	if _name == "":
		print("error")
		return false
	
	if exported_value.has(_name):
		print("error")
		return false
	
	if default == null:
		print("error")
		return false
	
	if type == "":
		print("error")
		return false
	
	if hint == "":
		print("error")
		return false
	
	exported_value[_name] = {
		"default": default,
		"type": type,
		"hint": hint
	}
	
	return true

func _lua_get_exporteds() -> Array:
	
	var exported_as_array = []
	
	for key in exported_value:
		exported_as_array.push_back(key)
	
	return exported_as_array

func _lua_get_export(_name: String) -> _c_lua_export:
	
	if _name == "":
		return _c_lua_export.new()
	
	if !exported_value.has(_name):
		return _c_lua_export.new()
	
	var data: Dictionary = exported_value.get(_name)
	var exported: _c_lua_export = _c_lua_export.new()
	
	exported.default = data['default']
	exported.hint = data['hint']
	exported.type = data['type']
	exported.value = data['value'] if data.has('value') else data['default']
	
	return exported

class _c_lua_export:
	var default: Variant = ""
	var type: String = ""
	var hint: String = ""
	var value: Variant = ""

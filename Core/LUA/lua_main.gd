extends Node
class_name LuaObject

signal STARTED(success: bool)
signal STOPPED(success: bool)
signal PUSH_VARIANT(var_name: String, var_value: Variant)
signal PULL_VARIANT(var_name: String, var_value: Variant)
signal CALL_FN(fn_name: String, args: Array)

var _plugin_name: String = ""
var _script_path: String = ""
var _script_name: String = ""
var _api: LuaAPI

var logger: Logger
var started: bool = false

func _init(plugin_name: String, script_path: String):
	
	_plugin_name = plugin_name
	_script_path = script_path
	_script_name = _script_path.rsplit("/", true, 1)[1]
	_script_name = _script_name.split(".")[0]
	
	logger = Logger.new(str("%s - %s - " % [_plugin_name, _script_name]))
	_api = LuaAPI.new()
	_api.bind_libraries(["table","string","base","math","utf8","coroutine"])
	_api.push_variant("node_to_lua", node_to_lua)
	_api.push_variant("node_from_lua", node_from_lua)
	_api.push_variant("print", _print)
	
	add_advanced_string_utility()

func add_advanced_string_utility():
	
	var string_lib = _api.pull_variant('string')
	
	string_lib['begins_with'] = LUA_StringUtility.begins_with
	string_lib['replace'] = LUA_StringUtility.replace
	
	_api.push_variant('string', string_lib)

func _print(message, var_to_str: bool = false):
	if var_to_str:
		print(var_to_str(message))
		return
	print(message)

func is_valid() -> bool:
	
	return _api != null

func start() -> bool:
	
	
	_api.set_registry_value("folder_path", _script_path.rsplit("/", true, 1)[0])
	_api.set_registry_value("script_path", _script_path)
	
	if started:
		logger.warning("Script already started, stop it if you want to start it again.")
		return true
	
	if !FileAccess.file_exists(_script_path):
		logger.warning("Script path return to a not existing file, can't start.")
		return false
	
	if !_script_path.ends_with(".lua"):
		logger.warning("Script path doesn't end with \".lua\", can't start.")
		return false
	
	var err = _api.do_file(_script_path)
	
	if err is LuaError:
		logger.error("Couldn't load the script file due to an Lua error!")
		logger.error(err.message)
		return false
	
	var start_return = await CALL('start')
	
	if start_return is LuaError:
		logger.error("function \"start\" got an error: %s" % start_return.message)
		return false
	
	if typeof(start_return) != TYPE_BOOL:
		logger.error("function \"start\" should return a boolean (true/false) but it return %s" % type_string(typeof(start_return)))
		return false
	
	if GlobalEventManager.is_valid():
		GlobalEventManager.get_singleton().CALL("editor/plugin/script/started", [_plugin_name, _script_path, start_return])
	
	STARTED.emit(start_return)
	
	if start_return:
		started = true
		return true
	else:
		await CALL("stop")
		started = false
		return false

func stop() -> bool:
	
	print("Stop plugin!")
	
	if !is_valid():
		logger.warning("API is not valid, can't stop.")
		return false
	
	if !started:
		logger.warning("Script already stopped.")
		return true
	
	var stop_return = await CALL('stop')
	
	if typeof(stop_return) != TYPE_BOOL:
		logger.error("function \"stop\" should return a boolean (true/false) but it return %s" % type_string(typeof(stop_return)))
		return false
	
	if GlobalEventManager.is_valid():
		GlobalEventManager.get_singleton().CALL("editor/plugin/script/stopped", [_plugin_name, _script_path, stop_return])
	
	STOPPED.emit(stop_return)
	
	started = false
	
	return stop_return

func CALL(fn_name: String, args: Array = []) -> Variant:
	
	CALL_FN.emit(fn_name, args)
	
	if !has_fn(fn_name):
		logger.warning("Function %s doesn't exist in the script." % fn_name)
		return null
	
	return _api.call_function(fn_name, args)

func has_fn(fn_name: String) -> bool:
	
	return _api.function_exists(fn_name)

func has_variant(var_name: String) -> bool:
	
	return (pull_variant(var_name) != null)

func push_variant(var_name: String, var_value: Variant) -> bool:
	
	if !is_valid():
		return false
	
	PUSH_VARIANT.emit(var_name, var_value)
	
	var err = _api.push_variant(var_name, var_value)
	
	if err is LuaError:
		logger.error("Error when trying to push_variant: %s" % err.message)
		return false
	
	return true

func pull_variant(var_name: String) -> Variant:
	
	if !is_valid():
		return null
	
	var variant_value = _api.pull_variant(var_name)
	
	PULL_VARIANT.emit(var_name, variant_value)
	return variant_value

func node_to_lua(node: Node) -> String:
	return var_to_str(node)

func node_from_lua(node: String) -> Node:
	if str_to_var(node) is Node:
		return str_to_var(node)
	return null

extends Node
class_name GlobalEventManager

static var singleton: GlobalEventManager

static func get_singleton() -> GlobalEventManager:
	return singleton

static func is_valid() -> bool:
	return (GlobalEventManager.get_singleton() != null)

func _init():
	singleton = self

var events = {}
var logger: Logger = Logger.new("GlobalEventManager")

func SET(path: String, callback: Callable) -> bool:
	
	if typeof(path) != TYPE_STRING:
		return false
	
	if path.is_empty():
		return false
	
	if typeof(callback) != TYPE_CALLABLE:
		return false
	
	var path_parts: PackedStringArray = path.split('/')
	var last_part: String = path_parts[path_parts.size() - 1]
	
	var data = events
	
	for part in path_parts:
		
		if last_part == part:
			
			if data.has(part):
				
				if typeof(data[part]) != TYPE_ARRAY:
					return false
				
				if data[part].has(callback):
					return false
				
				data[part].push_back(callback)
				return true
			
			data[part] = [callback]
			return true
		
		if data.has(part):
			data = data[part]
			continue
		
		data[part] = {}
		data = data[part]
	
	return false

func GET(path: String = "") -> Array:
	
	if typeof(path) != TYPE_STRING:
		return []
	
	if path.is_empty():
		return []
	
	var path_parts: PackedStringArray = path.split('/')
	var last_part: String = path_parts[path_parts.size() - 1]
	
	var data = events
	
	for part in path_parts:
		
		if last_part == part:
			
			if data.has(part):
				
				if typeof(data[part]) != TYPE_ARRAY:
					break
				
				return data[part]
			
			break
		
		if data.has(part):
			data = data[part]
			continue
		
		break
	
	return []

func CALL(path: String, args: Array = []):
	
	var functions = GET(path)
	
	for fn: Callable in functions:
		
		var binded_fn = fn.bindv(args)
		binded_fn.call()

func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_perm = [
		"GET",
		"SET",
		"CALL"
	]
	
	for perm in possible_perm:
		if _plugin_obj.has_allowed("EVENTS_MANAGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("EVENTS_MANAGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_perm.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("EventsManager", lua_obj)

class _lua_class:
	
	var fields = []
	var singleton: GlobalEventManager
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('singleton')
		singleton = GlobalEventManager.get_singleton()
	
	func lua_fields():
		return fields
	
	func is_valid() -> bool:
		return singleton != null
	
	func reload_singleton() -> bool:
		singleton = GlobalEventManager.get_singleton()
		return is_valid()
	
	func SET(path: String, callback: Callable) -> bool:
		return singleton.SET(path, callback)
	
	func GET(path: String = "") -> Array:
		return singleton.GET(path)
	
	func CALL(path: String, args: Array = []):
		singleton.CALL(path, args)

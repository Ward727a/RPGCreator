extends Object
class_name Logger

var obj_name: String

enum LOG_LEVEL {
	NORMAL,
	WARNING,
	ERROR,
	DEBUG
}

func _init(object_name: String):
	
	obj_name = object_name

func msg(message: String, level: LOG_LEVEL = LOG_LEVEL.NORMAL):
	
	match level:
		LOG_LEVEL.NORMAL:
			print_rich(str("[color=#1E90FF](%s) %s") % [obj_name, message])
		LOG_LEVEL.WARNING:
			print_rich(str("[color=yellow](%s) %s") % [obj_name, message])
			push_warning(str("(%s - warning) %s") % [obj_name, message])
		LOG_LEVEL.ERROR:
			print_rich(str("[color=red](%s) %s") % [obj_name, message])
			push_error(str("(%s - ERROR) %s") % [obj_name, message])
		LOG_LEVEL.DEBUG:
			print_rich(str("[color=cyan](%s) %s") % [obj_name, message])
			print_debug(str("(%s - debug) %s") % [obj_name, message])
		_:
			print(str("(%s) %s") % [obj_name, message])

func log(message: String):
	
	msg(message, LOG_LEVEL.NORMAL)

func warn(message: String):
	
	msg(message, LOG_LEVEL.WARNING)

func warning(message: String):
	
	warn(message)

func error(message: String):
	
	msg(message, LOG_LEVEL.ERROR)

func debug(message: String):
	
	msg(message, LOG_LEVEL.DEBUG)

static func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_perm = [
		"log",
		"warn",
		"warning",
		"debug",
		"error"
	]
	
	for perm in possible_perm:
		if _plugin_obj.has_allowed("LOGGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("LOGGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_perm.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("Logger", lua_obj)

class _lua_class:
	
	var fields = []
	var log_obj = {}
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('log_obj')
		fields.push_back('create_logger_obj')
	
	func lua_fields():
		return fields
	
	func reset_logger_obj():
		log_obj = {}
	
	func create_logger_obj(name: String) -> Logger:
		
		name = str("Lua - %s" % name)
		
		if log_obj.has(name):
			return log_obj[name]
		
		@warning_ignore("shadowed_global_identifier") var log = Logger.new(name)
		log_obj[name] = log
		return log
	
	func log(name: String, message: String):
		create_logger_obj(name).log(message)
	
	func warn(name: String, message: String):
		create_logger_obj(name).warn(message)
	
	func warning(name: String, message: String):
		create_logger_obj(name).warning(message)
	
	func error(name: String, message: String):
		create_logger_obj(name).error(message)
	
	func debug(name: String, message: String):
		create_logger_obj(name).debug(message)

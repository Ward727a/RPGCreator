extends Node
class_name InterfaceManager

static var singleton: InterfaceManager

var interface_data: Dictionary = {}
var logger: Logger = Logger.new("InterfaceManager")

static func get_singleton() -> InterfaceManager:
	return singleton

static func is_valid() -> bool:
	return (InterfaceManager.get_singleton() != null)

func _init():
	singleton = self

func create_ui(_name: String, _root: Node) -> bool:
	
	if _has_in_data(_name):
		logger.error("Can't add UI \"%s\" because their is already an UI with this name!" % _name)
		return false
	
	_add_to_data(_name, _root)
	return true

func has_ui(_name: String) -> bool:
	return _has_in_data(_name)

func get_ui(_name: String) -> Node:
	
	if !_has_in_data(_name):
		logger.error("Can't get UI \"%s\" because their is no UI with this name!" % _name)
		return null
	
	return _get_from_data(_name).root

func set_ui(_name: String, _root: Node) -> bool:
	
	var old_root = null
	
	if _has_in_data(_name):
		old_root = get_ui(_name)
	
	var new_root = _set_in_data(_name, _root).root
	
	if old_root != null:
		if old_root.is_inside_tree():
			logger.log("UI \"%s\" is inside the tree, replacing it with the new UI." % _name)
			for child in old_root.get_children():
				child.queue_free()
			
			old_root.replace_by(new_root, true)
			old_root.queue_free()
	
	return true

func remove_ui(_name: String) -> bool:
	
	if !_has_in_data(_name):
		logger.error("Can't remove UI \"%s\" because their is no UI called like that!" % _name)
		return false
	
	var node: Node = get_ui(_name)
	
	if node.is_inside_tree():
		logger.log("Root from UI \"%s\" is inside tree, removing it." % _name)
		
		node.queue_free()
	
	return true
	

func _has_in_data(_name: String) -> bool:
	return interface_data.has(_name)

func _add_to_data(_name: String, _root: Node) -> ResInterface:
	
	var res = ResInterface.new()
	res.id = IDGenerator.generate_id(16)
	res.root = _root
	
	interface_data[_name] = res
	
	return res

func _remove_from_data(_name: String) -> bool:
	
	if !_has_in_data(_name):
		return false
	
	return interface_data.erase(_name)

func _get_from_data(_name: String) -> ResInterface:
	
	if !_has_in_data(_name):
		return null
	
	return interface_data[_name]

func _set_in_data(_name: String, _root: Node) -> ResInterface:
	
	var res = ResInterface.new()
	res.id = IDGenerator.generate_id(16)
	res.root = _root
	
	interface_data[_name] = res
	return res

func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_permissions = [
		"SET",
		"ADD",
		"REMOVE",
		"HAS",
		"GET"
	]
	
	for perm in possible_permissions:
		if _plugin_obj.has_allowed("INTERFACE_MANAGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("INTERFACE_MANAGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_permissions.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("InterfaceManager", lua_obj)

class _lua_class:
	
	var fields = []
	var singleton: InterfaceManager
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('singleton')
		singleton = InterfaceManager.get_singleton()
	
	func lua_fields():
		return fields
	
	func is_valid() -> bool:
		return singleton != null
	
	func reload_singleton() -> bool:
		singleton = InterfaceManager.get_singleton()
		return is_valid()
	
	func ADD(_name: String, _root: Node) -> bool:
		return singleton.create_ui(_name, _root)
	
	func SET(_name: String, _root: Node) -> bool:
		return singleton.set_ui(_name, _root)
	
	func REMOVE(_name: String) -> bool:
		return singleton.remove_ui(_name)
	
	func HAS(_name: String) -> bool:
		return singleton.has_ui(_name)
	
	func GET(_name: String) -> Node:
		return singleton.get_ui(_name)

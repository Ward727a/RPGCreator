extends Base
class_name Register
# This class is used to register all classes in the engine to be able to use them in the game
# This class should NEVER be in the "classes" variables (never call "super" in the "_init" function), if you need to use it, use the get_instance function

static var self_class: Register = null

var allowed_classes: Array = [
	"quest",
	"loottables",
	"npc",
	"item",
	"skill",
	"effect",
	"area",
	"gui",
	"dialog",
	"event",
	"condition",
	"action",
	"trigger",
	"questobjective",
	"questreward",
	"queststatus",
	"questtype",
	"questlog",
	"questtracker",
	"questtrackeritem",
	"questtrackerobjective",
	"questtrackerreward",
	"questtrackerstatus",
	"questtrackertype",
	"questtrackerlog",
	"questtrackertracker",
	"questtrackertrackeritem",
	"questtrackertrackerobjective",
	"questtrackertrackerreward",
	"questtrackertrackerstatus",
	"questtrackertrackertype",
	"questtrackertrackerlog"
	]

var classes = {}
var plugins = {}

static func get_instance() -> Register:
	if self_class == null:
		self_class = Register.new()
	return self_class

func _init():
	allowed_events = ["pre_register", "post_register", "pre_unregister", "post_unregister", "pre_plugin_class_register", "post_plugin_class_register", "pre_plugin_class_unregister", "post_plugin_class_unregister"]

## Register an engine class - This should not be called from a custom plugin! Use the register_class_plugin function instead
func register_class(_class: Base):
	# Emit the pre_register event
	call_event("pre_register", [_class])

	var basename = _class.get_custom_name()

	if !allowed_classes.has(basename):
		error(str("Class ", basename, " is not allowed, allowed classes are: ", allowed_classes))
		return

	if !classes.has(basename):
		classes[basename] = _class
	
	classes[basename].append(_class)

	_class.add_event("post_unregister", _unregister_class)

	# Emit the post_register event
	call_event("post_register", [_class])

	_class.notify_registered()

## Unregister an engine class - This should not be called from a custom plugin! Use the unregister_class_plugin function instead
## If the class has a notify_unregister function that return true, it will be linked to the post_unregister event
func unregister_class(_class: Base):
	# Emit the pre_unregister event
	call_event("pre_unregister", [_class])

	var basename = _class.get_custom_name()

	if !allowed_classes.has(basename):
		error(str("Class ", basename, " is not allowed, allowed classes are: ", allowed_classes))
		return
	
	if classes.has(basename):
		_class.notify_unregister()

func _unregister_class(_class: Base):
	classes.erase(_class.get_custom_name())

	# Emit the post_unregister event
	call_event("post_unregister", [_class])

## Get a class by its name - This should not be called to get a custom plugin! Use the get_class_by_name_plugin function instead
func get_class_by_name(name: String) -> Base:
	var basename = name.to_lower()

	if classes.has(basename):
		return classes[basename]
	else:
		error(str("Class ", basename, " not found"))
		return null

## Check if the class is registered
func is_registered_class(name: String) -> bool:
	var basename = name.to_lower()

	return classes.has(basename)

## Register a custom plugin class.
## Be aware that all custom class name within a plugin should be unique
## All classes registered with this function will be available for every script in the engine
func register_class_plugin(plugin_name: String, _class: Base) -> void:
	# Emit the pre_plugin_class_register event
	call_event("pre_plugin_class_register", [plugin_name, _class])

	plugin_name = plugin_name.to_lower()

	if !plugins.has(plugin_name):
		plugins[plugin_name] = {}

	if !_class.has_custom_name():
		error(str("Plugin ",plugin_name," class ", _class.get_class().get_basename(), " does not have a custom name"))
		return
	
	var basename = _class.get_custom_name()

	if plugins[plugin_name].has(basename):
		error(str("Plugin ",plugin_name," class ", basename, " already exists"))
		return
	
	plugins[plugin_name][basename] = _class

	_class.add_event("post_unregister", _unregister_plugin_class)

	# Emit the post_plugin_class_register event
	call_event("post_plugin_class_register", [plugin_name, _class])

	_class.notify_registered()

## Unregister a custom plugin class
## You can have _class empty only if you set the custom_name in the class
func unregister_class_plugin(plugin_name: String, _class: Base = null, _custom_name: String = "") -> void:

	if _class == null:
		if _custom_name == "":
			error("You need to set the custom_name as argument if you don't set the _class argument")
			return
		else:
			_class = get_class_by_name_plugin(plugin_name, _custom_name)

			if _class == null:
				error(str("Plugin ",plugin_name," class ", _custom_name, " not found"))
				return

	# Emit the pre_plugin_class_unregister event
	call_event("pre_plugin_class_unregister", [plugin_name, _class])

	plugin_name = plugin_name.to_lower()

	if !plugins.has(plugin_name):
		error(str("Plugin ",plugin_name," not found"))
		return

	var basename = _class.get_custom_name()

	if plugins[plugin_name].has(basename):
		_class.notify_unregister()
	else:
		error(str("Plugin ",plugin_name," class ", basename, " not found"))
		return
	
	# Emit the post_plugin_class_unregister event
	call_event("post_plugin_class_unregister", [plugin_name, _class])

## Get a plugin class by its name
func get_class_by_name_plugin(plugin_name: String, name: String) -> Base:
	plugin_name = plugin_name.to_lower()
	var basename = name.to_lower()

	if plugins.has(plugin_name):
		if plugins[plugin_name].has(basename):
			return plugins[plugin_name][basename]
	else:
		error(str("Plugin ",plugin_name," not found"))
		return null
	
	error(str("Plugin ",plugin_name," class ", basename, " not found"))
	return null

## Check if the class is registered in a custom plugin
func is_registered_class_plugin(plugin_name: String, name: String) -> bool:
	plugin_name = plugin_name.to_lower()
	var basename = name.to_lower()

	if plugins.has(plugin_name):
		if plugins[plugin_name].has(basename):
			return true
	return false

## Unregister all custom plugin classes
func unregister_all_classes_plugin(plugin_name: String) -> void:
	if !plugins.has(plugin_name):
		error(str("Plugin ",plugin_name," not found"))
		return

	plugins.erase(plugin_name)

## Unregister all classes
func unregister_all_classes() -> void:
	for i in classes:
		
		var _class: Base = classes[i]

		_class.notify_unregister()

func _unregister_plugin_class(plugin_name: String, _class: Base):

	var basename = _class.get_custom_name()

	if plugins.has(plugin_name):
		if plugins[plugin_name].has(basename):
			plugins[plugin_name].erase(basename)
	
	# Emit the post_unregister event
	call_event("post_unregister", [_class])

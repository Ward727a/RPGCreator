extends Node
class_name ThemeManager

static var theme_data: Dictionary
static var singleton: ThemeManager

static func get_singleton() -> ThemeManager:
	return singleton

static func is_valid() -> bool:
	return (ThemeManager.get_singleton() != null)

func _init():
	singleton = self

func add_theme(theme_name: String, theme: Theme):
	if !has_theme(theme_name):
		theme_data[theme_name] = theme
		return true
	return false

func remove_theme(theme_name: String) -> bool:
	if has_theme(theme_name):
		return theme_data.erase(theme_name)
	return false

func has_theme(theme_name: String) -> bool:
	return theme_data.has(theme_name)

func set_theme(theme_name: String, theme: Theme):
	theme_data[theme_name] = theme

func get_theme(theme_name: String) -> Theme:
	if has_theme(theme_name):
		return theme_data[theme_name]
	return null

func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_perm = [
		"add_theme",
		"remove_theme",
		"has_theme",
		"set_theme",
		"get_theme"
	]
	
	for perm in possible_perm:
		if _plugin_obj.has_allowed("THEME_MANAGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("THEME_MANAGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_perm.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("ThemeManager", lua_obj)

class _lua_class:
	
	var fields = []
	var singleton: ThemeManager
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('singleton')
		singleton = ThemeManager.get_singleton()
	
	func lua_fields():
		return fields
	
	func is_valid() -> bool:
		return singleton != null
	
	func reload_singleton() -> bool:
		singleton = ThemeManager.get_singleton()
		return is_valid()
	
	func add_theme(theme_name: String, theme: Theme):
		singleton.add_theme(theme_name, theme)
	
	func remove_theme(theme_name: String) -> bool:
		return singleton.remove_theme(theme_name)
	
	func has_theme(theme_name: String) -> bool:
		return singleton.has_theme(theme_name)
	
	func set_theme(theme_name: String, theme: Theme) -> bool:
		return singleton.set_theme(theme_name, theme)
	
	func get_theme(theme_name: String) -> Theme:
		return singleton.get_theme(theme_name)

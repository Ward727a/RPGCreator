extends Node
class_name PluginManager

signal plugin_list_reloaded()
signal plugin_ready(plugin_name: String)

const data_path: String = "editor/plugins"

static var singleton: PluginManager

var logger: Logger = Logger.new("PluginManager")

var plugins: Dictionary = {}
var plugins_folder: String
var software_version: String

static func get_singleton() -> PluginManager:
	return singleton

static func is_valid() -> bool:
	return (PluginManager.get_singleton() != null)

func _init():
	singleton = self
	
	if DataManager.is_valid():
		plugins = DataManager.get_singleton().GET(data_path, {})
	
	if Settings.is_valid() != null:
		plugins_folder = Settings.get_singleton().SettingsData.plugins_folder
		software_version = Settings.get_singleton().SoftwareInfo.version
		if !_check_folder():
			logger.error("Can't access plugin folder \"%s\"!" % plugins_folder)
			queue_free()
	else:
		logger.error("Can't access Settings manager.")
		queue_free()

func _check_folder() -> bool:
	
	if plugins_folder.is_empty():
		return false
	
	if DirAccess.dir_exists_absolute(plugins_folder):
		return true
	
	var err = DirAccess.make_dir_recursive_absolute(plugins_folder)
	
	return (err == OK)

func VERIFY_PLUGINS():
	
	var folders = DirAccess.get_directories_at(plugins_folder)
	
	var added_plugin: int = 0
	
	for folder: String in folders:
		
		var folder_path: String = str(plugins_folder, folder)
		var plugin_config_path: String = str(folder_path, "/config.json")
		
		if FileAccess.file_exists(plugin_config_path):
			var config_content: String
			
			config_content = FileAccess.get_file_as_string(plugin_config_path)
			
			var plugin_config: ResPlugin = _parse_config_content(config_content, plugin_config_path)
			
			if !_check_plugin_name(plugin_config):
				continue
			
			plugin_config.path = folder_path
			plugins[plugin_config.name] = plugin_config
			added_plugin += 1
			plugin_config.permission_given.connect(
				func():
					plugin_ready.emit(plugin_config.name)
			)
			plugin_config.ask_for_perms()
	
	logger.log("Added %s plugins with success!" % added_plugin)
	logger.log("%s total plugin." % plugins.size())
	plugin_list_reloaded.emit()

func _parse_config_content(config_content: String, plugin_path: String) -> ResPlugin:
	
	if config_content.is_empty():
		return
	
	var config_json_obj = JSON.parse_string(config_content)
	
	if typeof(config_json_obj) == TYPE_NIL:
		return
	
	if !_check_keys(config_json_obj, plugin_path):
		return
	
	var plugin_config: ResPlugin = ResPlugin.new()
	
	plugin_config = _bind_config(config_json_obj, plugin_config)
	
	plugin_config.main_folder = plugin_path.rsplit("/", true, 1)[0]
	plugin_config.main_config = plugin_path
	
	return plugin_config

func _check_keys(json_obj, plugin_path: String):
	
	const NEEDED_KEY = [
		"name",
		"version",
		"main_script"
	]
	
	var missing_key: Array = []
	var empty_key: Array = []
	
	for key in NEEDED_KEY:
		
		if !json_obj.has(key):
			missing_key.push_back(key)
		
		if json_obj[key].is_empty():
			empty_key.push_back(key)
	
	if missing_key.size() != 0:
		logger.error("Plugin config file: \"%s\" doesn't have necessary key(s): %s" % [plugin_path, missing_key])
		return false
	
	if empty_key.size() != 0:
		logger.error("Plugin config file: \"%s\" have necessary key that are empty! %s" % [plugin_path, empty_key])
		return false
	
	return true

func _bind_config(json_obj, plugin_config: ResPlugin):
	
	plugin_config.name = json_obj['name']
	plugin_config.version = json_obj['version']
	plugin_config.main_script = json_obj['main_script']
	
	if json_obj.has('author'):
		plugin_config.author = json_obj['author']
	
	if json_obj.has('description'):
		plugin_config.description = json_obj['description']
	
	if json_obj.has('permissions'):
		plugin_config.permissions_required = _list_permissions(json_obj['permissions'])
	
	if json_obj.has('auto_start'):
		if typeof(json_obj['auto_start']) == TYPE_BOOL:
			plugin_config.auto_start = json_obj['auto_start']
	
	if json_obj.has('icon'):
		if typeof(json_obj['icon']) == TYPE_STRING:
			plugin_config.icon = json_obj['icon']
	
	if json_obj.has('need_plugin'):
		if typeof(json_obj['need_plugin']) == TYPE_ARRAY:
			plugin_config.need_plugin = json_obj['need_plugin']
	
	return plugin_config

func _check_plugin_name(plugin_config: ResPlugin):
	
	if plugin_config.name.is_empty():
		logger.error("Can't load plugin from \"%s\" because the name is empty!" % plugin_config.main_folder)
		return false
	
	if plugins.has(plugin_config.name):
		logger.warn("Can't load plugin \"%s\" because a plugin with the same name already exist!" % plugin_config.name)
		return false
	
	return true

func _init_main_script(plugin_config: ResPlugin):
	
	pass

func HAS(plugin_name: String) -> bool:
	return plugins.has(plugin_name)

func GET(plugin_name: String, default = null):
	
	if !HAS(plugin_name):
		return default
	
	return plugins[plugin_name]

func SET(plugin_name: String, plugin_data: Dictionary):
	plugins[plugin_name] = plugin_data

func NAMES() -> Array:
	
	return plugins.keys()

func LIST() -> Array:
	
	var list: Array = []
	
	for key in NAMES():
		list.push_back(GET(key))
	
	return list

func SIZE() -> int:
	
	return plugins.size()

func _list_permissions(permissions_list: PackedStringArray):
	
	if !PermissionManager.is_valid():
		logger.error("Permission Manager is not valid! Stopping software")
		queue_free()
		return
	
	var PM = PermissionManager.get_singleton()
	var permissions_list_obj: Array[ResPermissionObject] = []
	
	for permission_path: String in permissions_list:
		
		logger.log("Try adding permission: %s" % permission_path)
		
		if PM.HAS(permission_path):
			logger.log("Added permission: %s" % permission_path)
			
			var perm_object: ResPermissionObject = PM.GET(permission_path)
			
			permissions_list_obj.push_back(perm_object)
		
	
	return permissions_list_obj

func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_perm = [
		"GET",
		"SET",
		"HAS",
		"NAMES",
		"LIST",
		"SIZE",
		"VERIFY_PLUGINS"
	]
	
	for perm in possible_perm:
		if _plugin_obj.has_allowed("PLUGIN_MANAGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("PLUGIN_MANAGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_perm.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("PluginManager", lua_obj)

class _lua_class:
	
	var fields = []
	var singleton: PluginManager
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('singleton')
		singleton = PluginManager.get_singleton()
	
	func lua_fields():
		return fields
	
	func is_valid() -> bool:
		return singleton != null
	
	func reload_singleton() -> bool:
		singleton = PluginManager.get_singleton()
		return is_valid()
	
	func GET(plugin_name: String, default = null):
		if !is_valid():
			return default
		
		return singleton.GET(plugin_name, default)
	
	func VERIFY_PLUGINS():
		
		if !is_valid():
			return
		
		singleton.VERIFY_PLUGINS()
	
	func HAS(plugin_name: String) -> bool:
		
		if !is_valid():
			return false
		
		return singleton.HAS(plugin_name)
	
	func SET(plugin_name: String, plugin_data: Dictionary):
		
		if !is_valid():
			return
		
		singleton.SET(plugin_name, plugin_data)
	
	func NAMES() -> Array:
		
		if !is_valid():
			return []
		
		return singleton.NAMES()

	func LIST() -> Array:
		
		if !is_valid():
			return []
		
		return singleton.LIST()
	
	func SIZE() -> int:
		
		if !is_valid():
			return -1
		
		return singleton.SIZE()

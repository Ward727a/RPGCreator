extends Node
class_name DataManager

static var singleton: DataManager

static func get_singleton() -> DataManager:
	return singleton

static func is_valid() -> bool:
	return (DataManager.get_singleton() != null)

var save_folder = "user://data/"
const save_ext = ".rpgcdat"

var data = {}
var logger: Logger = Logger.new("DataManager")

func _init():
	singleton = self
	if Settings.is_valid():
		save_folder = Settings.get_singleton().SettingsData.data_folder
	
	tree_exiting.connect(save_data)

func GET(path: String, default = null, ignore_project: bool = false):
	
	if path.is_empty():
		return default
	
	if !path.begins_with('editor/') and !ignore_project:
		
		if ProjectManager.is_valid():
			path = str(ProjectManager.get_singleton().actual_project.project_id, "/%s" % path)
	
	var path_parts: PackedStringArray = path.split('/')
	
	var _data = data
	
	for part in path_parts:
		
		if _data.has(part):
			_data = _data[part]
			continue
		
		logger.warn("Can't GET %s because %s part doesn't exist!" % [path, part])
		return default
	
	return _data

func HAS(path: String, ignore_project: bool = false) -> bool:
	
	if path.is_empty():
		return false
	
	if !path.begins_with('editor/') and !ignore_project:
		
		if ProjectManager.is_valid():
			path = str(ProjectManager.get_singleton().actual_project.project_id, "/%s" % path)
	
	var path_parts: PackedStringArray = path.split('/')
	
	var _data = data
	
	for part in path_parts:
		
		if _data.has(part):
			_data = _data[part]
			continue
		
		return false
	
	return true

func SET(path: String, object, ignore_project: bool = false):
	
	if path.is_empty():
		logger.error("Can't SET because path is empty!")
		return null
	
	if typeof(object) == TYPE_NIL:
		logger.error("Can't SET %s because object to save is null!" % path)
		return null
	
	if !path.begins_with('editor/') and !ignore_project:
		
		if ProjectManager.is_valid():
			path = str(ProjectManager.get_singleton().actual_project.project_id, "/%s" % path)
	
	var path_parts: PackedStringArray = path.split('/')
	
	var _data = data
	
	for part in path_parts:
		
		if part == path_parts[path_parts.size() - 1]:
			_data[part] = object
			break
		
		if _data.has(part):
			_data = _data[part]
			continue
		
		_data[part] = {}
		_data = _data[part]
	
	return object

func DELETE(path: String, ignore_project: bool = false):
	
	if path.is_empty():
		logger.error("Can't SET because path is empty!")
		return null
	
	if !path.begins_with('editor/') and !ignore_project:
		
		if ProjectManager.is_valid():
			path = str(ProjectManager.get_singleton().actual_project.project_id, "/%s" % path)
	
	var path_parts: PackedStringArray = path.split('/')
	
	var _data = data
	
	for part in path_parts:
		
		if part == path_parts[path_parts.size() - 1]:
			_data.erase(part)
			break
		
		if _data.has(part):
			_data = _data[part]
			continue
		
		_data[part] = {}
		_data = _data[part]

func save_data():
	
	if !DirAccess.dir_exists_absolute(save_folder):
		DirAccess.make_dir_recursive_absolute(save_folder)
	
	for key in data.keys():
		
		var key_folder: String = str(save_folder, key, "/")
		
		if !DirAccess.dir_exists_absolute(key_folder):
			DirAccess.make_dir_recursive_absolute(key_folder)
		
		for child_key in data[key].keys():
			var data_to_save = data[key][child_key]
			var file_path: String = str(key_folder, child_key, save_ext)
			
			var save_file = FileAccess.open(file_path, FileAccess.WRITE)
			save_file.store_string(var_to_str(data_to_save))
			save_file.close()

func load_data():
	
	if !DirAccess.dir_exists_absolute(save_folder):
		DirAccess.make_dir_recursive_absolute(save_folder)
	
	for folder_name in DirAccess.get_directories_at(save_folder):
		
		var main_folder: String = str(save_folder, folder_name, '/')
		if !data.has(folder_name):
			data[folder_name] = {}
		
		for file_name in DirAccess.get_files_at(main_folder):
			var data_key = file_name.split('.')[0]
			var file_path: String = str(main_folder, file_name)
			
			var load_file = FileAccess.open(file_path, FileAccess.READ)
			data[folder_name][data_key] = str_to_var(load_file.get_as_text())
		
			load_file.close()

func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_perm = [
		"GET",
		"SET",
		"HAS",
		"DELETE",
		"save_data",
		"load_data"
	]
	
	for perm in possible_perm:
		if _plugin_obj.has_allowed("DATA_MANAGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("DATA_MANAGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_perm.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("DataManager", lua_obj)

class _lua_class:
	
	var fields = []
	var singleton: DataManager
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('singleton')
		singleton = DataManager.get_singleton()
	
	func lua_fields():
		return fields
	
	func is_valid() -> bool:
		return singleton != null
	
	func reload_singleton() -> bool:
		singleton = DataManager.get_singleton()
		return is_valid()
	
	func GET(path: String, default = null, ignore_project: bool = false):
		singleton.GET(path, default, ignore_project)
	
	func HAS(path: String, ignore_project: bool = false) -> bool:
		return singleton.HAS(path, ignore_project)
	
	func SET(path: String, object, ignore_project: bool = false):
		singleton.SET(path, object, ignore_project)
	
	func DELETE(path: String, ignore_project: bool = false):
		singleton.DELETE(path, ignore_project)
	
	func save_data():
		singleton.save_data()
	
	func load_data():
		singleton.load_data()
	

extends Node
class_name AssetManager

static var singleton: AssetManager

static func get_singleton() -> AssetManager:
	return singleton

static func is_valid() -> bool:
	return (AssetManager.get_singleton() != null)

var assets_data = {}
var assets_folder: String = "user://assets/"

var logger: Logger = Logger.new("AssetManager")

func _init():
	singleton = self
	if Settings.get_singleton() != null:
		assets_folder = Settings.get_singleton().SettingsData.assets_folder


func load_data():
	
	if DataManager.is_valid():
		assets_data = DataManager.get_singleton().GET("editor/assets_data", {})

func GET(path: String = "", default = null):
	
	if path.is_empty():
		return default
	
	var path_parts: PackedStringArray = path.split('/')
	
	var data = assets_data
	
	for part in path_parts:
		
		if data.has(part):
			data = data[part]
			continue
		
		return default
	
	return data

func HAS(path: String = "") -> bool:
	
	if path.is_empty():
		logger.warn("Path for HAS is empty!")
		return false
	
	var path_parts: PackedStringArray = path.split("/")
	
	var data = assets_data
	
	for part in path_parts:
		
		if data.has(part):
			data = data[part]
			continue
		
		return false
	
	return true

func SET(path: String = "", file_path: String = "", to_path: String = "auto") -> bool:
	
	if path.is_empty():
		return false
	
	if file_path.is_empty():
		return false
	
	var file_name: String = file_path.rsplit("/", true, 1)[1]
	var file_ext: String = file_path.split(".")[1]
	var new_name: String = IDGenerator.generate_id(32)
	
	if to_path == "auto":
		to_path = str(assets_folder, new_name, ".", file_ext)
	
	var path_parts: PackedStringArray = path.split('/')
	var last_part: String = path_parts[path_parts.size() - 1]
	
	var data = assets_data
	
	for part in path_parts:
		
		if last_part == part:
			
			var asset = ResAssets.new()
			asset.extension = file_ext
			asset.origin_name = file_name.split(".")[0]
			asset.origin_path = file_path
			asset.target_name = new_name
			asset.target_path = to_path
			
			data[part] = asset
			break
		
		if data.has(part):
			data = data[part]
			continue
		
		data[part] = {}
		data = data[part]
		continue
	
	# TODO add the user project name in the SET path
	DataManager.get_singleton().SET("editor/assets_data", assets_data)
	
	return _save_file(file_path, to_path)

func _has_file(file_path: String) -> bool:
	return FileAccess.file_exists(file_path)

func _open_file(file_path: String) -> FileAccess:
	return FileAccess.open(file_path, FileAccess.READ)

func _save_file(file_path: String, to_path: String) -> bool:
	
	var folder = to_path.rsplit("/", true, 1)[0]
	
	if !DirAccess.dir_exists_absolute(folder):
		DirAccess.make_dir_recursive_absolute(folder)
	
	var err = DirAccess.copy_absolute(file_path, to_path)
	
	if err != OK:
		return false
	
	return true

func link_plugin(_plugin_obj: ResPlugin):
	
	var fields = []
	
	var possible_perm = [
		"GET",
		"HAS",
		"SET",
		"load_data"
	]
	
	for perm in possible_perm:
		if _plugin_obj.has_allowed("ASSETS_MANAGER"):
			continue
		
		if !_plugin_obj.has_allowed(str("ASSETS_MANAGER:%s" % perm)):
			fields.push_back(perm)
	
	if fields.size() < possible_perm.size():
		var lua_obj = _lua_class.new(fields)
		
		_plugin_obj.set_link("AssetsManager", lua_obj)

class _lua_class:
	
	var fields = []
	var singleton: AssetManager
	
	func _init(_fields: Array):
		fields = _fields
		fields.push_back('singleton')
		singleton = AssetManager.get_singleton()
	
	func lua_fields():
		return fields
	
	func is_valid() -> bool:
		return singleton != null
	
	func reload_singleton() -> bool:
		singleton = AssetManager.get_singleton()
		return is_valid()
	
	func GET(path: String = "", default = null):
		if !is_valid():
			return default
		
		return singleton.GET(path, default)
	
	func HAS(path: String = "") -> bool:
		
		if !is_valid():
			return false
		
		return singleton.HAS(path)
	
	func SET(path: String = "", file_path: String = "", to_path: String = "auto") -> bool:
		
		if !is_valid():
			return false
		
		return singleton.SET(path, file_path, to_path)
	
	func load_data():
		
		singleton.load_data()

extends Object
class_name FolderManager

## This function will check if each folders needed by the engine[br]
## are present. It return a dictionnary.[br]
## Dictionary template:[br]
## [codeblock]
## {
## 	"found": ["user://data", "user://plugins"]
## 	"not_found": ["user://projects"]
## }
## [/codeblock]
static func check_engine_folders() -> Dictionary:
	
	var data: Dictionary = {
		"found": [],
		"not_found": []
	}
	
	var folders_to_check: Array = [
		"user://plugins",
		"user://data",
		"user://projects",
		"user://engine"
	]
	
	for folder in folders_to_check:
		
		if DirAccess.dir_exists_absolute(folder):
			data.found.push_back(folder)
		else:
			data.not_found.push_back(folder)

	return data

## This function is linked to the [check_engine_folders]. [br]
## This function will create all folder that wasn't found and return true if it succeed, or false otherwise
static func create_missing_folders(data: Dictionary) -> bool:
	
	if !data.keys().has("not_found"):
		return true
	
	if data['not_found'].size() == 0:
		return true
	
	for folder in data['not_found']:
		var err: Error = DirAccess.make_dir_absolute(folder)
		
		if err != OK:
			push_error(err)
			return false
	
	return true

## Return the url to the folder in a godot type
static func get_plugins() -> String:
	return "user://plugins"

## Return the url to the folder in a normal way (ex: C:/RPG Creator/plugins)
static func get_plugins_global() -> String:
	return ProjectSettings.globalize_path("user://plugins")

## Return the url to the folder in a godot type
static func get_data() -> String:
	return "user://data"

## Return the url to the folder in a normal way (ex: C:/RPG Creator/data)
static func get_data_global() -> String:
	return ProjectSettings.globalize_path("user://data")

## Return the url to the folder in a godot type
static func get_projects() -> String:
	return "user://projects"

## Return the url to the folder in a normal way (ex: C:/RPG Creator/projects)
static func get_projects_global() -> String:
	return ProjectSettings.globalize_path("user://projects")

## Return the url to the folder in a godot type
static func get_engine() -> String:
	return "user://engine"

## Return the url to the folder in a normal way (ex: C:/RPG Creator/engine)
static func get_engine_global() -> String:
	return ProjectSettings.globalize_path("user://engine")

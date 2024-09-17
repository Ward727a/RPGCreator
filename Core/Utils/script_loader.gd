extends Object
class_name ScriptLoader

## This will check if the script is valid
static func is_valid(script_path: String) -> bool:
	
	if !FileAccess.file_exists(script_path):
		return false
	
	if !script_path.ends_with(".gd"):
		return false
	
	return true

## This will try to load the script if it can be loaded
static func load_script(script_path: String) -> GDScript:
	
	if !is_valid(script_path):
		return null
	
	var script: GDScript = load(script_path)
	
	return script

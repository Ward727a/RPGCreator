extends Object
class_name EngineFolder

var folder_path: String = ""
var foldar_path_global: String = ""

var script_path: String = "scripts"
var assets_path: String = "assets"

var SP_skill_path: String = "skills"

var skill_effects_path: String = "effects"
var skill_conditions_path: String = "conditions"

var HealEffect_test

func _init():
	
	# Check if all folder exist
	var data = FolderManager.check_engine_folders()
	
	# Create all missing folders
	FolderManager.create_missing_folders(data)
	
	# Get all path
	folder_path = FolderManager.get_engine()
	foldar_path_global = FolderManager.get_engine_global()
	
	make_paths()

## This function will set all path variables 
func make_paths():
	script_path = make_path(folder_path, script_path)
	assets_path = make_path(folder_path, assets_path)
	
	SP_skill_path = make_path(script_path, SP_skill_path)
	
	skill_effects_path = make_path(SP_skill_path, skill_effects_path)
	skill_conditions_path = make_path(SP_skill_path, skill_conditions_path)
	
	check_folders()

func make_path(from: String, to: String):
	return str(from, "/", to)

## This function will check if all folder are present.[br]
## If not, they will be created automatically!
func check_folders():
	
	# We check only the end connection of each path, so it allow us to skip a lot of if
	if !DirAccess.dir_exists_absolute(skill_conditions_path):
		DirAccess.make_dir_recursive_absolute(skill_conditions_path)
	
	if !DirAccess.dir_exists_absolute(skill_effects_path):
		DirAccess.make_dir_recursive_absolute(skill_effects_path)
	
	if !DirAccess.dir_exists_absolute(assets_path):
		DirAccess.make_dir_recursive_absolute(assets_path)

func get_assets_path() -> String:
	return assets_path

func get_scripts_path() -> String:
	return script_path

func get_skill_path() -> String:
	return SP_skill_path

func get_skill_cond_path() -> String:
	return skill_conditions_path

func get_skill_effects_path() -> String:
	return skill_effects_path

func load_skill_effects():
	
	var access: DirAccess = DirAccess.open(get_skill_effects_path())
	
	if access == null:
		push_error(DirAccess.get_open_error())
		return null
	
	for file in access.get_files():
		
		var file_path: String = make_path(get_skill_effects_path(), file)
		
		print(file_path)
		
		EffectRegister.add_effect(ScriptLoader.load_script(file_path).new())
	
	print(var_to_str(EffectRegister.get_effect("stun_effect")))
	
	print("Loaded effects!")

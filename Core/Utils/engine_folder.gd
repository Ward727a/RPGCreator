extends Object
class_name EngineFolder

var folder_path: String = ""
var foldar_path_global: String = ""

var script_path: String = "scripts"
var assets_path: String = "assets"

var SP_skill_path: String = "skills"
var SP_items_path: String = "items"
var SP_ui_path: String = "UI"

var skill_effects_path: String = "effects"
var skill_conditions_path: String = "conditions"

var item_parameters_path: String = "parameters"

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
	SP_items_path = make_path(script_path, SP_items_path)
	SP_ui_path = make_path(script_path, SP_ui_path)
	
	skill_effects_path = make_path(SP_skill_path, skill_effects_path)
	skill_conditions_path = make_path(SP_skill_path, skill_conditions_path)
	
	item_parameters_path = make_path(SP_items_path, item_parameters_path)
	
	check_folders()

func make_path(from: String, to: String):
	return str(from, "/", to)

## This function will check if all folder are present.[br]
## If not, they will be created automatically!
func check_folders():
	
	# We check only the end connection of each path, so it allow us to skip a lot of if
	if !DirAccess.dir_exists_absolute(SP_ui_path):
		DirAccess.make_dir_recursive_absolute(SP_ui_path)
	
	if !DirAccess.dir_exists_absolute(skill_conditions_path):
		DirAccess.make_dir_recursive_absolute(skill_conditions_path)
	
	if !DirAccess.dir_exists_absolute(skill_effects_path):
		DirAccess.make_dir_recursive_absolute(skill_effects_path)
	
	if !DirAccess.dir_exists_absolute(item_parameters_path):
		DirAccess.make_dir_recursive_absolute(item_parameters_path)
	
	if !DirAccess.dir_exists_absolute(assets_path):
		DirAccess.make_dir_recursive_absolute(assets_path)

func get_assets_path() -> String:
	return assets_path

func get_scripts_path() -> String:
	return script_path

func get_skill_path() -> String:
	return SP_skill_path

func get_items_path() -> String:
	return SP_items_path

func get_ui_path() -> String:
	return SP_ui_path

func get_skill_cond_path() -> String:
	return skill_conditions_path

func get_skill_effects_path() -> String:
	return skill_effects_path

func get_item_parameters_path() -> String:
	return item_parameters_path

func load_skill_effects():
	
	var access: DirAccess = DirAccess.open(get_skill_effects_path())
	
	if access == null:
		push_error(DirAccess.get_open_error())
		return null
	
	for file in access.get_files():
		
		var file_path: String = make_path(get_skill_effects_path(), file)
		
		print(file_path)
		
		var effect = BaseEffect.new(file_path)
		
		if effect == null:
			push_error("Couldn't create the effect: '",file_path,"'!")
			continue
		
		EffectRegister.set_effect(effect.id, effect)
	
	print(LuaLoader.lua_scripts)
	
	print("Loaded effects!")

func load_skill_conditions():
	
	var access: DirAccess = DirAccess.open(get_skill_cond_path())
	
	if access == null:
		push_error(DirAccess.get_open_error())
		return null
	
	for file in access.get_files():
		
		var file_path: String = make_path(get_skill_cond_path(), file)
		
		print(file_path)
		
		var condition = BaseSkillCondition.new(file_path)
		
		if condition == null:
			push_error("Couldn't create the condition: '",file_path,"'!")
			continue
		
		SkillConditionsRegister.set_condition(condition.id, condition)
	
	print(LuaLoader.lua_scripts)
	
	print("Loaded conditions!")

func load_items_parameters():
	
	var access: DirAccess = DirAccess.open(get_item_parameters_path())
	
	if access == null:
		push_error(DirAccess.get_open_error())
		return null
	
	for file in access.get_files():
		
		var file_path: String = make_path(get_item_parameters_path(), file)
		
		if !file_path.ends_with('.lua'):
			continue
		
		var lua_loader: LuaLoader = LuaLoader.new(file_path)
		
		var err = lua_loader.load_lua([], true)
		
		if !err.success:
			if err.has('load_function_missing'):
				print('[LuaLoader - WARNING] The lua file %s doesn\'t have the loaded or unloaded function, so it can\'t be used for item parameters!' % file_path)
				continue

func load_ui():
	
	var access: DirAccess = DirAccess.open(get_ui_path())
	
	if access == null:
		push_error(DirAccess.get_open_error())
		return null
	
	for file in access.get_files():
		
		var file_path: String = make_path(get_ui_path(), file)
		
		if !file_path.ends_with('.lua'):
			continue
		
		var lua_loader: LuaLoader = LuaLoader.new(file_path)
		
		var err = lua_loader.load_lua([], true)
		
		if !err.success:
			if err.has('load_function_missing'):
				print('[LuaLoader - WARNING] The lua file %s doesn\'t have the loaded or unloaded function, so it can\'t be used for UI!' % file_path)
				continue

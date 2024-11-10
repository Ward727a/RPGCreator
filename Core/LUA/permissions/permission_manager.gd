extends Node
class_name PermissionManager

static var singleton: PermissionManager

static func get_singleton() -> PermissionManager:
	return singleton

static func is_valid() -> bool:
	return (PermissionManager.get_singleton() != null)

var DM: DataManager
var logger: Logger = Logger.new("PermissionManager")

enum LEVEL {
	LOW,
	MID,
	HIGH
}

func _init():
	singleton = self
	_check_permission_table()

func _check_permission_table():
	
	if !DataManager.is_valid():
		logger.error("Can't access DataManager!")
		queue_free()
		return
	
	if !Settings.is_valid():
		logger.error("Can't access the Settings!")
		queue_free()
		return
	
	DM = DataManager.get_singleton()
	
	if !DM.HAS('editor/permission_table/_info', true):
		logger.warning("No permission_table/_info found in the DM, creating the table.")
		_create_table()
		return
	
	var table_info = DM.GET('editor/permission_table/_info')
	
	if table_info != null:
		if table_info.version == Settings.get_singleton().SoftwareInfo.version:
			logger.log("Permission_table info has same version as the software. Cancel the creation of the table.")
			return
		else:
			logger.warning("Permission_table info is outdated, recreating the table.")
			_create_table()
			return
	
	logger.warning("Permission_table info is null or can't be found, recreating the table.")
	_create_table()

func _create_table():
	
	_reset_table()
	_create_table_info()
	
	# Here we have all the permission that will be applied to the table
	# TODO add each permissions here
	
	#=======================================================#
	#=================== Plugin manager ====================#
	#=======================================================#
	_to_table("PLUGIN_MANAGER", LEVEL.HIGH)
	_to_table("PLUGIN_MANAGER:GET", LEVEL.HIGH)
	_to_table("PLUGIN_MANAGER:SET", LEVEL.HIGH)
	_to_table("PLUGIN_MANAGER:HAS", LEVEL.HIGH)
	_to_table("PLUGIN_MANAGER:NAMES", LEVEL.HIGH)
	_to_table("PLUGIN_MANAGER:LIST", LEVEL.HIGH)
	_to_table("PLUGIN_MANAGER:SIZE", LEVEL.HIGH)
	_to_table("PLUGIN_MANAGER:VERIFY_PLUGINS", LEVEL.HIGH)
	
	#=======================================================#
	#=================== Project manager ===================#
	#=======================================================#
	_to_table("PROJECT_MANAGER", LEVEL.HIGH)
	_to_table("PROJECT_MANAGER:CREATE", LEVEL.HIGH)
	_to_table("PROJECT_MANAGER:LOAD", LEVEL.HIGH)
	_to_table("PROJECT_MANAGER:SAVE", LEVEL.HIGH)
	
	#=======================================================#
	#======================== Logger =======================#
	#=======================================================#
	_to_table("LOGGER", LEVEL.LOW, "Allow the plugin to log message in the logger (log, warn, error, debug)")
	_to_table("LOGGER:log", LEVEL.LOW, "This allow the plugin to log message (log)")
	_to_table("LOGGER:warn", LEVEL.LOW, "This allow the plugin to log message (warn)")
	_to_table("LOGGER:warning", LEVEL.LOW, "This allow the plugin to log message (warning)")
	_to_table("LOGGER:debug", LEVEL.LOW, "This allow the plugin to log message (debug)")
	_to_table("LOGGER:error", LEVEL.LOW, "This allow the plugin to log message (error)")
	
	#=======================================================#
	#==================== Events Manager ===================#
	#=======================================================#
	_to_table("EVENTS_MANAGER", LEVEL.MID)
	_to_table("EVENTS_MANAGER:GET", LEVEL.MID)
	_to_table("EVENTS_MANAGER:SET", LEVEL.MID)
	_to_table("EVENTS_MANAGER:CALL", LEVEL.MID)
	
	#=======================================================#
	#===================== Data Manager ====================#
	#=======================================================#
	_to_table("DATA_MANAGER", LEVEL.MID)
	_to_table("DATA_MANAGER:GET", LEVEL.MID)
	_to_table("DATA_MANAGER:SET", LEVEL.MID)
	_to_table("DATA_MANAGER:HAS", LEVEL.MID)
	_to_table("DATA_MANAGER:DELETE", LEVEL.MID)
	_to_table("DATA_MANAGER:save_data", LEVEL.MID)
	_to_table("DATA_MANAGER:load_data", LEVEL.MID)
	
	#=======================================================#
	#==================== Assets Manager ===================#
	#=======================================================#
	_to_table("ASSETS_MANAGER", LEVEL.MID)
	_to_table("ASSETS_MANAGER:GET", LEVEL.MID)
	_to_table("ASSETS_MANAGER:HAS", LEVEL.MID)
	_to_table("ASSETS_MANAGER:SET", LEVEL.MID)
	_to_table("ASSETS_MANAGER:load_data", LEVEL.MID)
	
	#=======================================================#
	#==================== Theme Manager ====================#
	#=======================================================#
	_to_table("THEME_MANAGER", LEVEL.LOW)
	_to_table("THEME_MANAGER:add_theme", LEVEL.LOW)
	_to_table("THEME_MANAGER:set_theme", LEVEL.LOW)
	_to_table("THEME_MANAGER:get_theme", LEVEL.LOW)
	_to_table("THEME_MANAGER:remove_theme", LEVEL.LOW)
	_to_table("THEME_MANAGER:has_theme", LEVEL.LOW)
	
	#=======================================================#
	#==================== Godot Manager ====================#
	#=======================================================#
	_to_table("GD:INTERFACE", LEVEL.LOW)
	_to_table("GD:STYLE", LEVEL.LOW)
	_to_table("GD:TEXTURE", LEVEL.LOW)
	_to_table("GD:MATERIAL", LEVEL.LOW)
	_to_table("GD:GLOBAL", LEVEL.LOW)
	
	#=======================================================#
	#==================== File Utility =====================#
	#=======================================================#
	_to_table("FILE", LEVEL.HIGH)
	_to_table("FILE:ALL_FILES", LEVEL.HIGH)
	_to_table("FILE:READ", LEVEL.LOW)
	_to_table("FILE:WRITE", LEVEL.LOW)
	_to_table("FILE:READ_COMPRESS", LEVEL.LOW)
	_to_table("FILE:WRITE_COMPRESS", LEVEL.LOW)
	_to_table("FILE:READ_ENCRYPT", LEVEL.MID)
	_to_table("FILE:WRITE_ENCRYPT", LEVEL.MID)
	
	_to_table("INTERFACE_MANAGER", LEVEL.LOW)
	_to_table("HELPER", LEVEL.HIGH)
	_to_table("HELPER:OPEN_LINK", LEVEL.HIGH)
	
	pass

func _reset_table():
	logger.log("Deleting permission table...")
	DM.DELETE("editor/permission_table", true)

func _create_table_info():
	
	var table_info = {
		"version": Settings.get_singleton().SoftwareInfo.version
	}
	
	DM.SET("editor/permission_table/_info", table_info, true)

func _to_table(permission_path: String, level: LEVEL, description: String = ""):
	
	logger.log("Add permission: %s" % permission_path)
	
	var permission: ResPermissionObject = ResPermissionObject.new()
	
	permission.path = permission_path
	permission.level = level
	permission.description = description
	
	DM.SET(str("editor/permission_table/%s" % permission_path), permission, true)

func GET(permission_path: String) -> ResPermissionObject:
	
	return DM.GET("editor/permission_table/%s" % permission_path, null, true)

func HAS(permission_path: String) -> bool:
	
	return DM.HAS("editor/permission_table/%s" % permission_path, true)

func FORCE_RESET():
	logger.log("Force reset launch. Reseting the table to the default value.")
	_create_table()
